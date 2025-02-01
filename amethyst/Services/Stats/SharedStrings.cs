using System.ComponentModel;
using System.Xml.Schema;
using System.Xml.Serialization;
// ReSharper disable InconsistentNaming
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.

namespace amethyst.Services.Stats;

[Serializable]
[XmlType(AnonymousType = true, Namespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main")]
[XmlRoot(Namespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main", IsNullable = false)]
public class sst
{

    [XmlElement("si")]
    public sstSI[] si { get; set; }

    [XmlAttribute]
    public int count { get; set; }

    [XmlAttribute]
    public int uniqueCount { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main")]
public class sstSI
{
    [XmlElement("r")]
    public sstSIR[] r { get; set; }

    public sstSIT t { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main")]
public class sstSIR
{
    public sstSIRRPr rPr { get; set; }
    public sstSIRT t { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main")]
public class sstSIRRPr
{
    public object b { get; set; }
    public sstSIRRPrSZ sz { get; set; }
    public sstSIRRPrRFont rFont { get; set; }
    public sstSIRRPrFamily family { get; set; }
    public sstSIRRPrScheme scheme { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main")]
public class sstSIRRPrSZ
{
    [XmlAttribute]
    public byte val { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main")]
public class sstSIRRPrRFont
{
    [XmlAttribute]
    public string val { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main")]
public class sstSIRRPrFamily
{
    [XmlAttribute]
    public byte val { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main")]
public class sstSIRRPrScheme
{
    [XmlAttribute]
    public string val { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main")]
public class sstSIRT
{
    [XmlAttribute(Form = XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace")]
    public string space { get; set; }

    [XmlText]
    public string Value { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main")]
public class sstSIT
{
    [XmlAttribute(Form = XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace")]
    public string space { get; set; }

    [XmlText]
    public string Value { get; set; }
}
