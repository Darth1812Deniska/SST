using System.ComponentModel.DataAnnotations;
using System;

namespace SSTCommonClasses.Constructor;

public class ConstructorItem
{
    private Guid _uuid = Guid.Empty;
    [Key]
    public Guid Uuid
    {
        get => _uuid == Guid.Empty ? _uuid = Guid.NewGuid() : _uuid;
        set => _uuid = value;
    }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? DisplayLabel { get; set; }
}