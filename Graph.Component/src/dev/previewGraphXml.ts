export const initialPreviewGraphXml = `<?xml version="1.0" encoding="UTF-8"?>
<graph dialect="base">
  <canvas width="920" height="520" />
  <background>
    <fill color="#f7fafc" />
  </background>

  <node id="intro" type="circle" label="Старт" rotation="0">
    <status state="earned" />
    <geometry x="150" y="250" radius="45" />
    <background color="#4caf50" />
    <labelSettings color="#111827" font="16px Arial" />
  </node>

  <node id="practice" type="rectangle" label="Практика" rotation="0">
    <status state="available" />
    <geometry x="360" y="205" width="150" height="90" />
    <background color="#ffd54f" />
    <labelSettings color="#111827" font="16px Arial" />
  </node>

  <node id="exam" type="rectangle" label="Экзамен" rotation="0">
    <status state="locked" />
    <geometry x="660" y="205" width="150" height="90" />
    <background color="#cfd8dc" />
    <labelSettings color="#111827" font="16px Arial" />
  </node>

  <edge id="intro-practice" type="line" label="" source="intro" target="practice" rotation="0">
    <status state="available" />
    <geometry startX="195" startY="250" endX="360" endY="250" />
    <background color="#f9a825" />
    <edgeStyle lineWidth="4" isEdgeDash="false" />
  </edge>

  <edge id="practice-exam" type="line" label="" source="practice" target="exam" rotation="0">
    <status state="locked" />
    <geometry startX="510" startY="250" endX="660" endY="250" />
    <background color="#90a4ae" />
    <edgeStyle lineWidth="4" isEdgeDash="false" />
  </edge>
</graph>`;

export const refreshedPreviewGraphXml = `<?xml version="1.0" encoding="UTF-8"?>
<graph dialect="base">
  <canvas width="920" height="520" />
  <background>
    <fill color="#f7fafc" />
  </background>

  <node id="intro" type="circle" label="Старт" rotation="0">
    <status state="earned" />
    <geometry x="150" y="250" radius="45" />
    <background color="#4caf50" />
    <labelSettings color="#111827" font="16px Arial" />
  </node>

  <node id="practice" type="rectangle" label="Практика" rotation="0">
    <status state="earned" />
    <geometry x="360" y="205" width="150" height="90" />
    <background color="#4caf50" />
    <labelSettings color="#111827" font="16px Arial" />
  </node>

  <node id="exam" type="rectangle" label="Экзамен" rotation="0">
    <status state="available" />
    <geometry x="660" y="205" width="150" height="90" />
    <background color="#ffd54f" />
    <labelSettings color="#111827" font="16px Arial" />
  </node>

  <edge id="intro-practice" type="line" label="" source="intro" target="practice" rotation="0">
    <status state="earned" />
    <geometry startX="195" startY="250" endX="360" endY="250" />
    <background color="#2e7d32" />
    <edgeStyle lineWidth="4" isEdgeDash="false" />
  </edge>

  <edge id="practice-exam" type="line" label="" source="practice" target="exam" rotation="0">
    <status state="available" />
    <geometry startX="510" startY="250" endX="660" endY="250" />
    <background color="#f9a825" />
    <edgeStyle lineWidth="4" isEdgeDash="false" />
  </edge>
</graph>`;
