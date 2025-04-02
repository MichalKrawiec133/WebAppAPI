
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
        string documentsPath = "..\\SQL\\CSVFiles\\Documents.csv";
        string documentItemsPath = "..\\SQL\\CSVFiles\\DocumentItems.csv";
        
        
        var documentsDict = new Dictionary<int, Documents>();
        if (File.Exists(documentsPath))
        {
            var documentsList = new List<Documents>();
            var documentLines = File.ReadAllLines(documentsPath);
            foreach (var line in documentLines.Skip(1))
            {
                var values = line.Split(';');

                var document = new Documents()
                {
                    
                    Type = values[1],
                    Date = DateOnly.Parse(values[2]),
                    FirstName = values[3],
                    LastName = values[4],
                    City = values[5],
                    DocumentItems = new List<DocumentItems>()

                };
                
                documentsDict[int.Parse(values[0])] = document;
            }
            
            _context.Documents.AddRange(documentsDict.Values);
        }
        else
        {
            
            Console.WriteLine("dokumenty nie istnieją");
            
        }
       
        if (File.Exists(documentItemsPath))
        {
            var documentItemsList = new List<DocumentItems>();
            var documentItemsLines = File.ReadAllLines(documentItemsPath);
            
            foreach (var line in documentItemsLines.Skip(1))
            {
                var values = line.Split(';');

                var documentItems = new DocumentItems()
                {
                    
                    DocumentId = int.Parse(values[0]),
                    Ordinal = int.Parse(values[1]),
                    Product = values[2],
                    Quantity = int.Parse(values[3]),
                    Price = float.Parse(values[4]),
                    TaxRate = int.Parse(values[5])
                    
                };
                
                if (documentsDict.TryGetValue(documentItems.DocumentId, out var document))
                {
                    document.DocumentItems.Add(documentItems); 
                }
            }
        }
        else
        {
            
            Console.WriteLine("documentItems nie istnieją");
            
        }

        _context.SaveChanges();


    }
}