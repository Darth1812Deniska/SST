using System.Text.Json;
using SSTCommonClasses.Constructor.Models;


Model model = new Model(); // дефолт
JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
string jsonString = JsonSerializer.Serialize(model, options);
Console.WriteLine(jsonString);
