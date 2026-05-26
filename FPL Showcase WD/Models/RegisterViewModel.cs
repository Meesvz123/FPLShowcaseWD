using System.ComponentModel.DataAnnotations;

namespace FPL_Showcase_WD.Models;

public sealed class RegisterViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Wachtwoorden komen niet overeen.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}