using System;
using System.Collections.Generic;

namespace DropStockAPI.Models;

public partial class CustomerModel
{
    public int customerid { get; set; }

    public string? firstname { get; set; }

    public string? lastname { get; set; }

    public string? address { get; set; }

    public string? phone { get; set; }

    public string? email { get; set; }

    public int? staffid { get; set; }
    // // Navigation property to Orders
    // public ICollection<OrderModel> Orders { get; set; }
}
