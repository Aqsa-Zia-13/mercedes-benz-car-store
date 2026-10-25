using System.Data;
using Microsoft.Data.SqlClient;

namespace Car.Database
{
    public class DatabaseHelper
    {
        // ⚠️ Change YOUR_SERVER_NAME to your SQL Server instance
        // e.g. ".\SQLEXPRESS"  or  "localhost"  or  "DESKTOP-ABC\SQLEXPRESS"
        private static readonly string ConnectionString =
            "Server=ALIZIAPC2025\\SQLEXPRESS;Database=MercedesBenzStore;" +
            "Trusted_Connection=True;TrustServerCertificate=True;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public static bool TestConnection()
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
