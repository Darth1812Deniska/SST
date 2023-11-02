namespace SSTCommonClasses.Infrastructure;

public class Project
{
    private Guid _uuid;
    private string _name;

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

    public Project() { }
}