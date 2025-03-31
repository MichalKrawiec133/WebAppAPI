
using System.Collections;
using SQL.Models;

namespace SQL.DatabaseSeed;


public class Seed
{
    private readonly WebAppDbContext _context;

    public Seed(WebAppDbContext context)
    {
        _context = context;
    }

    public void ConvertCSVtoModels()
    {
        string documentsPath = "..\\CSVFiles\\Documents.csv";
        string documentItemsPath = "..\\CSVFiles\\DocumentItems.csv";
        
        
        var documentsDict = new Dictionary<int, Documents>();
        if (!File.Exists(documentsPath))
        
        {
            var documentsList = new List<Documents>();
            var documentLines = File.ReadAllLines(documentsPath);
            foreach (var line in documentLines.Skip(1))
            {
                var values = line.Split(';');

                var document = new Documents()
                {
                    DocumentId = int.Parse(values[0]),
                    Type = values[1],
                    Date = DateOnly.Parse(values[2]),
                    FirstName = values[4],
                    LastName = values[4],
                    City = values[5],
                    DocumentItems = new List<DocumentItems>()

                };
                
                documentsDict[document.DocumentId] = document;
            }
            
            _context.Documents.AddRange(documentsList);
        }
        else
        {
            
            Console.WriteLine("dokumenty nie istnieją");
            
        }


        
        if (!File.Exists(documentItemsPath))
        {
            var documentItemsList = new List<DocumentItems>();
            var documentItemsLines = File.ReadAllLines(documentItemsPath);
            foreach (var line in documentItemsLines.Skip(1))
            {
                var values = line.Split(';');

                var documentItems = new DocumentItems()
                {
                    DocumentItemsId = int.Parse(values[0]),
                    DocumentId = int.Parse(values[1]),
                    Product = values[2],
                    Quantity = int.Parse(values[3]),
                    Price = int.Parse(values[4]),
                    TaxRate = int.Parse(values[5])
                    
                };
                if (documentsDict.TryGetValue(documentItems.DocumentId, out var document))
                {
                    document.DocumentItems.Add(documentItems);
                }
                documentItemsList.Add(documentItems);
            }
            
            _context.DocumentItems.AddRange(documentItemsList);
        }
        else
        {
            
            Console.WriteLine("documentItems nie istnieją");
            
        }

        _context.SaveChanges();


    }
}