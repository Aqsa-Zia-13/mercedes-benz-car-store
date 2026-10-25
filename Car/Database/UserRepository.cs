using System.Data;
using Microsoft.Data.SqlClient;
using Car.Models;

namespace Car.Database
{
    public class UserRepository
    {
        // ── LOGIN ──────────────────────────────────────────────
        public static User? Login(string username, string password, string role)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            const string sql = @"
                SELECT UserId, FullName, Username, Password, Email,
                       PhoneNumber, Address, IsActive, CreatedAt, UserRole
                FROM   Users
                WHERE  Username = @Username
                  AND  Password = @Password
                  AND  UserRole = @UserRole
                  AND  IsActive = 1";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Username", username.Trim());
            cmd.Parameters.AddWithValue("@Password", password);
            cmd.Parameters.AddWithValue("@UserRole", role);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;
            return MapUser(r);
        }

        // ── REGISTER ───────────────────────────────────────────
        public static bool UsernameExists(string username)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "SELECT COUNT(1) FROM Users WHERE Username=@U", conn);
            cmd.Parameters.AddWithValue("@U", username.Trim());
            return (int)cmd.ExecuteScalar() > 0;
        }

        public static bool EmailExists(string email)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "SELECT COUNT(1) FROM Users WHERE Email=@E", conn);
            cmd.Parameters.AddWithValue("@E", email.Trim());
            return (int)cmd.ExecuteScalar() > 0;
        }

        public static void RegisterCustomer(string fullName, string username,
            string email, string phone, string address, string password)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            const string sql = @"
                INSERT INTO Users
                    (FullName,Username,Password,Email,PhoneNumber,Address,IsActive,UserRole)
                VALUES
                    (@F,@U,@P,@E,@Ph,@A,1,'Customer')";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@F", fullName);
            cmd.Parameters.AddWithValue("@U", username);
            cmd.Parameters.AddWithValue("@P", password);
            cmd.Parameters.AddWithValue("@E", email);
            cmd.Parameters.AddWithValue("@Ph", phone);
            cmd.Parameters.AddWithValue("@A", address);
            cmd.ExecuteNonQuery();
        }

        private static User MapUser(SqlDataReader r) => new()
        {
            UserId = r.GetInt32("UserId"),
            FullName = r.GetString("FullName"),
            Username = r.GetString("Username"),
            Email = r.GetString("Email"),
            PhoneNumber = r["PhoneNumber"] as string ?? "",
            Address = r["Address"] as string ?? "",
            IsActive = r.GetBoolean("IsActive"),
            CreatedAt = r.GetDateTime("CreatedAt"),
            UserRole = r.GetString("UserRole")
        };
    }
}