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
}
