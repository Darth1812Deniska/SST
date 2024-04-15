using System.ComponentModel.DataAnnotations;
using System.Data;

namespace SSTCommonClasses.Infrastructure;

/// <summary>
/// Класс для хранения информации о стендах разработки
/// </summary>
public class ServerInfo
{
    private Guid? _uuid;
    private string? _login;
    private string? _password;
    private string? _host;
    private string? _name;
    private Project? _project;
    private int? _port;
    private DbType? _dbType;
    private string? _displayLabel;

    /// <summary>
    /// Уникальный идентифкатор стенда в системе SST
    /// </summary>
    [Key]
    public Guid? Uuid
    {
        get => _uuid;
        set => _uuid = value;
    }

    /// <summary>
    /// Логин для подключения к БД стенда
    /// </summary>
    public string? Login
    {
        get => _login;
        set => _login = value;
    }

    /// <summary>
    /// Пароль для подключения к БД стенда
    /// </summary>
    public string? Password
    {
        get => _password;
        set => _password = value;
    }

    /// <summary>
    /// Адрес стенда
    /// </summary>
    public string? Host
    {
        get => _host;
        set => _host = value;
    }

    public string? Name
    {
        get => _name;
        set => _name = value;
    }

    /// <summary>
    /// Порт стенда для подключения к БД
    /// </summary>
    public int? Port
    {
        get => _port;
        set => _port = value;
    }

    /// <summary>
    /// Проект в рамках которого используется стенд
    /// </summary>
    public Project? Project
    {
        get => _project;
        set => _project = value;
    }

    public DbType? DbType
    {
        get => _dbType;
        set => _dbType = value;
    }
    /// <summary>
    /// Удобное название стенда
    /// </summary>
    public string? DisplayLabel
    {
        get => _displayLabel;
        set => _displayLabel = value;
    }

    /// <summary>
    /// Конструктор класса ServerInfo
    /// </summary>
    public ServerInfo() { }
}