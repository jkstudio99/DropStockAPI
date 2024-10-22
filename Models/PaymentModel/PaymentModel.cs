using System;
using System.Collections.Generic;

namespace DropStockAPI.Models;

public partial class PaymentModel
{
    public int billnumber { get; set; }

    public string? paymenttype { get; set; }

    public string? otherdetail { get; set; }
}
