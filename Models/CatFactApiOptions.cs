using System.ComponentModel.DataAnnotations;

namespace CatBase.Models;

public class CatFactApiOptions
{
    [Required]
    [Url]
    public string FactUrl { get; set; } = string.Empty;

    [Required]
    public string OutputFileName { get; set; } = string.Empty;
}
