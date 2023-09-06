using System.Text.Json;
using System.Text.Json.Serialization;
using SSTCommonClasses;

var model = new Model(); // дефолт
var options = new JsonSerializerOptions { WriteIndented = true };
var jsonString = JsonSerializer.Serialize(model, options);
Console.WriteLine(jsonString);
var dsModel = JsonSerializer.Deserialize<Model>(jsonString);
Console.WriteLine(model.Equals(dsModel));


Model modelMssql = new Model("TypeMsSqlModel", "Типовая модель MS SQL", "Модель для проверки", DbType.MsSql, "dbo",
    ModelAccessType.Common, 0);

var rootModelItem = modelMssql.RootModelItem;
string testMssqlJsonModel =  JsonSerializer.Serialize(rootModelItem, options);
Console.WriteLine(testMssqlJsonModel);