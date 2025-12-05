using SampleDataGeneratorLibrary;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DataGenAI.Desktop;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        SubmitButton.IsEnabled = false;
        SubmitButton.Content = "Processing...";


        try
        {
           using JsonDocument sampleDocument = JsonDocument.Parse(SampleStructure.Text.ToString());

            AIGenerator generator = new(@"C:\Users\shahe\Downloads\Llama-3.2-3B-Instruct-Q4_K_M.gguf");
            var output = await generator.GetSampleDataAsync(int.Parse(NumberOfRecords.Text), sampleDocument);
            JsonSerializerOptions options = new()
            {
                WriteIndented = true
            };
            string finalJson = JsonSerializer.Serialize(output, options);

            Results.Text = finalJson;

        }
        catch (Exception ex )
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            
        }
        finally
        {
            SubmitButton.IsEnabled = true;
            SubmitButton.Content = "Submit";
        }
    }
}