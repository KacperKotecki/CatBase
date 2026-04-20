using System.ComponentModel.DataAnnotations;

namespace CatBase.Models;

public class CatFactApiOptions
{
    [Required]
    [Url]
    public string FactUrl { get; set; } = string.Empty;
}
