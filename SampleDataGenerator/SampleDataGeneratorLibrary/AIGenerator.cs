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

<|eot_id|><|start_header_id|>assistant<|end_header_id|>
";
    }
}
