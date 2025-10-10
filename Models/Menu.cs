namespace CateringManagement.Models
{
    public class MenuItem
    {
        public int MenuId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public bool Availability { get; set; } = true;
        public bool IsVegetarian { get; set; } = true;
    }
    public class UpdateMenuItem
    {
        public string? Name { get; set; }
        public string? Category { get; set; }
        public decimal? Price { get; set; }
        public bool? Availability { get; set; }
        public bool? IsVegetarian { get; set; }
    }
}
