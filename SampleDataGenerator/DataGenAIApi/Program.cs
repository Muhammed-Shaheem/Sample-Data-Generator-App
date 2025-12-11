using SampleDataGeneratorLibrary;
using Scalar.AspNetCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();
string modelPath = builder.Configuration.GetValue<string>("ModelPath") ?? string.Empty;
builder.Services.AddSingleton(new AIGenerator(modelPath));


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); 
}

app.UseHttpsRedirection();


app.MapGet("/", () => "Hello world");
app.MapGet("/api/generate-sample-data", async (int recordCount, string sampleStructure, AIGenerator generator) => {
    using JsonDocument sampleDocument = JsonDocument.Parse(sampleStructure);

    var output = await generator.GetSampleDataAsync(recordCount, sampleDocument);

    return output;
});

app.Run();


