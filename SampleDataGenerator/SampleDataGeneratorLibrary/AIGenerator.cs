using LLama;
using LLama.Common;
using LLama.Sampling;
using System.Text;
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

        ModelParams parameters = new(modelPath)
        {
            ContextSize = 4096,
            GpuLayerCount = 0,
            BatchSize = 512,
            UseMemorymap = true
        };
        using var model = LLamaWeights.LoadFromFile(parameters);
        using var context = model.CreateContext(parameters);
        StatelessExecutor executor = new(model,parameters);

        InferenceParams inferenceParams = new()
        {
            MaxTokens = 4096,
            SamplingPipeline = new DefaultSamplingPipeline
            {
                Temperature = 0.6f
            },
            AntiPrompts = ["<|eot_id|>","<|end_of_text|>"]
        };

        List<JsonElement> records = new();
        int batchSize = 10;
        int numberOfBatches = (int)Math.Ceiling((double)recordCount / batchSize);

        for (int batchNumber = 0; batchNumber < numberOfBatches; batchNumber++)
        {
            int recordsInThisBatch = Math.Min(batchSize, recordCount - (batchNumber * batchSize));
            int startingId = batchNumber * batchSize + 1;
            string prompt = BuildSampleDataPrompt(startingId,recordsInThisBatch, sampleDocument);

            StringBuilder fullResponse = new();
            await foreach(var text in executor.InferAsync(prompt, inferenceParams))
            {
                fullResponse.Append(text);

            }
            string? jsonData = ExtractJson(fullResponse.ToString());

            if (jsonData is null)
            {
                Console.WriteLine($"Batch number {batchNumber + 1} ignored - bad data.");
                continue;
            }

            try
            {
                JsonDocument jsonDoc = JsonDocument.Parse(jsonData);
                if (jsonDoc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach(var record in jsonDoc.RootElement.EnumerateArray())
                    {
                        records.Add(record.Clone());
                    }
                }
            }
            catch 
            {

                Console.WriteLine("No valid records found in JSON"); 
            }
        }

        string jsonString = JsonSerializer.Serialize(records);
        JsonDocument output = JsonDocument.Parse(jsonString);

        return output;
    }

    private string BuildSampleDataPrompt(int startingId, int recordCount, JsonDocument sampleDocument)
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
5. Do not ever include example datas in actual data generation. 
6. Generate varied information, including using rare edge cases. Avoid repeating patterns within the response  
7. DO NOT include any text, explanation, or markdown before or after the JSON array
8. Ensure valid JSON syntax with proper commas between objects
9. Make each record unique with different values
10. If a property name requires a numeric Id, these records should start with the id of {startingId} and increment by 1 each time 
11. If an Id or similar unique value is required. Ensure that it is unique, utilize the value of {startingId}
12. If an Guid or similar unique value is required. Ensure that it is unique, utilize the value of {startingId}
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
