namespace Car.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public int CarId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = "";
        public string Status { get; set; } = "";
        public string ReceiptNumber { get; set; } = "";
        public string ReceiptPath { get; set; } = "";

        // Joined display fields
        public string CarName { get; set; } = "";
        public string CarModel { get; set; } = "";
        public int CarYear { get; set; }
        public string CustomerName { get; set; } = "";
        public string CustomerEmail { get; set; } = "";
        public string CustomerPhone { get; set; } = "";
        public string CustomerUsername { get; set; } = "";

        public string PriceFormatted => $"PKR {TotalAmount:N0}";
        public string DateFormatted => OrderDate.ToString("dd MMM yyyy");
    }
}