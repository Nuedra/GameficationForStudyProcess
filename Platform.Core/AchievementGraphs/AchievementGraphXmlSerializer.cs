using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Platform.Core.AchievementGraphs;

public sealed class AchievementGraphXmlSerializer : IAchievementGraphXmlSerializer
{
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

    public string Serialize(
        string template,
        IReadOnlyCollection<AchievementGraphNodeState> nodeStates)
    {
        if (string.IsNullOrWhiteSpace(template))
            throw new AchievementGraphXmlException("Achievement graph XML template is empty.");

        ArgumentNullException.ThrowIfNull(nodeStates);

        var xml = ExtractGraphXml(template);
        var document = ParseXml(xml);
        var root = document.Root;

        if (root is null || root.Name.LocalName != "graph")
            throw new AchievementGraphXmlException("Achievement graph XML must contain a graph root element.");

        var statesById = nodeStates.ToDictionary(
            item => item.AchievementId,
            item => item.Status);
        var statesByGraphNodeId = nodeStates
            .Where(item => !string.IsNullOrWhiteSpace(item.GraphNodeId))
            .GroupBy(item => item.GraphNodeId!, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.First().Status,
                StringComparer.Ordinal);
        var statesByTitle = nodeStates
            .GroupBy(item => NormalizeLabel(item.Title), StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.First().Status,
                StringComparer.Ordinal);

        var nodeStatusesByXmlId = new Dictionary<string, AchievementGraphStatus>(StringComparer.Ordinal);

        foreach (var node in root.Elements("node"))
        {
            var status = ResolveNodeStatus(
                node,
                statesById,
                statesByGraphNodeId,
                statesByTitle);
            SetStatus(node, status);

            var xmlId = node.Attribute("id")?.Value;
            if (!string.IsNullOrWhiteSpace(xmlId))
                nodeStatusesByXmlId[xmlId] = status;
        }

        foreach (var edge in root.Elements("edge"))
        {
            var status = ResolveEdgeStatus(edge, nodeStatusesByXmlId);
            SetStatus(edge, status);
        }

        return document.ToString(SaveOptions.DisableFormatting);
    }

    private static string ExtractGraphXml(string template)
    {
        var start = template.IndexOf("<graph", StringComparison.Ordinal);
        var end = template.LastIndexOf("</graph>", StringComparison.Ordinal);

        if (start < 0 || end < 0 || end < start)
            throw new AchievementGraphXmlException("Template must contain a graph XML block.");

        return template[start..(end + "</graph>".Length)];
    }

    private static XDocument ParseXml(string xml)
    {
        try
        {
            return XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
        }
        catch (Exception exception) when (exception is not AchievementGraphXmlException)
        {
            throw new AchievementGraphXmlException(
                "Achievement graph XML template is invalid.",
                exception);
        }
    }

    private static AchievementGraphStatus ResolveNodeStatus(
        XElement node,
        IReadOnlyDictionary<Guid, AchievementGraphStatus> statesById,
        IReadOnlyDictionary<string, AchievementGraphStatus> statesByGraphNodeId,
        IReadOnlyDictionary<string, AchievementGraphStatus> statesByTitle)
    {
        var achievementId = GetAchievementId(node);
        if (achievementId.HasValue && statesById.TryGetValue(achievementId.Value, out var statusById))
            return statusById;

        var graphNodeId = node.Attribute("id")?.Value;
        if (!string.IsNullOrWhiteSpace(graphNodeId) &&
            statesByGraphNodeId.TryGetValue(graphNodeId, out var statusByGraphNodeId))
        {
            return statusByGraphNodeId;
        }

        var label = node.Attribute("label")?.Value;
        if (!string.IsNullOrWhiteSpace(label) &&
            statesByTitle.TryGetValue(NormalizeLabel(label), out var statusByTitle))
        {
            return statusByTitle;
        }

        return AchievementGraphStatus.Locked;
    }

    private static Guid? GetAchievementId(XElement node)
    {
        foreach (var attributeName in new[]
                 {
                     "achievementId",
                     "achievement-id",
                     "data-achievement-id"
                 })
        {
            var value = node.Attribute(attributeName)?.Value;
            if (Guid.TryParse(value, out var achievementId))
                return achievementId;
        }

        return null;
    }

    private static AchievementGraphStatus ResolveEdgeStatus(
        XElement edge,
        IReadOnlyDictionary<string, AchievementGraphStatus> nodeStatusesByXmlId)
    {
        var source = edge.Attribute("source")?.Value;
        var target = edge.Attribute("target")?.Value;

        if (string.IsNullOrWhiteSpace(source) ||
            string.IsNullOrWhiteSpace(target) ||
            !nodeStatusesByXmlId.TryGetValue(source, out var sourceStatus) ||
            !nodeStatusesByXmlId.TryGetValue(target, out var targetStatus))
        {
            return AchievementGraphStatus.Locked;
        }

        if (sourceStatus == AchievementGraphStatus.Earned &&
            targetStatus == AchievementGraphStatus.Earned)
        {
            return AchievementGraphStatus.Earned;
        }

        if (sourceStatus == AchievementGraphStatus.Earned &&
            targetStatus == AchievementGraphStatus.Available)
        {
            return AchievementGraphStatus.Available;
        }

        return AchievementGraphStatus.Locked;
    }

    private static void SetStatus(XElement element, AchievementGraphStatus status)
    {
        var statusElement = element.Element("status");
        if (statusElement is null)
        {
            statusElement = new XElement("status");
            var geometry = element.Element("geometry");

            if (geometry is not null)
                geometry.AddAfterSelf(statusElement);
            else
                element.AddFirst(statusElement);
        }

        statusElement.SetAttributeValue("state", ToXmlValue(status));
    }

    private static string ToXmlValue(AchievementGraphStatus status)
    {
        return status switch
        {
            AchievementGraphStatus.Locked => "locked",
            AchievementGraphStatus.Available => "available",
            AchievementGraphStatus.Earned => "earned",
            _ => "locked"
        };
    }

    private static string NormalizeLabel(string value)
    {
        return WhitespaceRegex.Replace(value.Replace("\\n", " "), " ").Trim();
    }
}
