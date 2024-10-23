using System;
using System.ComponentModel.DataAnnotations;

namespace DropStockAPI.Models;

public class ResetPasswordModel
{
    [Required(ErrorMessage = "Password is required.")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Confirmation password is required."), Compare(nameof(Password), ErrorMessage = "Password and confirmation password is mismatched.")]
    public string? ConfirmPassword { get; set; }

    public string? Token { get; set; }

    public string? Email { get; set; }
}
