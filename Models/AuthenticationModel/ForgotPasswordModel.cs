using System;
using System.ComponentModel.DataAnnotations;

namespace DropStockAPI.Models;

public class ForgotPasswordModel


{
    [Required(ErrorMessage = "Email is required."), EmailAddress(ErrorMessage = "Incorrect email.")]
    public required string Email { get; set; }
}

