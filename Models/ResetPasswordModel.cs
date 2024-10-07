using System;

namespace DropStockAPI.Models;

public class ResetPasswordModel
{
    public string? Email { get; set; }
    public string? Token { get; set; }
    public string? NewPassword { get; set; }
}
