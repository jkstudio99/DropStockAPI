using System;
using System.Collections.Generic;

namespace DropStockAPI.Models;

public partial class OrderModel
{
    public int orderid { get; set; }
    public string? ordername { get; set; }
    public decimal orderprice { get; set; }
    public string? orderstatus { get; set; }
    public string? orderdetails { get; set; }
    public int customerid { get; set; }
    public DateTime createddate { get; set; }
    public DateTime? modifieddate { get; set; }

}
