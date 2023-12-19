using System.Text.Json;
using SSTCommonClasses;
using SSTCommonClasses.Constants;
using SSTCommonClasses.Models;

var model = new Model(); // дефолт
var options = new JsonSerializerOptions { WriteIndented = true };
var jsonString = JsonSerializer.Serialize(model, options);
Console.WriteLine(jsonString);
