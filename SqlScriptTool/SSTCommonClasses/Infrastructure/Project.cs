namespace SSTCommonClasses.Infrastructure;

/// <summary>
/// Класс описывает проект в рамках которого ведется рабочая деятельность
/// </summary>
public class Project
{
    private Guid _uuid;
    private string _name;

    /// <summary>
    /// Уникальный идентификатор проекта
    /// </summary>
    public Guid Uuid
    {
        get => _uuid;
        set => _uuid = value;
    }

    /// <summary>
    /// Наименование проекта
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    /// <summary>
    /// Конструктор класса Project
    /// </summary>
    public Project() { }
}