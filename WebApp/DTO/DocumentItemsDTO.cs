namespace WebApplication1.DTO;

public class DocumentItemsDTO
{
    public int DocumentItemsId { get; set; }
    
    public int DocumentId { get; set; }
    
    public int Ordinal { get; set; }
  
    public string Product { get; set; }
  
    public int Quantity { get; set; }
    
    public float Price { get; set; }
    
    public int TaxRate { get; set; }
}