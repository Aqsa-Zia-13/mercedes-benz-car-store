using Microsoft.Data.SqlClient;
using System.Data;

namespace Car.Database
{
    public class ManagerRepository
    {
        public static List<QueryItem> GetAllQueries()
        {
            var list = new List<QueryItem>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            const string sql = @"
                SELECT q.QueryId, q.Subject, q.Message,
                       q.Status, ISNULL(q.ManagerReply,'') AS ManagerReply,
                       q.CreatedAt, u.FullName AS CustomerName
                FROM   Queries q
                JOIN   Users   u ON q.UserId = u.UserId
                ORDER  BY q.CreatedAt DESC";
            using var cmd = new SqlCommand(sql, conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new QueryItem
                {
                    QueryId = r.GetInt32("QueryId"),
                    Subject = r.GetString("Subject"),
                    Message = r.GetString("Message"),
                    Status = r.GetString("Status"),
                    ManagerReply = r.GetString("ManagerReply"),
                    CreatedAt = r.GetDateTime("CreatedAt"),
                    CustomerName = r.GetString("CustomerName")
                });
            return list;
        }

        public static void ReplyToQuery(int queryId, string reply)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            const string sql = @"
                UPDATE Queries
                SET    ManagerReply = @R,
                       Status       = 'Resolved',
                       ResolvedAt   = GETDATE()
                WHERE  QueryId = @Id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@R", reply);
            cmd.Parameters.AddWithValue("@Id", queryId);
            cmd.ExecuteNonQuery();
        }
    }

    public class QueryItem
    {
        public int QueryId { get; set; }
        public string Subject { get; set; } = "";
        public string Message { get; set; } = "";
        public string Status { get; set; } = "";
        public string ManagerReply { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string CustomerName { get; set; } = "";
    }
}