using System.ComponentModel.DataAnnotations;
using System;
using SSTCommonClasses.Constants;

namespace SSTCommonClasses.Constructor;

public class ConstructorItem
{
    private Guid _uuid = Guid.Empty;
    private string _name = string.Empty;

    [Key]
    public Guid Uuid
    {
        get => _uuid == Guid.Empty ? _uuid = Guid.NewGuid() : _uuid;
        set => _uuid = value;
    }

    public string Name
    {
        get => _name;
        set => _name = value;
    }

    public string? Description { get; set; }
    public string? DisplayLabel { get; set; }
    public AccessType AccessType { get; set; }
}