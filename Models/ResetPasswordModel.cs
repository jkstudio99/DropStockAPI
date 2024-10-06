using System;

namespace DropStockAPI.Models;

public class ResetPasswordModel
{
    public required string Token { get; set; }
    public required string NewPassword { get; set; }
    public required string Email { get; set; } // Add this line
}
