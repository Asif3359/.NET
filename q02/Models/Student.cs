using System.ComponentModel.DataAnnotations;
namespace q02.Models;

public class Student
{
    public long Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}