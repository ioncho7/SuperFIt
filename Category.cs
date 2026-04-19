using System.ComponentModel.DataAnnotations;

namespace SuperFit.Models;

public class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Моля, въведете име.")]
    [Display(Name = "Име")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Описание")]
    [StringLength(300, ErrorMessage = "Описанието може да е до 300 символа.")]
    public string? Description { get; set; }
}
