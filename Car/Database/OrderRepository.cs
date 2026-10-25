using Car.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Car.Database
{
    public class OrderRepository
    {
        public static void SaveReceiptPath(int orderId, string path)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "UPDATE Orders SET ReceiptPath=@P WHERE OrderId=@Id", conn);
            cmd.Parameters.AddWithValue("@P", path);
            cmd.Parameters.AddWithValue("@Id", orderId);
            cmd.ExecuteNonQuery();
        }

        public static List<Order> GetAllOrders()
        {
            var list = new List<Order>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            const string sql = @"
                SELECT o.OrderId, o.UserId, o.CarId, o.OrderDate,
                       o.TotalAmount, o.PaymentMethod, o.Status,
                       o.ReceiptNumber,
                       ISNULL(o.ReceiptPath,'') AS ReceiptPath,
                       c.Name  AS CarName,
                       c.Model AS CarModel,
                       c.Year  AS CarYear,
                       u.FullName    AS CustomerName,
                       u.Email       AS CustomerEmail,
                       ISNULL(u.PhoneNumber,'') AS CustomerPhone,
                       u.Username    AS CustomerUsername
                FROM   Orders o
                JOIN   Cars   c ON o.CarId  = c.CarId
                JOIN   Users  u ON o.UserId = u.UserId
                ORDER  BY o.OrderDate DESC";
            using var cmd = new SqlCommand(sql, conn);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(MapOrder(r));
            return list;
        }

        public static Order? GetById(int orderId)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            const string sql = @"
                SELECT o.OrderId, o.UserId, o.CarId, o.OrderDate,
                       o.TotalAmount, o.PaymentMethod, o.Status,
                       o.ReceiptNumber,
                       ISNULL(o.ReceiptPath,'') AS ReceiptPath,
                       c.Name  AS CarName,
                       c.Model AS CarModel,
                       c.Year  AS CarYear,
                       u.FullName    AS CustomerName,
                       u.Email       AS CustomerEmail,
                       ISNULL(u.PhoneNumber,'') AS CustomerPhone,
                       u.Username    AS CustomerUsername
                FROM   Orders o
                JOIN   Cars   c ON o.CarId  = c.CarId
                JOIN   Users  u ON o.UserId = u.UserId
                WHERE  o.OrderId = @Id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", orderId);
            using var r = cmd.ExecuteReader();
            return r.Read() ? MapOrder(r) : null;
        }








        //public static (decimal TotalRevenue, int TotalOrders,
        //               int TotalCustomers, int TotalCars)
        //    GetDashboardStats()
        //{
        //    using var conn = DatabaseHelper.GetConnection();
        //    conn.Open();
        //    const string sql = @"
        //        SELECT
        //            (SELECT ISNULL(SUM(TotalAmount),0)
        //             FROM Orders WHERE Status='Completed') AS Revenue,
        //            (SELECT COUNT(*) FROM Orders)          AS Orders,
        //            (SELECT COUNT(*) FROM Users
        //             WHERE UserRole='Customer')            AS Customers,
        //            (SELECT COUNT(*) FROM Cars)            AS Cars";
        //    using var cmd = new SqlCommand(sql, conn);
        //    using var r = cmd.ExecuteReader();
        //    r.Read();
        //    return (r.GetDecimal(0), r.GetInt32(1),
        //            r.GetInt32(2), r.GetInt32(3));
        //}



        public static (decimal TotalRevenue, int TotalOrders,
               int TotalCustomers, int TotalCars)
    GetDashboardStats()
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            decimal revenue = 0;
            int orders = 0;
            int customers = 0;
            int cars = 0;

            using (var cmd = new SqlCommand(
                "SELECT ISNULL(SUM(TotalAmount),0) FROM Orders " +
                "WHERE Status='Completed'", conn))
                revenue = (decimal)cmd.ExecuteScalar();

            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Orders", conn))
                orders = (int)cmd.ExecuteScalar();

            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Users WHERE UserRole='Customer'", conn))
                customers = (int)cmd.ExecuteScalar();

            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Cars", conn))
                cars = (int)cmd.ExecuteScalar();

            return (revenue, orders, customers, cars);
        }










        public static List<Order> GetOrdersForReport(
            DateTime? from, DateTime? to)
        {
            var list = new List<Order>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            const string sql = @"
                SELECT o.OrderId, o.UserId, o.CarId, o.OrderDate,
                       o.TotalAmount, o.PaymentMethod, o.Status,
                       o.ReceiptNumber,
                       ISNULL(o.ReceiptPath,'') AS ReceiptPath,
                       c.Name  AS CarName,
                       c.Model AS CarModel,
                       c.Year  AS CarYear,
                       u.FullName    AS CustomerName,
                       u.Email       AS CustomerEmail,
                       ISNULL(u.PhoneNumber,'') AS CustomerPhone,
                       u.Username    AS CustomerUsername
                FROM   Orders o
                JOIN   Cars  c ON o.CarId  = c.CarId
                JOIN   Users u ON o.UserId = u.UserId
                WHERE  (@From IS NULL OR o.OrderDate >= @From)
                  AND  (@To   IS NULL OR o.OrderDate <  @To)
                ORDER  BY o.OrderDate DESC";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@From",
                from.HasValue ? (object)from.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@To",
                to.HasValue ? (object)to.Value.AddDays(1) : DBNull.Value);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(MapOrder(r));
            return list;
        }

        private static Order MapOrder(SqlDataReader r) => new()
        {
            OrderId = r.GetInt32("OrderId"),
            UserId = r.GetInt32("UserId"),
            CarId = r.GetInt32("CarId"),
            OrderDate = r.GetDateTime("OrderDate"),
            TotalAmount = r.GetDecimal("TotalAmount"),
            PaymentMethod = r.GetString("PaymentMethod"),
            Status = r.GetString("Status"),
            ReceiptNumber = r["ReceiptNumber"] as string ?? "",
            ReceiptPath = r["ReceiptPath"] as string ?? "",
            CarName = r.GetString("CarName"),
            CarModel = r.GetString("CarModel"),
            CarYear = r.GetInt32("CarYear"),
            CustomerName = r.GetString("CustomerName"),
            CustomerEmail = r.GetString("CustomerEmail"),
            CustomerPhone = r.GetString("CustomerPhone"),
            CustomerUsername = r.GetString("CustomerUsername")
        };
    }
}


