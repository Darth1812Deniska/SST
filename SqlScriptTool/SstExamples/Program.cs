using System.Text.Json;
using SSTCommonClasses.Constructor.Models;


Model model = new(); // дефолт
JsonSerializerOptions options = new () { WriteIndented = true };
string jsonString = JsonSerializer.Serialize(model, options);
Console.WriteLine(jsonString);
