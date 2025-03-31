using System.ComponentModel.DataAnnotations;

namespace SQL.Models;

public class Documents
{
    [Key]
    [Required]
    public int DocumentId { get; set; }
    [Required]
    public string Type { get; set; }
    [Required]
    public DateOnly Date { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }

    public virtual ICollection<DocumentItems> DocumentItems { get; set; } = new List<DocumentItems>();

}