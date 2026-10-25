using Car.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Car.Database
{
    public class CarInventoryRepository
    {
        // ── Get ALL cars (all statuses) ────────────────────────
        public static List<Car.Models.Car> GetAllCars()
        {
            var list = new List<Car.Models.Car>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            const string sql = @"
                SELECT CarId, Name, Model, Year, Price, Color, FuelType,
                       Mileage, IsAvailable, ISNULL(ImagePath,'') AS ImagePath,
                       ISNULL(Description,'') AS Description, AddedAt,
                       ISNULL(CarStatus,'Available') AS CarStatus,
                       ISNULL(StatusNote,'')  AS StatusNote,
                       ISNULL(StockCount,1)   AS StockCount,
                       StatusUpdatedBy, StatusUpdatedAt
                FROM   Cars
                ORDER  BY Name";
            using var cmd = new SqlCommand(sql, conn);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(MapCar(r));
            return list;
        }

        // ── Get cars that need attention (Admin flags) ─────────
        public static List<Car.Models.Car> GetFlaggedCars()
        {
            var list = new List<Car.Models.Car>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            const string sql = @"
                SELECT CarId, Name, Model, Year, Price, Color, FuelType,
                       Mileage, IsAvailable, ISNULL(ImagePath,'') AS ImagePath,
                       ISNULL(Description,'') AS Description, AddedAt,
                       ISNULL(CarStatus,'Available') AS CarStatus,
                       ISNULL(StatusNote,'')  AS StatusNote,
                       ISNULL(StockCount,1)   AS StockCount,
                       StatusUpdatedBy, StatusUpdatedAt
                FROM   Cars
                WHERE  CarStatus IN ('Needs Repair','Needs Restock')
                ORDER  BY CarStatus, Name";
            using var cmd = new SqlCommand(sql, conn);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(MapCar(r));
            return list;
        }

        // ── Admin: update car status + note ───────────────────
        public static void UpdateCarStatus(int carId, string status,
            string note, int updatedByUserId)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            const string sql = @"
                UPDATE Cars
                SET    CarStatus       = @Status,
                       StatusNote      = @Note,
                       StatusUpdatedBy = @By,
                       StatusUpdatedAt = GETDATE()
                WHERE  CarId = @Id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@Note", note);
            cmd.Parameters.AddWithValue("@By", updatedByUserId);
            cmd.Parameters.AddWithValue("@Id", carId);
            cmd.ExecuteNonQuery();
        }

        // ── SuperAdmin: update stock count ────────────────────
        public static void UpdateStock(int carId, int newStock, int updatedBy)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            // If stock > 0 mark available, if 0 mark unavailable
            const string sql = @"
                UPDATE Cars
                SET    StockCount      = @Stock,
                       IsAvailable     = CASE WHEN @Stock > 0 THEN 1 ELSE 0 END,
                       CarStatus       = CASE WHEN @Stock > 0
                                              THEN 'Available'
                                              ELSE CarStatus END,
                       StatusUpdatedBy = @By,
                       StatusUpdatedAt = GETDATE()
                WHERE  CarId = @Id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Stock", newStock);
            cmd.Parameters.AddWithValue("@By", updatedBy);
            cmd.Parameters.AddWithValue("@Id", carId);
            cmd.ExecuteNonQuery();
        }

        // ── SuperAdmin: resolve a flagged car ─────────────────
        public static void ResolveFlaggedCar(int carId, int resolvedBy)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            const string sql = @"
                UPDATE Cars
                SET    CarStatus       = 'Available',
                       StatusNote      = NULL,
                       StatusUpdatedBy = @By,
                       StatusUpdatedAt = GETDATE()
                WHERE  CarId = @Id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@By", resolvedBy);
            cmd.Parameters.AddWithValue("@Id", carId);
            cmd.ExecuteNonQuery();
        }

        // ── Add new car ───────────────────────────────────────
        public static void AddCar(Car.Models.Car car)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            const string sql = @"
                INSERT INTO Cars
                    (Name,Model,Year,Price,Color,FuelType,
                     Mileage,IsAvailable,Description,CarStatus,StockCount)
                VALUES
                    (@N,@Mo,@Y,@P,@C,@F,@Mi,1,@D,'Available',@St)";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@N", car.Name);
            cmd.Parameters.AddWithValue("@Mo", car.Model);
            cmd.Parameters.AddWithValue("@Y", car.Year);
            cmd.Parameters.AddWithValue("@P", car.Price);
            cmd.Parameters.AddWithValue("@C", car.Color);
            cmd.Parameters.AddWithValue("@F", car.FuelType);
            cmd.Parameters.AddWithValue("@Mi", car.Mileage);
            cmd.Parameters.AddWithValue("@D", car.Description);
            cmd.Parameters.AddWithValue("@St", car.StockCount);
            cmd.ExecuteNonQuery();
        }

        // ── Delete car ────────────────────────────────────────
        public static void DeleteCar(int carId)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "DELETE FROM Cars WHERE CarId=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", carId);
            cmd.ExecuteNonQuery();
        }

        // ── Inventory report data ─────────────────────────────
        public static (int Total, int Available,
                       int NeedsRepair, int NeedsRestock)
            GetInventoryStats()
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            const string sql = @"
                SELECT
                    COUNT(*)                                      AS Total,
                    SUM(CASE WHEN CarStatus='Available'     THEN 1 ELSE 0 END),
                    SUM(CASE WHEN CarStatus='Needs Repair'  THEN 1 ELSE 0 END),
                    SUM(CASE WHEN CarStatus='Needs Restock' THEN 1 ELSE 0 END)
                FROM Cars";
            using var cmd = new SqlCommand(sql, conn);
            using var r = cmd.ExecuteReader();
            r.Read();
            return (r.GetInt32(0), r.GetInt32(1),
                    r.GetInt32(2), r.GetInt32(3));
        }

        // ── Customer activity report ──────────────────────────
        public static List<(string Name, string Email,
                            int Orders, decimal Spent)>
            GetCustomerActivity()
        {
            var list = new List<(string, string, int, decimal)>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            const string sql = @"
                SELECT u.FullName, u.Email,
                       COUNT(o.OrderId)       AS Orders,
                       ISNULL(SUM(o.TotalAmount),0) AS Spent
                FROM   Users u
                LEFT JOIN Orders o ON u.UserId = o.UserId
                WHERE  u.UserRole = 'Customer'
                GROUP  BY u.FullName, u.Email
                ORDER  BY Spent DESC";
            using var cmd = new SqlCommand(sql, conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add((r.GetString(0), r.GetString(1),
                          r.GetInt32(2), r.GetDecimal(3)));
            return list;
        }

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