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
        StatelessExecutor executor = new(model, parameters);

        InferenceParams inferenceParams = new()
        {
            MaxTokens = 4096,
            SamplingPipeline = new DefaultSamplingPipeline
            {
                Temperature = 0.6f
            },
            AntiPrompts = ["<|eot_id|>", "<|end_of_text|>"]
        };

        List<JsonElement> records = new();
        int batchSize = 10;
        int numberOfBatches = (int)Math.Ceiling((double)recordCount / batchSize);

        for (int batchNumber = 0; batchNumber < numberOfBatches; batchNumber++)
        {
            int recordsInThisBatch = Math.Min(batchSize, recordCount - (batchNumber * batchSize));
            int startingId = batchNumber * batchSize + 1;
            string prompt = BuildSampleDataPrompt(startingId, recordsInThisBatch, sampleDocument);

            StringBuilder fullResponse = new();
            await foreach (var text in executor.InferAsync(prompt, inferenceParams))
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
                    foreach (var record in jsonDoc.RootElement.EnumerateArray())
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

        string prompt = $@"<|start_header_id|>system<|end_header_id|>
You are a precise JSON data generator. Follow all instructions exactly.
<|eot_id|><|start_header_id|>user<|end_header_id|>
Generate exactly {recordCount} records of realistic sample data using the following structure:
{sampleAsString}

CRITICAL REQUIREMENTS:
1. Output ONLY a valid JSON array starting with [ and ending with ].
2. Generate exactly {recordCount} complete records.
3. Each record MUST match the exact property names and types from the structure above.
4. ALL fields must contain realistic, non-empty values:
   - Names: Use real first and last names.
   - Addresses: Realistic street, city, state, and ZIP.
   - Phone numbers: Valid formats, e.g., ""+919567206946"".
   - Email: Realistic emails.
   - Dates: Valid formats, e.g., ""21-04-2003"".
   - Numbers: Realistic non-zero values unless zero is required.
5. Do NOT include example placeholder values inside the actual data.
6. Generate varied data. Avoid repeating patterns.
7. Do NOT include any text, explanation, or markdown before or after the JSON.
8. Ensure valid JSON: proper commas, quotes, and structure.
9. Each record must be unique.
10. If a property uses an integer Id, start at {startingId} and increment by 1.
11. If a property requires any unique value (e.g., numeric ID), base uniqueness on {startingId}.
12. If a property requires a Guid, it must be unique for each record.
Your response must start with [ and end with ].

<|eot_id|><|start_header_id|>assistant<|end_header_id|>";

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
