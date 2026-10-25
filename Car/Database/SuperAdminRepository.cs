using Car.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Car.Database
{
    public class SuperAdminRepository
    {
        public static List<User> GetAllUsers()
        {
            var list = new List<User>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            const string sql = @"
                SELECT UserId, FullName, Username, Email,
                       ISNULL(PhoneNumber,'') AS PhoneNumber,
                       ISNULL(Address,'') AS Address,
                       IsActive, CreatedAt, UserRole
                FROM   Users
                ORDER  BY UserRole, FullName";
            using var cmd = new SqlCommand(sql, conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new User
                {
                    UserId = r.GetInt32("UserId"),
                    FullName = r.GetString("FullName"),
                    Username = r.GetString("Username"),
                    Email = r.GetString("Email"),
                    PhoneNumber = r.GetString("PhoneNumber"),
                    Address = r.GetString("Address"),
                    IsActive = r.GetBoolean("IsActive"),
                    CreatedAt = r.GetDateTime("CreatedAt"),
                    UserRole = r.GetString("UserRole")
                });
            return list;
        }
    }
}