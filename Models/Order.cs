using System.ComponentModel.DataAnnotations;

namespace CateringManagement.Models
{
    public class OrderRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime EventDate { get; set; } // Only date

        [Required]
        public TimeSpan EventTime { get; set; } // Only time

        [Required]
        [StringLength(255)]
        public string Venue { get; set; }

        [Required]
        [StringLength(20)]
        public string ContactNumber { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        [Required]
        [MinLength(1)]
        public List<OrderItemRequest> Items { get; set; }
    }

    public class OrderItemRequest
    {
        [Required]
        public int MenuId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }

    public class Order
    {
        public int UserId { get; set; }
        public DateTime EventDate { get; set; }  // Only date part
        public TimeSpan EventTime { get; set; }  // Only time part
        public string Venue { get; set; }
        public string ContactNumber { get; set; }
        public string Notes { get; set; }
        public decimal TotalAmount { get; set; } // Calculated from Menu prices
    }

    public class OrderItem
    {
        public int MenuId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } // Filled from Menu in DB
    }

    public class OrderSummary
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public DateTime EventDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; }
    }

    public class OrderDetails
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public DateTime EventDate { get; set; }
        public TimeSpan EventTime { get; set; }
        public string Venue { get; set; }
        public string ContactNumber { get; set; }
        public string Notes { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; }
        public List<OrderItemDetails> Items { get; set; }
    }

    public class OrderItemDetails
    {
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
