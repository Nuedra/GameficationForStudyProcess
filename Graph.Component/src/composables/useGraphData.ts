import { ref } from 'vue'

// XML данные из старого App.vue
export function useGraphData() {
    const xmlString = ref(`<graph>
            <canvas height="500" width="600"/>

            <node id="cloud1111" type="cloud" label="Sky is blue">
                <geometry x_center="-75" y_center="100"/>
                <background color="white"/>
                <labelSettings font="bold italic 14px Arial" color="black" />
            </node>

            <node id="star" type="star" label="">
                <geometry x_center="100" y_center="85"/>
                <background color=""/>
                <labelSettings font="14px Arial" color="black"/>
            </node>

            <node id="star2" type="star" label="">
                <geometry x_center="250" y_center="85"/>
                <background color="gold"/>
                <labelSettings font="10px Arial" color="black"/>
            </node>

            <customDescriptions>
                <customDescription type="cloud">
                    <points>
                        <point x="170" y="80"/>
                        <point x="230" y="150"/>
                        <point x="340" y="150"/>
                        <point x="390" y="100"/>
                        <point x="340" y="50"/>
                        <point x="250" y="50"/>
                        <point x="170" y="80"/>
                    </points>
                    <curve>
                        <curvePoint isCurved="true" cp1x="130" cp1y="100" cp2x="130" cp2y="150"/>
                        <curvePoint isCurved="true" cp1x="250" cp1y="180" cp2x="320" cp2y="180"/>
                        <curvePoint isCurved="true" cp1x="420" cp1y="150" cp2x="420" cp2y="120"/>
                        <curvePoint isCurved="true" cp1x="430" cp1y="40" cp2x="310" cp2y="10"/>
                        <curvePoint isCurved="true" cp1x="320" cp1y="5" cp2x="250" cp2y="20"/>
                        <curvePoint isCurved="true" cp1x="200" cp1y="5" cp2x="150" cp2y="20"/>
                    </curve>
                </customDescription>
                <customDescription type="star">
                    <points>
                        <point x="0" y="-30"/>
                        <point x="-5.510910596163089e-15" y="-30"/>
                        <point x="8.816778784387093" y="-12.135254915624213"/>
                        <point x="28.531695488854606" y="-9.27050983124843"/>
                        <point x="14.265847744427305" y="4.635254915624207"/>
                        <point x="17.6335575687742" y="24.270509831248418"/>
                        <point x="4.592425496802574e-15" y="15"/>
                        <point x="-17.633557568774187" y="24.27050983124843"/>
                        <point x="-14.265847744427301" y="4.635254915624217"/>
                        <point x="-28.53169548885461" y="-9.270509831248411"/>
                        <point x="-8.816778784387102" y="-12.135254915624209"/>
                        <point x="0" y="-30"/>
                    </points>
                    <curve>
                        <curvePoint isCurved="false" cp1x="0" cp1y="0" cp2x="0" cp2y="0"/>
                        <curvePoint isCurved="false" cp1x="0" cp1y="0" cp2x="0" cp2y="0"/>
                        <curvePoint isCurved="false" cp1x="0" cp1y="0" cp2x="0" cp2y="0"/>
                        <curvePoint isCurved="false" cp1x="0" cp1y="0" cp2x="0" cp2y="0"/>
                        <curvePoint isCurved="false" cp1x="0" cp1y="0" cp2x="0" cp2y="0"/>
                        <curvePoint isCurved="false" cp1x="0" cp1y="0" cp2x="0" cp2y="0"/>
                        <curvePoint isCurved="false" cp1x="0" cp1y="0" cp2x="0" cp2y="0"/>
                        <curvePoint isCurved="false" cp1x="0" cp1y="0" cp2x="0" cp2y="0"/>
                        <curvePoint isCurved="false" cp1x="0" cp1y="0" cp2x="0" cp2y="0"/>
                        <curvePoint isCurved="false" cp1x="0" cp1y="0" cp2x="0" cp2y="0"/>
                        <curvePoint isCurved="false" cp1x="0" cp1y="0" cp2x="0" cp2y="0"/>
                    </curve>
                </customDescription>
            </customDescriptions>

        </graph>`)
    const xmlString2 = ref(`<graph>
            <canvas height="400" width="400"/>

            <edge id="line3" type="line" label="Line" rotation="0" endArrow="none" startArrow="triangle">
                <geometry startX="150" startY="50" endX="250" endY="50"/>
                <background color=""/>
                <edgeStyle lineWidth="1" />
                <labelSettings font="14px Arial" color="black"/>
            </edge>

            <node id="circle1" type="circle" label="" rotation="0">
                <geometry x="150" y="100" radius="30"/>
                <background color="red"/>
                <labelSettings font="14px Arial" color="white"/>
            </node>

            <node id="rect1" type="rectangle" label="" rotation="0">
                <geometry x="200" y="150" width="80" height="40"/>
                <edgeStyle isEdgeDash = "true"/>
                <background color="blue"/>
                <labelSettings font="Bold 17px Arial" color="white"/>
            </node>

            <node id="tri1" type="triangle" label="" rotation="0">
                <geometry x1="100" y1="200" x2="130" y2="250" x3="70" y3="250"/>
                <background color="#ffcc00"/>
                <labelSettings font="18px Arial" color="black"/>
            </node>

            <node id="oval" type="ellipse" label="" rotation="20">
                <geometry x="250" y="300" radius_x="30" radius_y="60" />
                <background color="#ffcc00"/>
                <labelSettings font="18px Arial" color="black"/>
            </node>

            <node id="circle2" type="circle" label="" rotation="0">
                <geometry x="50" y="50" radius="20"/>
                <background color="green"/>
                <labelSettings font="14px Arial" color="white"/>
            </node>

            <node id="rect2" type="rectangle" label="" rotation="0">
                <geometry x="30" y="100" width="40" height="60"/>
                <background color="purple"/>
                <labelSettings font="Bold 17px Arial" color="white"/>
            </node>

            <node id="poly1" type="regular polygon" label="" rotation="0">
                <geometry x="150" y="100" radius="30" number_of_edges="5"/>
                <background color=""/>
                <labelSettings font="14px Arial" color="white"/>
            </node>

            <edge id="line_cracked" type="line" label="" rotation="0" endArrow="none" startArrow="none">
                <geometry startX="100" startY="280" endX="15" endY="373"/>
                <background color="black"/>
                <edgeStyle lineWidth="" isRounded= "true" isEdgeDash="false" maxRadiusOfCorners="10"/>
                <internalPoints>
                    <internalPoint x="50" y="280"/>
                </internalPoints>
            </edge>

        </graph>`)
    const xmlStr3 = ref(`<graph>
            <canvas height="370" width="700"/>

            <edge id="line1" type="line" label="" rotation="0" endArrow="none" startArrow="stick">
                <geometry startX="180" startY="70" endX="370" endY="100"/>
                <background color="black"/>
                <edgeStyle lineWidth="" />
                <labelSettings font="14px Arial" color="black"/>
            </edge>

            <edge id="line2" type="line" label="" rotation="0" endArrow="none" startArrow="none">
                <geometry startX="430" startY="100" endX="570" endY="50"/>
                <background color="black"/>
                <edgeStyle lineWidth="" />
                <labelSettings font="14px Arial" color="black"/>
            </edge>

            <edge id="line3" type="line" label="" rotation="0" endArrow="none" startArrow="none">
                <geometry startX="600" startY="80" endX="515" endY="173"/>
                <background color="black"/>
                <edgeStyle lineWidth="" />
                <labelSettings font="14px Arial" color="black"/>
            </edge>

            <edge id="line4" type="line" label="" rotation="0" endArrow="none" startArrow="none">
                <geometry startX="470" startY="200" endX="330" endY="300"/>
                <background color="black"/>
                <edgeStyle lineWidth="" />
                <labelSettings font="14px Arial" color="black"/>
            </edge>

            <edge id="line5" type="line" label="" rotation="0" endArrow="none" startArrow="none">
                <geometry startX="150" startY="100" endX="270" endY="300"/>
                <background color="black"/>
                <edgeStyle lineWidth="" />
                <labelSettings font="14px Arial" color="black"/>
            </edge>

            <node id="circle1" type="circle" label="" rotation="0"  info="you can click">
                <geometry x="150" y="70" radius="30"/>
                <background color="yellow"/>
                <labelSettings font="14px Arial" color="black"/>
            </node>

            <node id="circle2" type="circle" label="" rotation="0"  info="don't touch">
                <geometry x="400" y="100" radius="30"/>
                <background color="cyan"/>
                <labelSettings font="14px Arial" color="black"/>
            </node>

            <node id="circle3" type="circle" label="" rotation="0">
                <geometry x="600" y="50" radius="30"/>
                <background color="magenta"/>
                <labelSettings font="14px Arial" color="black"/>
            </node>

            <node id="circle4" type="circle" label="" rotation="0">
                <geometry x="500" y="200" radius="30"/>
                <background color="lime"/>
                <labelSettings font="14px Arial" color="black"/>
            </node>

            <node id="circle5" type="circle" label="" rotation="0">
                <geometry x="300" y="300" radius="30"/>
                <background color="orange"/>
                <labelSettings font="14px Arial" color="black"/>
            </node>
        </graph>`)
    const xmlStrClear = ref(`<graph>
            <canvas height="1000" width="1000"/>

        </graph>`)
    const xmlEgor = ref(`<graph>
            <canvas height="962" width="1512"/>

            <node dialect="base" id="V2Ptl4yMcGnV9rHh" type="rectangle" label="Отрисовка элементов \nна Canvas (Canvas API)" rotation="0">
                <geometry x="716" y="559" width="360" height="50" border="true"/>
                <background color="#ffffff"/>
                <style alpha="1" border="true"/>
                <arealRect x1="716" y1="559" x2="1076" y2="609"/>
                <lineConnectionStart>
                    <connection id_con="p3elRmug4s8GmZeP" id_shape="0yx7o2Z6c2QTOk9P"/>
                </lineConnectionStart>
                <lineConnectionEnd>
                    <connection id_con="7g0pSXtKqOyZMT6e" id_shape="nazExDVZJeqLD6A6"/>
                </lineConnectionEnd>
                <connectors>
                    <connector id="9gV2kHfu26AXm51s" x="716" y="584" type="left"/>
                    <connector id="C0v1BxjTxRCNFocO" x="1076" y="584" type="right"/>
                    <connector id="p3elRmug4s8GmZeP" x="896" y="559" type="top"/>
                    <connector id="7g0pSXtKqOyZMT6e" x="896" y="609" type="bottom"/>
                </connectors>
            </node>
            <node dialect="base" id="23yy2AOZecMk6mUk" type="rectangle" label="Формирование визуальных элементов для каждого Shape" rotation="0">
                <geometry x="670" y="455" width="460" height="50" border="true"/>
                <background color="#ffffff"/>
                <style alpha="1" border="true"/>
                <arealRect x1="670" y1="455" x2="1130" y2="505"/>
                <lineConnectionStart>
                  <connection id_con="H2qlfR59pfRuO86E" id_shape="VSgHXXf5rTyA11zp"/>
                </lineConnectionStart>
                <lineConnectionEnd>
                  <connection id_con="6myADrUKUY4qTKUt" id_shape="0yx7o2Z6c2QTOk9P"/>
                </lineConnectionEnd>
                <connectors>
                  <connector id="yWvITayUlFPv0tE8" x="670" y="480" type="left"/>
                  <connector id="ahTHLF5Vd3sRSsgq" x="1130" y="480" type="right"/>
                  <connector id="H2qlfR59pfRuO86E" x="900" y="455" type="top"/>
                  <connector id="6myADrUKUY4qTKUt" x="900" y="505" type="bottom"/>
                </connectors>
              </node>
              <node dialect="base" id="3dBl5uCY9Aa2lYVJ" type="rectangle" label="Разбор(парсинг) файла, создание объектов Shape" rotation="0">
                <geometry x="696" y="349" width="404" height="50" border="true"/>
                <background color="#ffffff"/>
                <style alpha="1" border="true"/>
                <arealRect x1="696" y1="349" x2="1100" y2="399"/>
                <lineConnectionStart>
                  <connection id_con="4WUou0gEufhYeURV" id_shape="BjKPCylyBFRQYZNY"/>
                </lineConnectionStart>
                <lineConnectionEnd>
                  <connection id_con="D1DhLfd8aG7eZUwp" id_shape="VSgHXXf5rTyA11zp"/>
                </lineConnectionEnd>
                <connectors>
                  <connector id="fKSosh6Qn8N3bjTq" x="696" y="374" type="left"/>
                  <connector id="ma8k7pfCltqOkGI0" x="1100" y="374" type="right"/>
                  <connector id="4WUou0gEufhYeURV" x="898" y="349" type="top"/>
                  <connector id="D1DhLfd8aG7eZUwp" x="898" y="399" type="bottom"/>
                </connectors>
              </node>
              <node dialect="base" id="3yTPzJ1yE93IXKGn" type="rectangle" label="Чтение описания графа" rotation="0">
                <geometry x="800" y="249" width="194" height="50" border="true"/>
                <background color="#ffffff"/>
                <style alpha="1" border="true"/>
                <arealRect x1="800" y1="249" x2="994" y2="299"/>
                <lineConnectionStart>
                  <connection id_con="yTP3uAqOZSRSqwW6" id_shape="FH08Wtqc51HPxXDm"/>
                </lineConnectionStart>
                <lineConnectionEnd>
                  <connection id_con="XVSJwgOaCAAzCPfO" id_shape="BjKPCylyBFRQYZNY"/>
                </lineConnectionEnd>
                <connectors>
                  <connector id="KMKEPFMn6sevElkJ" x="800" y="274" type="left"/>
                  <connector id="Ehn3Nxrky7Y26IDo" x="994" y="274" type="right"/>
                  <connector id="yTP3uAqOZSRSqwW6" x="897" y="249" type="top"/>
                  <connector id="XVSJwgOaCAAzCPfO" x="897" y="299" type="bottom"/>
                </connectors>
              </node>
              <node dialect="base" id="Ks5gjYfdhk0XVeIi" type="circle" label="" rotation="0">
                <geometry x="898" y="689" radius="25"/>
                <background color="#000000"/>
                <style alpha="1" border="false"/>
                <arealRect x1="871" y1="662" x2="925" y2="716"/>
                <lineConnectionStart>
                  <connection id_con="oKQlrCgo3zLPimlo" id_shape="nazExDVZJeqLD6A6"/>
                </lineConnectionStart>
                <lineConnectionEnd>
                </lineConnectionEnd>
                <connectors>
                  <connector id="UKUToL7f5Hv5JBqy" x="873" y="689" type="left"/>
                  <connector id="Cp7zgGIBG2TM0OMd" x="923" y="689" type="right"/>
                  <connector id="oKQlrCgo3zLPimlo" x="898" y="664" type="top"/>
                  <connector id="j9BXRtSRcEdQVpHF" x="898" y="714" type="bottom"/>
                </connectors>
              </node>
              <node dialect="base" id="EJU8tnSBCC7Jz4If" type="circle" label="" rotation="0">
                <geometry x="896" y="191" radius="25"/>
                <background color="#000000"/>
                <style alpha="1" border="false"/>
                <arealRect x1="869" y1="164" x2="923" y2="218"/>
                <lineConnectionStart>
                </lineConnectionStart>
                <lineConnectionEnd>
                  <connection id_con="CqJsR1lei3t5dRpe" id_shape="FH08Wtqc51HPxXDm"/>
                </lineConnectionEnd>
                <connectors>
                  <connector id="0MEWDICyon24aqRU" x="871" y="191" type="left"/>
                  <connector id="eQmhNsmHbSnmBxIq" x="921" y="191" type="right"/>
                  <connector id="iA1oVyI7wBvQIqW4" x="896" y="166" type="top"/>
                  <connector id="CqJsR1lei3t5dRpe" x="896" y="216" type="bottom"/>
                </connectors>
              </node>
              <edge dialect="base" id="BjKPCylyBFRQYZNY" type="line" label="" source="3dBl5uCY9Aa2lYVJ" target="3yTPzJ1yE93IXKGn" endArrow=">" startArrow="none">
                <lineGeometry startX="898" startY="351" endX="897" endY="301"/>
                <background color="#000000"/>
                <edgeStyle edgeColor="#000000" lineWidth="1" alpha="1"/>
                <lineConnectionStart>
                  <connection id_con="4WUou0gEufhYeURV" id_shape="3dBl5uCY9Aa2lYVJ"/>
                </lineConnectionStart>
                <lineConnectionEnd>
                  <connection id_con="XVSJwgOaCAAzCPfO" id_shape="3yTPzJ1yE93IXKGn"/>
                </lineConnectionEnd>
              </edge>
              <edge dialect="base" id="VSgHXXf5rTyA11zp" type="line" label="" source="23yy2AOZecMk6mUk" target="3dBl5uCY9Aa2lYVJ" endArrow=">" startArrow="none">
                <lineGeometry startX="900" startY="457" endX="898" endY="401"/>
                <background color="#000000"/>
                <edgeStyle edgeColor="#000000" lineWidth="1" alpha="1"/>
                <lineConnectionStart>
                  <connection id_con="H2qlfR59pfRuO86E" id_shape="23yy2AOZecMk6mUk"/>
                </lineConnectionStart>
                <lineConnectionEnd>
                  <connection id_con="D1DhLfd8aG7eZUwp" id_shape="3dBl5uCY9Aa2lYVJ"/>
                </lineConnectionEnd>
              </edge>
              <edge dialect="base" id="nazExDVZJeqLD6A6" type="line" label="" source="Ks5gjYfdhk0XVeIi" target="V2Ptl4yMcGnV9rHh" endArrow=">" startArrow="none">
                <lineGeometry startX="900" startY="664" endX="898" endY="609"/>
                <background color="#000000"/>
                <edgeStyle edgeColor="#000000" lineWidth="1" alpha="1"/>
                <lineConnectionStart>
                  <connection id_con="oKQlrCgo3zLPimlo" id_shape="Ks5gjYfdhk0XVeIi"/>
                </lineConnectionStart>
                <lineConnectionEnd>
                  <connection id_con="7g0pSXtKqOyZMT6e" id_shape="V2Ptl4yMcGnV9rHh"/>
                </lineConnectionEnd>
              </edge>
              <edge dialect="base" id="0yx7o2Z6c2QTOk9P" type="line" label="" source="V2Ptl4yMcGnV9rHh" target="23yy2AOZecMk6mUk" endArrow=">" startArrow="none">
                <lineGeometry startX="898" startY="559" endX="900" endY="507"/>
                <background color="#000000"/>
                <edgeStyle edgeColor="#000000" lineWidth="1" alpha="1"/>
                <lineConnectionStart>
                  <connection id_con="p3elRmug4s8GmZeP" id_shape="V2Ptl4yMcGnV9rHh"/>
                </lineConnectionStart>
                <lineConnectionEnd>
                  <connection id_con="6myADrUKUY4qTKUt" id_shape="23yy2AOZecMk6mUk"/>
                </lineConnectionEnd>
              </edge>
              <edge dialect="base" id="FH08Wtqc51HPxXDm" type="line" label="" source="3yTPzJ1yE93IXKGn" target="EJU8tnSBCC7Jz4If" endArrow=">" startArrow="none">
                <lineGeometry startX="897" startY="251" endX="898" endY="216"/>
                <background color="#000000"/>
                <edgeStyle edgeColor="#000000" lineWidth="1" alpha="1"/>
                <lineConnectionStart>
                  <connection id_con="yTP3uAqOZSRSqwW6" id_shape="3yTPzJ1yE93IXKGn"/>
                </lineConnectionStart>
                <lineConnectionEnd>
                  <connection id_con="CqJsR1lei3t5dRpe" id_shape="EJU8tnSBCC7Jz4If"/>
                </lineConnectionEnd>
              </edge>
        </graph>`)
    const xmlEgor2 = ref(`<graph>
            <canvas height="962" width="1912"/>
            <edge dialect="base" id="FH08Wtqc51HPxXDm" type="line" label="" source="3yTPzJ1yE93IXKGn" target="EJU8tnSBCC7Jz4If" endArrow=">" startArrow="none">
                <lineGeometry startX="897" startY="251" endX="898" endY="216"/>
                <background color="#000000"/>
                <edgeStyle edgeColor="#000000" lineWidth="1" alpha="1"/>
                <lineConnectionStart>
                  <connection id_con="yTP3uAqOZSRSqwW6" id_shape="3yTPzJ1yE93IXKGn"/>
                </lineConnectionStart>
                <lineConnectionEnd>
                  <connection id_con="CqJsR1lei3t5dRpe" id_shape="EJU8tnSBCC7Jz4If"/>
                </lineConnectionEnd>
             </edge>
        </graph>`)
    const dialectXML = ref(`<dialect name="oriented">
          <Nodes>
            <allowedNode type="circle" />
            <allowedNode type="sun">
                <basedOnType type="circle" />
            </allowedNode>
            <allowedNode type="rectangle" />
            <allowedNode type="triangle" />
          </Nodes>
          <Edges>
            <allowedEdge type="line" />
          </Edges>
          <Arrowheads>
            <allowedArrowhead type="triangle" />
            <allowedArrowhead type="stick" />
          </Arrowheads>
          <graphSettings>
            <edgeSettings requireAllArrow="start/end/both" requireAllTarget="true" requireAllSource="true" />
            <edgeStyleSettings lineDash="none" />
            <labelSettings font="18px Arial" />
          </graphSettings>

          <allowedConnections>
            <connection>
              <nodeType name="rectangle" position="both"/>
              <nodeType name="triangle" position="both"/>
              <edgeType name="line" />
              <arrowheadType name="triangle" position="any"/>
            </connection>
            <connection>
              <nodeType name="circle" position="both"/>
              <nodeType name="triangle" position="both"/>
              <edgeType name="line" />
              <arrowheadType name="triangle" position="any"/>
            </connection>
          </allowedConnections>
        </dialect>`)
    const xmlStrDialectTest = ref(`<graph dialect="oriented">
            <canvas height="370" width="700"/>

            <edge id="line1" type="line" label="привет\n как ты?" rotation="0" endArrow="none" startArrow="stick" >
                <geometry startX="180" startY="70" endX="370" endY="100"/>
                <background color="black"/>
                <edgeStyle lineWidth="" />
                <labelSettings font="14px Arial" color="black"/>
            </edge>

            <node id="circle1" type="sun" label="привет \n как ты?" rotation="0" info="я жииив">
                <geometry x="150" y="70" radius="30"/>
                <background color="yellow"/>
                <labelSettings font="14px Arial" color="black"/>
            </node>

            <node id="circle2" type="circle" label="" rotation="0">
                <geometry x="400" y="100" radius="30"/>
                <background color="cyan"/>
                <labelSettings font="14px Arial" color="black"/>
            </node>

        </graph>`)

    return {
        xmlString,
        xmlString2,
        xmlStr3,
        xmlStrClear,
        xmlEgor,
        xmlEgor2,
        dialectXML,
        xmlStrDialectTest
    }
}