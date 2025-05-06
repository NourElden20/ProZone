namespace ProZone.Models
{
    public class DashboardModel
    {
        public int UserCount { get; set; }            // Total number of users
        public int OrdersDelivered { get; set; }      // Total number of orders delivered
        public int ProductCount { get; set; }         // Total number of products
        public decimal TotalRevenue { get; set; }     // Total revenue from all orders
        public List<Order> Orders { get; set; }       // List of orders (to display pending orders)
    }
}
