using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.SignalR;

namespace SuperFit.Models
{
    public enum OrderStatus
    {
        Pending = 0,
        Approved = 1,
        Cancelled = 2
    }
    public class Order
    {
        public int Id { get; set; }

        public ApplicationUser? ApplicationUser {  get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public decimal TotalPrice { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
