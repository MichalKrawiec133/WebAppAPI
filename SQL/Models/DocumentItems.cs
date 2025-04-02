using System.ComponentModel.DataAnnotations;

namespace SQL.Models;

public class DocumentItems
{
    [Key]
    [Required]
    public int DocumentItemsId { get; set; }
    [Required]
    public int DocumentId { get; set; }
    [Required]
    public int Ordinal { get; set; }
    [Required]
    public string Product { get; set; }
    [Required]
    public int Quantity { get; set; }
    [Required]
    public float Price { get; set; }
    [Required]
    public int TaxRate { get; set; }
    public virtual Documents Document { get; set; }
    
}