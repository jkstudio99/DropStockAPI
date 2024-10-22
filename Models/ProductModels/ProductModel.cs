using System;
using System.Collections.Generic;

namespace DropStockAPI.Models;

public partial class ProductModel
{
    public int productid { get; set; }

    public string? productname { get; set; }

    public decimal? unitprice { get; set; }

    public int? unitinstock { get; set; }

    public string? productpicture { get; set; }

    public int categoryid { get; set; }

    public DateTime createddate { get; set; }

    public DateTime? modifieddate { get; set; }
}
