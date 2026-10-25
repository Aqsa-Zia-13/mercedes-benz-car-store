namespace Car.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Address { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public string UserRole { get; set; } = "";   // SuperAdmin / Admin / Manager / Customer
        public DateTime CreatedAt { get; set; }
    }
}