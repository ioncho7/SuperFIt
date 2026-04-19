using System.ComponentModel.DataAnnotations;

namespace SuperFit.Models;

public class ContactMessage
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Моля, въведете име.")]
    [Display(Name = "Име")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Моля, въведете имейл.")]
    [Display(Name = "Имейл")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Моля, въведете тема.")]
    [Display(Name = "Тема")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Моля, въведете съобщение.")]
    [Display(Name = "Съобщение")]
    public string Message { get; set; } = string.Empty;

    [Display(Name = "Дата")]
    public DateTime SentOn { get; set; } = DateTime.UtcNow;
    public int? OrderId { get; set; }  
    public ApplicationUser? ApplicationUser { get; set; }
    public bool IsOrderNotification { get; set; } 

}
