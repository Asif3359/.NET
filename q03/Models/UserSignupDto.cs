
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace q03.Models;

public class UserSignupDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
