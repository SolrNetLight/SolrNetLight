using System.Xml.Linq;

namespace SolrNetLight.Impl {
    /// <summary>
    /// Parses the extract response
    /// </summary>
    public interface ISolrExtractResponseParser {
        ExtractResponse Parse(XDocument response);
    }
}