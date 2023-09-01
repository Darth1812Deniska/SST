using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SSTCommonClasses;

[Serializable]
public class LdObjectType
{
    public int ID { get; set; }
    public string Name { get; set; }
    public string DisplayLabel { get; set; }
    public string Description { get; set; }
    public Guid UID { get; set; }

    public LdObjectType()
    {
    }

    public LdObjectType(int iD, string name, string displayLabel, string description, Guid uid)
    {
        ID = iD;
        Name = name;
        DisplayLabel = displayLabel;
        Description = description;
        UID = uid;
    }

    public bool Equals(LdObjectType? other)
    {
        if (other == null)
            return false;
        return ID == other.ID;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        return obj is LdObjectType ldObjectType && Equals(ldObjectType);
    }

    public override string ToString()
    {
        return $"{ID} - {Name} ({DisplayLabel})";
    }
}