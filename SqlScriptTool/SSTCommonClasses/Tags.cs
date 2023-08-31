namespace SSTCommonClasses
{
    public enum TagDataType
    {
        Int,
        Float,
        Date,
        DateTime,
        String,
        Char,
        LdObjectType
    }

    public enum TagType
    {
        UserParameter,
        ModelItemparent
    }

    public class TagTemplate
    {
        public Guid Uuid { get; set; }
        public string Name { get; set; }
        public string DisplayLabel { get; set; }
        public string Description { get; set; }
        public TagType TagType { get; set;}
        public TagDataType DataType { get; set; }
        public TagTemplate() { }
    }

    public class Tag
    {
        public Guid Uuid { get; set;} 
        public string Name { get; set; }
        public string DisplayLabel { get; set; }
        public string Description { get; set; }
        public TagType TagType { get; set;}
        public TagDataType DataType { get; set; }
        public TagTemplate TagTemplate { get; set; }
        public Tag() { }

    }
}