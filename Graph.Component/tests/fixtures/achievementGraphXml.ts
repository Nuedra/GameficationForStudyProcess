export const achievementGraphXml = `<?xml version="1.0" encoding="UTF-8"?>
<graph dialect="base">
  <canvas width="420" height="260" />
  <background>
    <fill color="#f0f4ff" />
  </background>
  <node id="earned-node" type="circle" label="Старт" rotation="0">
    <status state="earned" />
    <geometry x="80" y="80" radius="24" />
    <background color="#111111" />
  </node>
  <node id="locked-node" type="rectangle" label="Экзамен" rotation="0">
    <status state="locked" />
    <geometry x="180" y="55" width="90" height="50" />
    <background color="#222222" />
  </node>
  <edge id="available-edge" type="line" label="" source="earned-node" target="locked-node" rotation="0">
    <status state="available" />
    <geometry startX="104" startY="80" endX="180" endY="80" />
    <background color="#333333" />
    <edgeStyle lineWidth="3" isEdgeDash="false" />
  </edge>
  <edge id="locked-edge" type="line" label="" sourceNodeId="locked-node" targetNodeId="earned-node" rotation="0">
    <status state="locked" />
    <geometry startX="225" startY="105" endX="80" endY="104" />
    <background color="#444444" />
    <edgeStyle lineWidth="2" isEdgeDash="false" />
  </edge>
</graph>`;

export const refreshedAchievementGraphXml = `<?xml version="1.0" encoding="UTF-8"?>
<graph dialect="base">
  <canvas width="420" height="260" />
  <background>
    <fill color="#f0f4ff" />
  </background>
  <node id="earned-node" type="circle" label="Старт" rotation="0">
    <status state="earned" />
    <geometry x="80" y="80" radius="24" />
    <background color="#111111" />
  </node>
  <node id="locked-node" type="rectangle" label="Экзамен" rotation="0">
    <status state="earned" />
    <geometry x="180" y="55" width="90" height="50" />
    <background color="#222222" />
  </node>
  <edge id="available-edge" type="line" label="" source="earned-node" target="locked-node" rotation="0">
    <status state="earned" />
    <geometry startX="104" startY="80" endX="180" endY="80" />
    <background color="#333333" />
    <edgeStyle lineWidth="3" isEdgeDash="false" />
  </edge>
</graph>`;
