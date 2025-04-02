using System.Reflection.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQL;
using SQL.DatabaseSeed;
using SQL.Models;


namespace WebApplication1.Controllers;

[ApiController]
public class DocumentsController: ControllerBase
{
    private readonly WebAppDbContext _context;

    public DocumentsController(WebAppDbContext context)
    {

        _context = context;
        
    }

    [HttpDelete("ClearDatabase")]
    public async Task<IActionResult> ClearDatabase()
    {
        //Do wyczyszczenia bazy, do czystego testu przesyłania plików.
        var documents = _context.Documents.ToList();
        _context.Documents.RemoveRange(documents);
        await _context.SaveChangesAsync();
        
        return Ok();

    }

    [HttpPost("RebuildDatabase")]
    public IActionResult RebuildDatabase()
    {
        //W celu szybkiego odbudowania bazy bez odpalania na nowo projektu lub wrzucania plików csv.
        var seed = new SQL.DatabaseSeed.Seed(_context);
        seed.ConvertCSVtoModels();
        return Ok();

    }
    
    
    [HttpPost("SaveCSV")]
    public async Task<IActionResult> UploadItemsCSV(IFormFile fileDocuments, IFormFile fileDocumentItems)
    {
        if (fileDocuments == null || fileDocuments.Length == 0)
        {
            return BadRequest("Documents.csv Null or Empty");
        }
        if (fileDocumentItems == null || fileDocumentItems.Length == 0)
        {
            return BadRequest("DocumentItems.csv Null or Empty");
        }
        
        using var streamDocuments = new StreamReader(fileDocuments.OpenReadStream());
        using var streamDocumentItems = new StreamReader(fileDocumentItems.OpenReadStream());
        
        //pobranie pierwszej linii obu plików
        var lineDocuments = await streamDocuments.ReadLineAsync();
        var lineDocumentItems = await streamDocumentItems.ReadLineAsync();
        var firstLineDocumentsSplit = lineDocuments.Split(";");
        var firstLineDocumentItemsSplit = lineDocumentItems.Split(";");
        var firstLineDocumentsCheck = false;
        var firstLineDocumentItemsCheck = false;
        
        //sprawdzenie czy pierwsza linia zawiera nazwe kolumn
        try
        {
            if (int.TryParse(firstLineDocumentsSplit[0], out _))
            {
                firstLineDocumentsCheck = true;
            }
            
            if (int.TryParse(firstLineDocumentItemsSplit[0], out _))
            {
                firstLineDocumentItemsCheck = true;
            }
           
        }
        catch (Exception e)
        {
            throw;
        }

        if (!firstLineDocumentsCheck)
        {
            lineDocuments = await streamDocuments.ReadLineAsync();
            firstLineDocumentsSplit = lineDocuments.Split(';');
        }
        if (!firstLineDocumentItemsCheck)
        {
            lineDocumentItems = await streamDocumentItems.ReadLineAsync();
            firstLineDocumentItemsSplit = lineDocumentItems.Split(';');
        }

        
        //dodanie pierwszego wiersza do słownika dokumentów
        var documentsDict = new Dictionary<int, Documents>();
        var documentFirst = new Documents()
        {
                    
            Type = firstLineDocumentsSplit[1],
            Date = DateOnly.Parse(firstLineDocumentsSplit[2]),
            FirstName = firstLineDocumentsSplit[3],
            LastName = firstLineDocumentsSplit[4],
            City = firstLineDocumentsSplit[5],
            DocumentItems = new List<DocumentItems>()

        };
                
        documentsDict[int.Parse(firstLineDocumentsSplit[0])] = documentFirst;
        
        
        //przetwarzanie reszty dokumentów
        while (!streamDocuments.EndOfStream)
        {
            lineDocuments = await streamDocuments.ReadLineAsync();
            var lineDocumentsSplit = lineDocuments.Split(";");
            var document = new Documents()
            {
                    
                Type = lineDocumentsSplit[1],
                Date = DateOnly.Parse(lineDocumentsSplit[2]),
                FirstName = lineDocumentsSplit[3],
                LastName = lineDocumentsSplit[4],
                City = lineDocumentsSplit[5],
                DocumentItems = new List<DocumentItems>()

            };
                
            documentsDict[int.Parse(lineDocumentsSplit[0])] = document;
        }
            
        _context.Documents.AddRange(documentsDict.Values);
        
        
        //dodanie pierwszego rekordu pozycji dokumentów 
        var documentItemsFirst = new DocumentItems()
        {
            DocumentId = int.Parse(firstLineDocumentItemsSplit[0]),
            Ordinal = int.Parse(firstLineDocumentItemsSplit[1]),
            Product = firstLineDocumentItemsSplit[2],
            Quantity = int.Parse(firstLineDocumentItemsSplit[3]),
            Price = float.Parse(firstLineDocumentItemsSplit[4]), 
            TaxRate = int.Parse(firstLineDocumentItemsSplit[5])
        };

        if (documentsDict.TryGetValue(documentItemsFirst.DocumentId, out var firstDocument))
        {
            firstDocument.DocumentItems.Add(documentItemsFirst);
        }
        
        //przetworzenie reszty rekordów pozycji dokumentów
        
        while (!streamDocuments.EndOfStream)
        {
            lineDocumentItems = await streamDocumentItems.ReadLineAsync();
            var lineDocumentItemsSplit = lineDocumentItems.Split(";");
            

            var documentItems = new DocumentItems()
            {
                    
                DocumentId = int.Parse(lineDocumentItemsSplit[0]),
                Ordinal = int.Parse(lineDocumentItemsSplit[1]),
                Product = lineDocumentItemsSplit[2],
                Quantity = int.Parse(lineDocumentItemsSplit[3]),
                Price = float.Parse(lineDocumentItemsSplit[4]),
                TaxRate = int.Parse(lineDocumentItemsSplit[5])
                    
            };
                
            if (documentsDict.TryGetValue(documentItems.DocumentId, out var nextDocument))
            {
                nextDocument.DocumentItems.Add(documentItems); 
            }
            
        }
        //todo: synchronizacja id.
        _context.SaveChanges();
        return Ok();
    }
    
}