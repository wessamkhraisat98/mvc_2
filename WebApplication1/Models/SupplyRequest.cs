using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

public enum RequestStatus { Pending, Approved, Rejected }

public class SupplyRequest
{
    public int Id { get; set; }
    public DateTime RequestDate { get; set; }

    [Precision(18, 2)]
    public decimal TotalPrice { get; set; }

    public RequestStatus Status { get; set; }
    public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null!;
    public List<RequestItem> Items { get; set; } = [];
}

public class RequestItem
{
    public int Id { get; set; }
    public int SupplyRequestId { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }

    [Precision(18, 2)]
    public decimal Price { get; set; }
}
