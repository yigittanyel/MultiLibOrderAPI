namespace MultiLLibray.API.Models
{
    public sealed class Order
    {
        public int Id { get; set; }
        public string ProductName{ get; set; }
        public int Quantity { get; set; }
        public int TotalPrice { get; set; }
    }
}
