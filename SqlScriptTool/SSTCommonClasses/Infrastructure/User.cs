namespace SSTCommonClasses.Infrastructure;

public class User
{
    private Guid _uuid;
    private string _name;
    private string _password;
    private string _email;
    private string _login;

    public Guid Uuid
    {
        get => _uuid;
        set => _uuid = value;
    }

    public string Name
    {
        get => _name;
        set => _name = value;
    }

    public string Password
    {
        get => _password;
        set => _password = value;
    }

    public string Email
    {
        get => _email;
        set => _email = value;
    }

    public string Login
    {
        get => _login;
        set => _login = value;
    }

    public User() { }
}