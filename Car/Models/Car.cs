namespace Car.Models
{
    public class Car
    {
        public int CarId { get; set; }
        public string Name { get; set; } = "";
        public string Model { get; set; } = "";
        public int Year { get; set; }
        public decimal Price { get; set; }
        public string Color { get; set; } = "";
        public string FuelType { get; set; } = "";
        public int Mileage { get; set; }
        public bool IsAvailable { get; set; }
        public string ImagePath { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime AddedAt { get; set; }
        public string CarStatus { get; set; } = "Available";
        public string StatusNote { get; set; } = "";
        public int StockCount { get; set; } = 1;
        public int? StatusUpdatedBy { get; set; }
        public DateTime? StatusUpdatedAt { get; set; }

        // Display helpers
        public string PriceFormatted => $"PKR {Price:N0}";
        public string MileageFormatted => Mileage == 0 ? "Brand New" : $"{Mileage:N0} km";
        public string DisplayTitle => $"{Name} — {Model}";
        public string StatusBadge => CarStatus switch
        {
            "Needs Repair" => "🔧 Needs Repair",
            "Needs Restock" => "📦 Needs Restock",
            _ => "✅ Available"
        };
    }
}