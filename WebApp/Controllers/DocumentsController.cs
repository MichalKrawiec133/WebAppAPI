﻿using System.Reflection.Metadata;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQL;
using SQL.DatabaseSeed;
using SQL.Models;
using WebApplication1.DTO;


namespace WebApplication1.Controllers;

[ApiController]
public class DocumentsController: ControllerBase
{
    private readonly WebAppDbContext _context;

    public DocumentsController(WebAppDbContext context)
    {

        _context = context;
        
    }

    //Do wyczyszczenia bazy, do czystego testu przesyłania plików.
    [HttpDelete("ClearDatabase")]
    public async Task<IActionResult> ClearDatabase()
    {
        
        var documents = _context.Documents.ToList();
        _context.Documents.RemoveRange(documents);
        await _context.SaveChangesAsync();
        
        return Ok();

    }

    //W celu szybkiego odbudowania bazy bez odpalania na nowo projektu lub wrzucania plików csv.
    [HttpPost("RebuildDatabase")]
    public IActionResult RebuildDatabase()
    {
        
        var seed = new SQL.DatabaseSeed.Seed(_context);
        seed.ConvertCSVtoModels();
        return Ok();

    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        
        var document = await _context.Documents.FindAsync(id);
        if (document != null)
        {
            _context.Documents.Remove(document);
        }

        await _context.SaveChangesAsync();
        return Ok();
        
        
    }
    //pobranie ilosci danych w bazie
    [HttpGet("GetDocuments/Count")]
    public async Task<IActionResult> GetDocumentsCount()
    {
        var count = await _context.Documents.CountAsync();
        return Ok(count);
    }
    
    //wysłanie wszystkich danych. 
    // [HttpGet("GetDocuments")]
    // public async Task<IActionResult> GetDocuments()
    // {
    //     var documents = await _context.Documents
    //         .AsNoTracking()
    //         .Include(d => d.DocumentItems) 
    //         .Select(d => new DocumentsDTO
    //         {
    //             DocumentId = d.DocumentId,
    //             Type = d.Type,
    //             Date = d.Date,
    //             FirstName = d.FirstName,
    //             LastName = d.LastName,
    //             City = d.City,
    //             DocumentItems = d.DocumentItems.Select(di => new DocumentItemsDTO
    //             {
    //                 DocumentItemsId = di.DocumentItemsId,
    //                 DocumentId = di.DocumentId,
    //                 Ordinal = di.Ordinal,
    //                 Product = di.Product,
    //                 Quantity = di.Quantity,
    //                 Price = di.Price,
    //                 TaxRate = di.TaxRate
    //             }).ToList()
    //         })
    //         .ToListAsync();
    //
    //     return Ok(documents);
    // }
    
    //pobranie dokumentów csv 
    [HttpGet("GetDocuments")]
    public IActionResult GetDocuments()
    {
        var documents = _context.Documents.AsNoTracking().ToList();

        var csv = new StringBuilder();
        csv.AppendLine("DocumentId;Type;Date;FirstName;LastName;City");

        foreach (var doc in documents)
        {
            csv.AppendLine($"{doc.DocumentId};{doc.Type};{doc.Date};{doc.FirstName};{doc.LastName};{doc.City}");
        }

        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "documents.csv");
    }
    
    //pobranie itemów dokumentów
    [HttpGet("GetDocumentItems")]
    public IActionResult GetDocumentItems()
    {
        var items = _context.DocumentItems.AsNoTracking().ToList();

        var csv = new StringBuilder();
        csv.AppendLine("DocumentId;Ordinal;Product;Quantity;Price;TaxRate");

        foreach (var item in items)
        {
            csv.AppendLine($"{item.DocumentId};{item.Ordinal};{item.Product};{item.Quantity};{item.Price};{item.TaxRate}");
        }

        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "documentItems.csv");
    }
    
    [HttpGet("GetDocuments/Search")]
    public async Task<IActionResult> SearchDocuments(string field, string value)
    {
        var query = _context.Documents.Include(d => d.DocumentItems).AsQueryable();

        switch (field.ToLower())
        {
            case "firstname":
                query = query.Where(d => d.FirstName.Contains(value));
                break;
            case "lastname":
                query = query.Where(d => d.LastName.Contains(value));
                break;
            case "city":
                query = query.Where(d => d.City.Contains(value));
                break;
            default:
                return BadRequest("Nieprawidłowe pole wyszukiwania.");
        }

        var result = await query.Select(d => new DocumentsDTO
        {
            DocumentId = d.DocumentId,
            Type = d.Type,
            Date = d.Date,
            FirstName = d.FirstName,
            LastName = d.LastName,
            City = d.City,
            DocumentItems = d.DocumentItems.Select(di => new DocumentItemsDTO
            {
                DocumentItemsId = di.DocumentItemsId,
                DocumentId = di.DocumentId,
                Ordinal = di.Ordinal,
                Product = di.Product,
                Quantity = di.Quantity,
                Price = di.Price,
                TaxRate = di.TaxRate
            }).ToList()
        }).ToListAsync();
        if (result == null || result.Count == 0)
        {
            return NotFound("Brak wyników dla podanych kryteriów.");
        }
        return Ok(result);
    }
    
    //wysyłanie części danych. domyślnie 50 dokumentów z ich rekordami
    [HttpGet("GetDocuments/Partial")]
    public async Task<IActionResult> GetDocumentsPartial(int skip = 0, int take = 50)
    {
        var documents = await _context.Documents
            .Include(d => d.DocumentItems)
            .Skip(skip)
            .Take(take)
            .Select(d => new DocumentsDTO
            {
                DocumentId = d.DocumentId,
                Type = d.Type,
                Date = d.Date,
                FirstName = d.FirstName,
                LastName = d.LastName,
                City = d.City,
                DocumentItems = d.DocumentItems.Select(di => new DocumentItemsDTO
                {
                    DocumentItemsId = di.DocumentItemsId,
                    DocumentId = di.DocumentId,
                    Ordinal = di.Ordinal,
                    Product = di.Product,
                    Quantity = di.Quantity,
                    Price = di.Price,
                    TaxRate = di.TaxRate
                }).ToList()
            })
            .ToListAsync();

        return Ok(documents);
    }


    //zapisanie przesłanych danych
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
        //pominięcie pierwszej linii
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
        
        while (!streamDocumentItems.EndOfStream)
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
        
        _context.SaveChanges();
        return Ok();
    }
    
}