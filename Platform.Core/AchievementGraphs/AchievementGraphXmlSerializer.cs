using System.Xml.Linq;

namespace Platform.Core.AchievementGraphs;

public sealed class AchievementGraphXmlSerializer : IAchievementGraphXmlSerializer
{
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

        var nodeStatusesByXmlId = new Dictionary<string, AchievementGraphStatus>(StringComparer.Ordinal);

        foreach (var node in root.Elements("node"))
        {
            var status = ResolveNodeStatus(node, statesById);
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
        IReadOnlyDictionary<Guid, AchievementGraphStatus> statesById)
    {
        var achievementId = GetAchievementId(node);
        if (achievementId.HasValue && statesById.TryGetValue(achievementId.Value, out var statusById))
            return statusById;

        return AchievementGraphStatus.Locked;
    }

    private static Guid? GetAchievementId(XElement node)
    {
        foreach (var attributeName in new[]
                 {
                     "achievementId",
                     "AchievementId",
                     "achievement-id",
                     "data-achievement-id",
                     "achivementId",
                     "AchivementId"
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
}
