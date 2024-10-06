using System;

namespace DropStockAPI.Models;

public class ConfirmEmailModel
{
    public required string Token { get; set; }
    public required string Email { get; set; }
}
