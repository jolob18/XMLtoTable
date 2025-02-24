using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using xmlobjects.Models;

namespace xmlobjects.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class XmlDataController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetXmlData()
        {
            string xmlUrl = "https://receiptservice.egretail.cloud/ARTSPOSLogSchema/6.0.0";

            // Fetch XML data from the URL
            string xmlContent;
            using (HttpClient client = new HttpClient())
            {
                xmlContent = await client.GetStringAsync(xmlUrl);
            }

            // Load the XML document
            XDocument doc = XDocument.Parse(xmlContent);

            // Namespace for XML schema
            XNamespace xs = "http://www.w3.org/2001/XMLSchema";

            var types = new List<TypeViewModel>();

            // Parse simpleType elements
            var simpleTypes = doc.Descendants(xs + "simpleType")
                .Where(type => type.Attribute("name") != null) // Ensure simpleType has a name
                .Select(type => new TypeViewModel
                {
                    Type = "SimpleType",
                    Name = type.Attribute("name")?.Value,
                    Documentation = CleanDocumentation(type.Element(xs + "annotation")?.Element(xs + "documentation")?.Value),
                    Enumerations = type.Descendants(xs + "enumeration")
                        .Select(e => new ElementViewModel
                        {
                            Name = e.Attribute("value")?.Value,
                            Documentation = CleanDocumentation(e.Element(xs + "annotation")?.Element(xs + "documentation")?.Value)
                        })
                        .Where(e => !string.IsNullOrEmpty(e.Name) && !string.IsNullOrEmpty(e.Documentation)) // Filter enumerations without value or documentation
                        .ToList(),
                    Attributes = new List<AttributeViewModel>() // SimpleTypes won't have attributes
                })
                .Where(t => !string.IsNullOrEmpty(t.Name) && !string.IsNullOrEmpty(t.Documentation) || t.Enumerations.Any()) // Exclude simpleTypes without name or documentation
                .ToList();

            types.AddRange(simpleTypes);

            // Parse complexType elements
            var complexTypes = doc.Descendants(xs + "complexType")
                 .Where(type => type.Attribute("name") != null) // Ensure complexType has a name
                 .Select(type => new TypeViewModel
                 {
                     Type = "ComplexType",
                     Name = type.Attribute("name")?.Value,
                     Documentation = CleanDocumentation(type.Element(xs + "annotation")?.Element(xs + "documentation")?.Value),
                     Enumerations = new List<ElementViewModel>(), // ComplexTypes won't have enumerations
                     Attributes = type.Descendants(xs + "attribute")
                         .Select(a => new AttributeViewModel
                         {
                             Name = a.Attribute("name")?.Value,
                             Documentation = CleanDocumentation(a.Element(xs + "annotation")?.Element(xs + "documentation")?.Value)
                         })
                         .Where(a => !string.IsNullOrEmpty(a.Name) && !string.IsNullOrEmpty(a.Documentation)) // Only include attributes with names and documentation
                         .ToList(),
                     Elements = type.Descendants(xs + "element")
                         .Select(e => new ElementViewModel
                         {
                             Name = e.Attribute("name")?.Value,
                             Documentation = CleanDocumentation(e.Element(xs + "annotation")?.Element(xs + "documentation")?.Value)
                         })
                         .Where(e => !string.IsNullOrEmpty(e.Name) && !string.IsNullOrEmpty(e.Documentation)) // Only include elements with names and documentation
                         .ToList()
                 })
                 .Where(t =>
                     (
                         // ComplexType with name and documentation
                         (!string.IsNullOrEmpty(t.Name) && !string.IsNullOrEmpty(t.Documentation)) ||

                         // ComplexType without documentation but child elements or attributes with documentation
                         (string.IsNullOrEmpty(t.Documentation) &&
                             (t.Attributes.Any(a => !string.IsNullOrEmpty(a.Documentation)) ||
                              t.Elements.Any(e => !string.IsNullOrEmpty(e.Documentation))))
                     ) ||
                     // Include complexType with attributes or elements with documentation
                     t.Attributes.Any() || t.Elements.Any())
                 .ToList();

            types.AddRange(complexTypes);

            return Ok(types);
        }

        // Utility method to clean up documentation text
        private string CleanDocumentation(string rawDocumentation)
        {
            return string.IsNullOrWhiteSpace(rawDocumentation) ? null : rawDocumentation.Trim();
        }
    }
}
