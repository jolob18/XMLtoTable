namespace xmlobjects.Models
{
    // Common base class for both SimpleType and ComplexType
    public class TypeViewModel
    {
        public string Type { get; set; } // SimpleType or ComplexType
        public string Name { get; set; } // Name of the type
        public string Documentation { get; set; } // Documentation for the type
        public List<ElementViewModel> Enumerations { get; set; } // Enumerations for SimpleTypes
        public List<AttributeViewModel> Attributes { get; set; } // Attributes for ComplexTypes
        public List<ElementViewModel> Elements { get; set; }

    }

    // Class for Enumerations (used in SimpleType)
    public class ElementViewModel
    {
        public string Name { get; set; } // Name of the enumeration value
        public string Documentation { get; set; } // Documentation for the enumeration value
    }

    // Class for Attributes (used in ComplexType)
    public class AttributeViewModel
    {
        public string Name { get; set; } // Name of the attribute
        public string Documentation { get; set; } // Documentation for the attribute
    }
}
