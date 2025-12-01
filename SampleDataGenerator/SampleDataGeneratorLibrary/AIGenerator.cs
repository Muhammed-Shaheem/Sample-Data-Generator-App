using System.Text.Json;

namespace SampleDataGeneratorLibrary;

public class AIGenerator
{
    private readonly string modelPath;

    public AIGenerator(string modelPath)
    {
        if (File.Exists(modelPath) == false)
        {
            throw new ArgumentException("The path to the model was not found.");
        }
        this.modelPath = modelPath;
    }

    public async Task<JsonDocument> GetSampleDataAsync(int recordCount, JsonDocument sampleDocument)
    {
        throw new NotImplementedException();
    }

    private string BuildSampleDataPrompt(int recordCount, JsonDocument sampleDocument)
    {
        string sampleAsString = sampleDocument.RootElement.ToString();
        string prompt = $@" <|start_header_id|>system<|end_header_id|>
You are a precise JSON data generator. You must follow instructions exactly.
<|eot_id|><|start_header_id|>user<|end_header_id|>
Generate exactly {recordCount} records of realistic sample data of foloowing structure:
{sampleAsString}

CRITICAL REQUIREMNETS:
1. Output ONLY a valid JSON array starting with [ and ending with ]
2. Generate exactly {recordCount} complete records 
3. Each record MUST match the exact property names and types shown above 
4. ALL fields must contain realistic, non-empty values:
    — Names: Use realistic first and last names (e.g. , ""Muhammed Shaheem"", ""Sue Storm"")
    — Addresses: Use realistic street addresses, cities, states, and zip code
    - Phone numbers: Use valid format (e.g., ""+919567206946"")
    - Email addresses: Use realistic emails (e.g., Muhammedshaheem@gmail.com)
    - Dates: Use valid date formats (e.g., ""21-04-2003"")
    - Numbers: Use realistic numbers non zero numbers unless a zero is called for 
5. DO NOT include any text, explanation, or markdown before or after the JSON array
6. Ensure valid JSON syntax with proper commas between objects
7. Make each record unique with different values
Your response must start with [ and end with ]

<|eot_id|><|start_header_id|>assistant<|end_header_id|>
";
        return prompt;
    }

    private string? ExtractJson(string response)
    {
        int startIndex = response.IndexOf('[');
        if (startIndex < 0)
        {
            Console.WriteLine("No array found in response");
            return null;

        }

        int bracketCount = 0;//check
        int endIndex = -1;

        for (int i = startIndex; i < response.Length; i++)
        {
            if (response[i] == '[')
            {
                bracketCount++;
            }
            else if (response[i] == ']')
            {
                bracketCount--;
                if (bracketCount == 0)
                {
                    endIndex = i;
                    break;
                }
            }
        }

        if (endIndex > startIndex)
        {
            return response.Substring(startIndex, endIndex - startIndex + 1).Trim();
        }

        Console.WriteLine("response was not properly formed array");
        return null;
    }
}
