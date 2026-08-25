using System.Xml;
using System.Xml.Linq;

namespace PackageReferencesCollector;

public static class XDocumentExtensions
{
    public static IEnumerable<XElement> GetNodeAndDescendants(this XDocument xDoc) // Note that this method is lazy
    {
        return xDoc.Elements().SelectMany(x => x.GetNodeAndDescendants());
    }

    public static IEnumerable<XElement> GetNodeAndDescendants(this XElement xElement) // Note that this method is lazy
    {
        return new[] { xElement }
               .Concat(xElement.Elements().SelectMany(child => child.GetNodeAndDescendants()));
    }

    public static XmlDocument ToXmlDocument(this XDocument xDocument)
    {
        var xmlDocument = new XmlDocument();
        using (var xmlReader = xDocument.CreateReader())
        {
            xmlDocument.Load(xmlReader);
        }
        return xmlDocument;
    }

    public static XDocument ToXDocument(this XmlDocument xmlDocument)
    {
        using (var nodeReader = new XmlNodeReader(xmlDocument))
        {
            nodeReader.MoveToContent();
            return XDocument.Load(nodeReader);
        }
    }
}
