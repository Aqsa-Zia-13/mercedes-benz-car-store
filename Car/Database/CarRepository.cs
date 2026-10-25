using Car.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Car.Database
{
    public class CarRepository
    {
        // ── GET ALL AVAILABLE CARS (with optional search) ──────
        public static List<Car.Models.Car> GetAvailableCars(
            string search = "", string fuelFilter = "", string sortBy = "Name")
        {
            var list = new List<Car.Models.Car>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            string sql = @"
                SELECT CarId, Name, Model, Year, Price, Color,
                       FuelType, Mileage, IsAvailable,
                       ISNULL(ImagePath,'')   AS ImagePath,
                       ISNULL(Description,'') AS Description,
                       AddedAt,
                       ISNULL(CarStatus,'Available') AS CarStatus,
                       ISNULL(StatusNote,'')  AS StatusNote,
                       ISNULL(StockCount,1)   AS StockCount,
                       StatusUpdatedBy, StatusUpdatedAt
                FROM   Cars
                WHERE  IsAvailable = 1
                  AND (@Search = '' OR Name  LIKE '%'+@Search+'%'
                                    OR Model LIKE '%'+@Search+'%'
                                    OR Color LIKE '%'+@Search+'%')
                  AND (@Fuel   = '' OR FuelType = @Fuel)
                ORDER BY " + sortBy;

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Search", search);
            cmd.Parameters.AddWithValue("@Fuel", fuelFilter);

            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(MapCar(r));
            return list;
        }

        // ── GET SINGLE CAR ──────────────────────────────────────
        public static Car.Models.Car? GetById(int carId)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            const string sql = @"
                SELECT CarId, Name, Model, Year, Price, Color,
                       FuelType, Mileage, IsAvailable,
                       ISNULL(ImagePath,'')   AS ImagePath,
                       ISNULL(Description,'') AS Description,
                       AddedAt,
                       ISNULL(CarStatus,'Available') AS CarStatus,
                       ISNULL(StatusNote,'')  AS StatusNote,
                       ISNULL(StockCount,1)   AS StockCount,
                       StatusUpdatedBy, StatusUpdatedAt
                FROM   Cars
                WHERE  CarId = @Id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", carId);
            using var r = cmd.ExecuteReader();
            return r.Read() ? MapCar(r) : null;
        }

        // ── CART ────────────────────────────────────────────────
        public static void AddToCart(int userId, int carId)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            const string check =
                "SELECT COUNT(1) FROM Cart WHERE UserId=@U AND CarId=@C";
            using var chkCmd = new SqlCommand(check, conn);
            chkCmd.Parameters.AddWithValue("@U", userId);
            chkCmd.Parameters.AddWithValue("@C", carId);
            if ((int)chkCmd.ExecuteScalar() > 0) return;

            const string sql = "INSERT INTO Cart(UserId,CarId) VALUES(@U,@C)";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@U", userId);
            cmd.Parameters.AddWithValue("@C", carId);
            cmd.ExecuteNonQuery();
        }

        public static void RemoveFromCart(int userId, int carId)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            const string sql =
                "DELETE FROM Cart WHERE UserId=@U AND CarId=@C";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@U", userId);
            cmd.Parameters.AddWithValue("@C", carId);
            cmd.ExecuteNonQuery();
        }

        public static List<Car.Models.Car> GetCartItems(int userId)
        {
            var list = new List<Car.Models.Car>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            const string sql = @"
                SELECT c.CarId, c.Name, c.Model, c.Year, c.Price, c.Color,
                       c.FuelType, c.Mileage, c.IsAvailable,
                       ISNULL(c.ImagePath,'')   AS ImagePath,
                       ISNULL(c.Description,'') AS Description,
                       c.AddedAt,
                       ISNULL(c.CarStatus,'Available') AS CarStatus,
                       ISNULL(c.StatusNote,'')  AS StatusNote,
                       ISNULL(c.StockCount,1)   AS StockCount,
                       c.StatusUpdatedBy, c.StatusUpdatedAt
                FROM   Cart ct
                JOIN   Cars c ON ct.CarId = c.CarId
                WHERE  ct.UserId = @U";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@U", userId);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(MapCar(r));
            return list;
        }

        public static int GetCartCount(int userId)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "SELECT COUNT(1) FROM Cart WHERE UserId=@U", conn);
            cmd.Parameters.AddWithValue("@U", userId);
            return (int)cmd.ExecuteScalar();
        }

        // ── PLACE ORDER ─────────────────────────────────────────
        public static int PlaceOrder(int userId, int carId,
            decimal amount, string paymentMethod)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            string receipt = "MB-" + DateTime.Now.ToString("yyyyMMddHHmmss")
                             + "-" + userId;

            const string sql = @"
                INSERT INTO Orders
                    (UserId,CarId,TotalAmount,PaymentMethod,Status,ReceiptNumber)
                OUTPUT INSERTED.OrderId
                VALUES (@U,@C,@A,@PM,'Completed',@R)";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@U", userId);
            cmd.Parameters.AddWithValue("@C", carId);
            cmd.Parameters.AddWithValue("@A", amount);
            cmd.Parameters.AddWithValue("@PM", paymentMethod);
            cmd.Parameters.AddWithValue("@R", receipt);
            int orderId = (int)cmd.ExecuteScalar();

            // Reduce stock by 1; mark unavailable if stock reaches 0
            using var upd = new SqlCommand(@"
                UPDATE Cars
                SET StockCount  = CASE WHEN StockCount > 0
                                       THEN StockCount - 1 ELSE 0 END,
                    IsAvailable = CASE WHEN StockCount - 1 > 0 THEN 1 ELSE 0 END
                WHERE CarId = @C", conn);
            upd.Parameters.AddWithValue("@C", carId);
            upd.ExecuteNonQuery();

            // Remove from cart
            using var del = new SqlCommand(
                "DELETE FROM Cart WHERE UserId=@U AND CarId=@C", conn);
            del.Parameters.AddWithValue("@U", userId);
            del.Parameters.AddWithValue("@C", carId);
            del.ExecuteNonQuery();

            return orderId;
        }

        // ── GET ORDER BY ID (with ReceiptPath) ──────────────────
        public static Order? GetOrderById(int orderId)
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
                       u.PhoneNumber AS CustomerPhone,
                       u.Username    AS CustomerUsername
                FROM   Orders o
                JOIN   Cars   c ON o.CarId  = c.CarId
                JOIN   Users  u ON o.UserId = u.UserId
                WHERE  o.OrderId = @Id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", orderId);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;
            return new Order
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
                CustomerPhone = r["CustomerPhone"] as string ?? "",
                CustomerUsername = r.GetString("CustomerUsername")
            };
        }

        // ── GET MY ORDERS (Customer) ────────────────────────────
        public static List<Order> GetMyOrders(int userId)
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
                       u.PhoneNumber AS CustomerPhone,
                       u.Username    AS CustomerUsername
                FROM   Orders o
                JOIN   Cars  c ON o.CarId  = c.CarId
                JOIN   Users u ON o.UserId = u.UserId
                WHERE  o.UserId = @U
                ORDER  BY o.OrderDate DESC";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@U", userId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Order
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
                    CustomerPhone = r["CustomerPhone"] as string ?? "",
                    CustomerUsername = r.GetString("CustomerUsername")
                });
            return list;
        }

        // ── QUERIES ─────────────────────────────────────────────
        public static void SubmitQuery(int userId, string subject, string message)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            const string sql = @"
                INSERT INTO Queries (UserId, Subject, Message)
                VALUES (@U, @S, @M)";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@U", userId);
            cmd.Parameters.AddWithValue("@S", subject);
            cmd.Parameters.AddWithValue("@M", message);
            cmd.ExecuteNonQuery();
        }

        public static List<(int QueryId, string Subject, string Message,
            string Status, string Reply, DateTime CreatedAt)>
            GetMyQueries(int userId)
        {
            var list = new List<(int, string, string, string, string, DateTime)>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            const string sql = @"
                SELECT QueryId, Subject, Message, Status,
                       ISNULL(ManagerReply,'') AS ManagerReply,
                       CreatedAt
                FROM   Queries
                WHERE  UserId = @U
                ORDER  BY CreatedAt DESC";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@U", userId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add((
                    r.GetInt32("QueryId"),
                    r.GetString("Subject"),
                    r.GetString("Message"),
                    r.GetString("Status"),
                    r.GetString("ManagerReply"),
                    r.GetDateTime("CreatedAt")
                ));
            return list;
        }

        // ── MAPPER ──────────────────────────────────────────────
        private static Car.Models.Car MapCar(SqlDataReader r) => new()
        {
            CarId = r.GetInt32("CarId"),
            Name = r.GetString("Name"),
            Model = r.GetString("Model"),
            Year = r.GetInt32("Year"),
            Price = r.GetDecimal("Price"),
            Color = r.GetString("Color"),
            FuelType = r.GetString("FuelType"),
            Mileage = r.GetInt32("Mileage"),
            IsAvailable = r.GetBoolean("IsAvailable"),
            ImagePath = r.GetString("ImagePath"),
            Description = r.GetString("Description"),
            AddedAt = r.GetDateTime("AddedAt"),
            CarStatus = r.GetString("CarStatus"),
            StatusNote = r.GetString("StatusNote"),
            StockCount = r.GetInt32("StockCount"),
            StatusUpdatedBy = r["StatusUpdatedBy"] as int?,
            StatusUpdatedAt = r["StatusUpdatedAt"] as DateTime?
        };
    }
}