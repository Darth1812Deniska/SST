using System.Text.Json;
using SSTCommonClasses.Constructor.Models;


var model = new Model(); // дефолт
var options = new JsonSerializerOptions { WriteIndented = true };
var jsonString = JsonSerializer.Serialize(model, options);
Console.WriteLine(jsonString);
