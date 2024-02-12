namespace SSTCommonClasses.Infrastructure;

/// <summary>
/// Класс для хранения информации о стендах разработки
/// </summary>
public class ServerInfo
{
    private Guid _uuid;
    private string _name;
    private string _login;
    private string _password;
    private string _host;
    private Project _project;
    private int? _port;

    /// <summary>
    /// Уникальный идентифкатор стенда в системе SST
    /// </summary>
    public Guid Uuid
    {
        get => _uuid;
        set => _uuid = value;
    }

    /// <summary>
    /// Удобное название стенда
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    /// <summary>
    /// Логин для подключения к БД стенда
    /// </summary>
    public string Login
    {
        get => _login;
        set => _login = value;
    }

    /// <summary>
    /// Пароль для подключения к БД стенда
    /// </summary>
    public string Password
    {
        get => _password;
        set => _password = value;
    }

    /// <summary>
    /// Адрес стенда
    /// </summary>
    public string Host
    {
        get => _host;
        set => _host = value;
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
    public Project Project
    {
        get => _project;
        set => _project = value;
    }

    /// <summary>
    /// Конструктор класса ServerInfo
    /// </summary>
    public ServerInfo() { }
}