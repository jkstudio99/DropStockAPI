using System;
using System.Collections.Generic;

namespace DropStockAPI.Models;

public partial class SupplierModel
{
    public int supplierid { get; set; }

    public string? name { get; set; }

    public string? address { get; set; }

    public string? phone { get; set; }

    public string? email { get; set; }

    public string? otherdetail { get; set; }
}
