
using SampleDataGeneratorLibrary;
using System.Text.Json;
using System.Threading.Tasks;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("This is the AI Generator Test App");

        string modelName = @"C:\Users\shahe\Downloads\Llama-3.2-3B-Instruct-Q4_K_M.gguf";
        int numberOfRecords = 10;
        string sampleString = """"
    {
        "Id":0,
        "FirstName":"",
        "LastName":"",
        "EmailAddress": ""
    }
    """";

        JsonDocument sampleDocument = JsonDocument.Parse(sampleString);

        AIGenerator aIGenerator = new(modelName);
        var output = await aIGenerator.GetSampleDataAsync(numberOfRecords, sampleDocument);
        JsonSerializerOptions options = new()
        {
            WriteIndented = true
        };
        string finalJson = JsonSerializer.Serialize(output, options);

        Console.WriteLine();
        Console.WriteLine(finalJson);
        Console.WriteLine();

        Console.WriteLine("Press any key to close");
        Console.ReadLine();
    }
}