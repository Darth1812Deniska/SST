namespace SSTCommonClasses.Infrastructure;

public class Stand
{
    private Guid _uuid;
    private string _name;
    private string _login;
    private string _password;
    private string _host;
    private Project _project;

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

    public string Login
    {
        get => _login;
        set => _login = value;
    }

    public string Password
    {
        get => _password;
        set => _password = value;
    }

    public string Host
    {
        get => _host;
        set => _host = value;
    }

    public Project Project
    {
        get => _project;
        set => _project = value;
    }

    public Stand() { }
}