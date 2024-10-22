using System;
using System.Collections.Generic;

namespace DropStockAPI.Models;

public partial class CategoryModel
{
    public int categoryid { get; set; }

    public string categoryname { get; set; } = null!;

    public int categorystatus { get; set; }
}
