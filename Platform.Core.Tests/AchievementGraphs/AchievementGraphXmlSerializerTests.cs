using System.Xml.Linq;
using Platform.Core.AchievementGraphs;

namespace Platform.Core.Tests.AchievementGraphs;

public sealed class AchievementGraphXmlSerializerTests
{
    private static readonly Guid EarnedAchievementId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AvailableAchievementId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly AchievementGraphXmlSerializer _serializer = new();

    [Fact]
    public void Serialize_UpdatesNodeStatusesByAchievementId()
    {
        var xml = _serializer.Serialize(
            """
            <graph>
              <node id="node-1" achievementId="11111111-1111-1111-1111-111111111111" label="Первое достижение">
                <geometry x="0" y="0"/>
                <status state="locked"/>
              </node>
            </graph>
            """,
            [
                new AchievementGraphNodeState(
                    EarnedAchievementId,
                    AchievementGraphStatus.Earned)
            ]);

        var document = XDocument.Parse(xml);
        Assert.Equal("earned", GetNodeStatus(document, "node-1"));
    }

    [Fact]
    public void Serialize_DoesNotUseLabelAsAchievementIdentity()
    {
        var xml = _serializer.Serialize(
            """
            const xmlAchievementsGraph = `
            <graph>
              <node id="node-1" label="Первый коммит">
                <geometry x="0" y="0"/>
                <status state="locked"/>
              </node>
            </graph>
            `;
            """,
            [
                new AchievementGraphNodeState(
                    EarnedAchievementId,
                    AchievementGraphStatus.Available)
            ]);

        var document = XDocument.Parse(xml);
        Assert.Equal("locked", GetNodeStatus(document, "node-1"));
    }

    [Fact]
    public void Serialize_AddsMissingStatusElement()
    {
        var xml = _serializer.Serialize(
            """
            <graph>
              <node id="node-1" AchivementId="11111111-1111-1111-1111-111111111111" label="Первая ачивка">
                <geometry x="0" y="0"/>
              </node>
            </graph>
            """,
            [
                new AchievementGraphNodeState(
                    EarnedAchievementId,
                    AchievementGraphStatus.Earned)
            ]);

        var document = XDocument.Parse(xml);
        Assert.Equal("earned", GetNodeStatus(document, "node-1"));
    }

    [Fact]
    public void Serialize_UnknownNodeBecomesLocked()
    {
        var xml = _serializer.Serialize(
            """
            <graph>
              <node id="node-1" label="Неизвестная ачивка">
                <geometry x="0" y="0"/>
                <status state="earned"/>
              </node>
            </graph>
            """,
            []);

        var document = XDocument.Parse(xml);
        Assert.Equal("locked", GetNodeStatus(document, "node-1"));
    }

    [Fact]
    public void Serialize_UpdatesEdgesFromNodeStatuses()
    {
        var xml = _serializer.Serialize(
            """
            <graph>
              <node id="node-1" achievementId="11111111-1111-1111-1111-111111111111" label="Первая">
                <geometry x="0" y="0"/>
                <status state="locked"/>
              </node>
              <node id="node-2" achievementId="22222222-2222-2222-2222-222222222222" label="Вторая">
                <geometry x="0" y="0"/>
                <status state="locked"/>
              </node>
              <node id="node-3" label="Третья">
                <geometry x="0" y="0"/>
                <status state="locked"/>
              </node>
              <edge id="edge-1" source="node-1" target="node-2">
                <status state="locked"/>
              </edge>
              <edge id="edge-2" source="node-2" target="node-3">
                <status state="locked"/>
              </edge>
            </graph>
            """,
            [
                new AchievementGraphNodeState(
                    EarnedAchievementId,
                    AchievementGraphStatus.Earned),
                new AchievementGraphNodeState(
                    AvailableAchievementId,
                    AchievementGraphStatus.Available)
            ]);

        var document = XDocument.Parse(xml);
        Assert.Equal("available", GetEdgeStatus(document, "edge-1"));
        Assert.Equal("locked", GetEdgeStatus(document, "edge-2"));
    }

    [Fact]
    public void Serialize_InvalidTemplateThrowsReadableException()
    {
        var exception = Assert.Throws<AchievementGraphXmlException>(
            () => _serializer.Serialize("not xml", []));

        Assert.Contains("graph XML block", exception.Message);
    }

    private static string GetNodeStatus(XDocument document, string nodeId)
    {
        return document
            .Root!
            .Elements("node")
            .Single(node => node.Attribute("id")?.Value == nodeId)
            .Element("status")!
            .Attribute("state")!
            .Value;
    }

    private static string GetEdgeStatus(XDocument document, string edgeId)
    {
        return document
            .Root!
            .Elements("edge")
            .Single(edge => edge.Attribute("id")?.Value == edgeId)
            .Element("status")!
            .Attribute("state")!
            .Value;
    }
}
