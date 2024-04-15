using System.ComponentModel.DataAnnotations;

namespace SSTCommonClasses.Infrastructure;

/// <summary>
/// Класс описывает пользователей системы SST
/// </summary>
public class User
{
    private Guid? _uuid;
    private string? _name;
    private string? _password;
    private string? _email;
    private string? _login;

    /// <summary>
    /// Уникальный идентифкатор пользователя
    /// </summary>
    [Key]
    public Guid? Uuid
    {
        get => _uuid;
        set => _uuid = value;
    }

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string? Name
    {
        get => _name;
        set => _name = value;
    }

    /// <summary>
    /// Логин пользователя для входа в систему SST
    /// </summary>
    public string? Login
    {
        get => _login;
        set => _login = value;
    }

    /// <summary>
    /// Пароль пользователя для входа в систему SST
    /// </summary>
    public string? Password
    {
        get => _password;
        set => _password = value;
    }

    /// <summary>
    /// Рабочий Email пользователя
    /// </summary>
    public string? Email
    {
        get => _email;
        set => _email = value;
    }

    /// <summary>
    /// Конструктор класса User
    /// </summary>
    public User()
    {
    }
}