namespace WebApplication1.DTO;

public class DocumentsDTO
{ 
    public int DocumentId { get; set; }
    
    public string Type { get; set; }
    
    public DateOnly Date { get; set; }
    
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public string City { get; set; }
    
    public List<DocumentItemsDTO> DocumentItems { get; set; }
}