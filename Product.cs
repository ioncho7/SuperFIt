using System.ComponentModel.DataAnnotations;

namespace SuperFit.Models;

public class Product
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Моля, въведете име.")]
    [Display(Name = "Име")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Моля, въведете описание.")]
    [Display(Name = "Описание")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Моля, въведете цена.")]
    [Display(Name = "Цена")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Цената трябва да е по-голяма от 0.")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Моля, въведете количество.")]
    [Display(Name = "Количество")]
    [Range(0, int.MaxValue, ErrorMessage = "Количеството не може да бъде отрицателно.")]
    public int StockQty { get; set; }

    [Required(ErrorMessage = "Моля, сложете снимка.")]
    [Display(Name = "Снимка")]
    public string? ImagePath { get; set; }

    [Display(Name = "Дата на създаване")]
    public DateTime CreatedOn { get; set; }

    [Required(ErrorMessage = "Моля, въведете категория.")]
    [Display(Name = "Категория")]
    public int CategoryId { get; set; }

    public Category? Category { get; set; }
}
