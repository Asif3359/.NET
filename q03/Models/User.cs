using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace q03.Models;

[Index(nameof(Email), IsUnique = true)]
public class User
{
    public long Id { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
