import * as img from './images';
import * as Graph from './Graph';

export function test(ctx: CanvasRenderingContext2D, graph: Graph.Graph, graph_figures: Graph.DataShapes[]) {
    const check_test = false;

    if (check_test) {
        //const line: Graph.ILine = { id: "efefef", type: "line", startX: 250, startY: 350, endX: 650, endY: 350, color: "purple", points: [{ x: 300, y: 300 }, { x: 350, y: 350 }, { x: 300, y: 400 }] }
        //const new_line = new Graph.Line(line, "none", "triangle")
        //new_line.draw_canvas(ctx);

        const label0: Graph.ILabel = { text: "Nod", color: 'gold', font: "14px Arial", padding: 10 }
        const circle: Graph.ICircle = { id: "sddddd", type: "circle", x: 350, y: 350, radius: 30, color: "blue", isEdgeDash: true, label_info: label0 }
        const new_circle = new Graph.Circle(circle);
        new_circle.draw_canvas(ctx)
        graph.addNode(new_circle);
        graph_figures.push(new_circle)


        //const label: Graph.ILabel = { text: "Broken", color: 'black', font: "14px Arial", padding: 10, alignment: 'left' }
        const line: Graph.ILine = { id: "efefef", type: "line", startX: 650, startY: 120, endX: 650, endY: 50, color: "purple", isEdgeDash: true, /*label_info: label,*/ points: [{ x: 600, y: 80 }, { x: 600, y: 70 }] }
        const new_line = new Graph.Line(line, "stick", "triangle", new_circle)
        new_line.draw_canvas(ctx);
        graph.addEdge(new_line);
        graph_figures.push(new_line);

        const line2: Graph.ILine = { id: "fofofo", type: "line", startX: 670, startY: 120, endX: 700, endY: 50, color: "", isEdgeDash: true }
        const new_line2 = new Graph.Line(line2, "crow", "line")
        new_line2.draw_canvas(ctx);
        graph.addEdge(new_line2);
        graph_figures.push(new_line2);

        //const label2: Graph.ILabel = { text: "Red", color: 'red', font: "18px Arial", padding: 10 }
        //const rectangle_data: Graph.IRectangle = { id: "reeeedddd", type: "rectangle", x: 550, y: 200, width: 100, height: 60, color: "green", label_info: label2 }
        //const new_rect = new Graph.Rectangle(rectangle_data);
        //new_rect.draw_canvas(ctx)

        const label3: Graph.ILabel = { text: "Triangle", color: 'white', font: "Bold 18px Arial", padding: 10 }
        const triangle_data: Graph.ITriangle = { id: "dsdvcdsvs", type: "triangle", x_1: 210, y_1: 210, x_2: 310, y_2: 300, x_3: 110, y_3: 350, isEdgeDash: true, color: "#621a6e", label_info: label3 }
        const new_tri = new Graph.Triangle(triangle_data);
        graph.addNode(new_tri);
        new_tri.draw_canvas(ctx)
        graph_figures.push(new_tri)


        const label4: Graph.ILabel = { text: "Coding...", color: 'black', font: "14px Arial", padding: 10 }
        const poly_data: Graph.IRegularPolygon = { id: "wqmmmd", "type": "regular polygon", "x": 620, "y": 380, "radius": 80, "number_of_edges": 6, "color": "gold", isEdgeDash: true, "rotation": 0, "info": "text", label_info: label4 }
        const new_poly = new Graph.RegularPolygon(poly_data)
        graph.addNode(new_poly);
        new_poly.draw_canvas(ctx)
        graph_figures.push(new_poly)


        const label5: Graph.ILabel = { text: "Sky is blue", color: 'black', font: "bold italic 14px Arial", padding: 10 }
        const cloud_descrip: Graph.ICustomDescription = {
            "typeName": "cloud", "points": [{ "x": 170, "y": 80 }, { "x": 230, "y": 150 }, { "x": 340, "y": 150 },
            { "x": 390, "y": 100 }, { "x": 340, "y": 50 }, { "x": 250, "y": 50 }, { "x": 170, "y": 80 }], "curve": [{ "isCurved": true, "cp1x": 130, "cp1y": 100, "cp2x": 130, "cp2y": 150 }, { "isCurved": true, "cp1x": 250, "cp1y": 180, "cp2x": 320, "cp2y": 180 },
            { "isCurved": true, "cp1x": 420, "cp1y": 150, "cp2x": 420, "cp2y": 120 }, { "isCurved": true, "cp1x": 430, "cp1y": 40, "cp2x": 310, "cp2y": 10 }, { "isCurved": true, "cp1x": 320, "cp1y": 5, "cp2x": 250, "cp2y": 20 }, { "isCurved": true, "cp1x": 200, "cp1y": 5, "cp2x": 150, "cp2y": 20 }]
        }
        const cloud_data: Graph.ICustomShape = { id: "cloud1111", "type": "cloud", "x_center": -75, "y_center": 365, "color": "white", isEdgeDash: true, label_info: label5 }
        const new_cloud = new Graph.CustomShape(cloud_data, cloud_descrip)
        graph.addNode(new_cloud);
        new_cloud.draw_canvas(ctx);
        graph_figures.push(new_cloud)


        const label_new: Graph.ILabel = { text: "Nod", color: 'gold', font: "14px Arial", padding: 10 }
        const ellipse: Graph.IEllipse = { id: "eu3hei3nnjd", type: "ellipse", x: 400, y: 200, radius_x: 30, radius_y: 60, rotation: 30, color: "blue", isEdgeDash: true, label_info: label_new }
        const new_ellipse = new Graph.Ellipse(ellipse);
        graph.addNode(new_ellipse);
        new_ellipse.draw_canvas(ctx)
        graph_figures.push(new_ellipse)

        //const ellipse2: Graph.IEllipse = { id: "eu3hei3nnjd", type: "ellipse", x: 400, y: 200, radius_x: 30, radius_y: 60, rotation: 0 }
        //const new_ellipse2 = new Graph.Ellipse(ellipse2);
        //new_ellipse2.draw_canvas(ctx)
        //graph_figures.push(new_ellipse2)

        const label_new2: Graph.ILabel = { text: "fix", font: "14px Arial", padding: 10 }
        const rhomb: Graph.IRhombus = { id: "meow1", type: "rhomb", x: 550, y: 200, width: 100, height: 60, rotation: 0, color: "green", isEdgeDash: true, label_info: label_new2 }
        const new_rhomb = new Graph.Rhombus(rhomb);
        graph.addNode(new_rhomb);
        new_rhomb.draw_canvas(ctx)
        graph_figures.push(new_rhomb)

        const rect_data2: Graph.IRectangle = { id: "opal", type: "rectangle", x: 150, y: 100, width: 100, color: "red", height: 60, isEdgeDash: true }
        const new_rect = new Graph.Rectangle(rect_data2);
        graph.addNode(new_rect);
        new_rect.draw_canvas(ctx)
        graph_figures.push(new_rect)

        const label_new_cloud: Graph.ILabel = { text: "404", font: "14px Arial", padding: 10 }
        const base_cloud_data: Graph.ICloud = { id: "kkkkk", type: "cloud", x_C: 430, y_C: 440, color: "#00bbe6", width: 100, height: 60, isEdgeDash: true, rotation: 0, label_info: label_new_cloud }
        const new_base_cloud = new Graph.Cloud(base_cloud_data);
        graph.addNode(new_base_cloud);
        new_base_cloud.draw_canvas(ctx)
        graph_figures.push(new_base_cloud)

        const label_new_star: Graph.ILabel = { text: "new", font: "14px Arial", padding: 10 }
        const base_star_data: Graph.IStar = { id: "loook", type: "star", x_C: 500, y_C: 550, color: "", rad: 30, amount_points: 7, m: 2, isEdgeDash: true, rotation: 0, label_info: label_new_star }
        const new_star_cloud = new Graph.Star(base_star_data);
        graph.addNode(new_star_cloud);
        new_star_cloud.draw_canvas(ctx)
        graph_figures.push(new_star_cloud)
    }

    const base_test = false;

    if (base_test) {
        //const line: Graph.ILine = { id: "efefef", type: "line", startX: 250, startY: 350, endX: 650, endY: 350, color: "purple", points: [{ x: 300, y: 300 }, { x: 350, y: 350 }, { x: 300, y: 400 }] }
        //const new_line = new Graph.Line(line, "none", "triangle")
        //new_line.draw_canvas(ctx);

        //const label0: Graph.ILabel = { text: "Nod", color: 'gold', font: "14px Arial", padding: 10 }
        const circle: Graph.ICircle = { id: "sddddd", type: "circle", x: 350, y: 350, radius: 30, color: "blue", isEdgeDash: false }
        const new_circle = new Graph.Circle(circle);
        new_circle.draw_canvas(ctx)
        graph.addNode(new_circle);
        graph_figures.push(new_circle)


        //const label: Graph.ILabel = { text: "Broken", color: 'black', font: "14px Arial", padding: 10, alignment: 'left' }
        const line: Graph.ILine = { id: "efefef", type: "line", startX: 250, startY: 190, endX: 330, endY: 240, color: "purple", isEdgeDash: false /*label_info: label,*/ /*points: [{ x: 600, y: 80 }, { x: 600, y: 70 }]*/ }
        const new_line = new Graph.Line(line, "stick", "triangle", new_circle)
        new_line.draw_canvas(ctx);
        graph.addEdge(new_line);
        graph_figures.push(new_line);

        //const line2: Graph.ILine = { id: "fofofo", type: "line", startX: 350, startY: 100, endX: 450, endY: 100, color: "", isEdgeDash: false }
        //const new_line2 = new Graph.Line(line2, "crow", "line")
        //new_line2.draw_canvas(ctx);
        //graph.addEdge(new_line2);
        //graph_figures.push(new_line2);

        //const label2: Graph.ILabel = { text: "Red", color: 'red', font: "18px Arial", padding: 10 }
        //const rectangle_data: Graph.IRectangle = { id: "reeeedddd", type: "rectangle", x: 550, y: 200, width: 100, height: 60, color: "green", label_info: label2 }
        //const new_rect = new Graph.Rectangle(rectangle_data);
        //new_rect.draw_canvas(ctx)

        //const label3: Graph.ILabel = { text: "Triangle", color: 'white', font: "Bold 18px Arial", padding: 10 }
        const triangle_data: Graph.ITriangle = { id: "dsdvcdsvs", type: "triangle", x_1: 210, y_1: 210, x_2: 310, y_2: 300, x_3: 110, y_3: 350, isEdgeDash: false, color: "#621a6e" }
        const new_tri = new Graph.Triangle(triangle_data);
        graph.addNode(new_tri);
        new_tri.draw_canvas(ctx)
        graph_figures.push(new_tri)


        //const label4: Graph.ILabel = { text: "Coding...", color: 'black', font: "14px Arial", padding: 10 }
        const poly_data: Graph.IRegularPolygon = { id: "wqmmmd", "type": "regular polygon", "x": 560, "y": 350, "radius": 80, "number_of_edges": 6, "color": "gold", isEdgeDash: false, "rotation": 0, "info": "text" }
        const new_poly = new Graph.RegularPolygon(poly_data)
        graph.addNode(new_poly);
        new_poly.draw_canvas(ctx)
        graph_figures.push(new_poly)


        //const label5: Graph.ILabel = { text: "Sky is blue", color: 'black', font: "bold italic 14px Arial", padding: 10 }
        const cloud_descrip: Graph.ICustomDescription = {
            "typeName": "cloud", "points": [{ "x": 170, "y": 80 }, { "x": 230, "y": 150 }, { "x": 340, "y": 150 },
            { "x": 390, "y": 100 }, { "x": 340, "y": 50 }, { "x": 250, "y": 50 }, { "x": 170, "y": 80 }], "curve": [{ "isCurved": true, "cp1x": 130, "cp1y": 100, "cp2x": 130, "cp2y": 150 }, { "isCurved": true, "cp1x": 250, "cp1y": 180, "cp2x": 320, "cp2y": 180 },
            { "isCurved": true, "cp1x": 420, "cp1y": 150, "cp2x": 420, "cp2y": 120 }, { "isCurved": true, "cp1x": 430, "cp1y": 40, "cp2x": 310, "cp2y": 10 }, { "isCurved": true, "cp1x": 320, "cp1y": 5, "cp2x": 250, "cp2y": 20 }, { "isCurved": true, "cp1x": 200, "cp1y": 5, "cp2x": 150, "cp2y": 20 }]
        }
        const cloud_data: Graph.ICustomShape = { id: "cloud1111", "type": "cloud", "x_center": -75, "y_center": 465, "color": "white", isEdgeDash: false }
        const new_cloud = new Graph.CustomShape(cloud_data, cloud_descrip)
        graph.addNode(new_cloud);
        new_cloud.draw_canvas(ctx);
        graph_figures.push(new_cloud)


        //const label_new: Graph.ILabel = { text: "Nod", color: 'gold', font: "14px Arial", padding: 10 }
        const ellipse: Graph.IEllipse = { id: "eu3hei3nnjd", type: "ellipse", x: 400, y: 200, radius_x: 30, radius_y: 60, rotation: 30, color: "blue", isEdgeDash: false }
        const new_ellipse = new Graph.Ellipse(ellipse);
        graph.addNode(new_ellipse);
        new_ellipse.draw_canvas(ctx)
        graph_figures.push(new_ellipse)

        //const ellipse2: Graph.IEllipse = { id: "eu3hei3nnjd", type: "ellipse", x: 400, y: 200, radius_x: 30, radius_y: 60, rotation: 0 }
        //const new_ellipse2 = new Graph.Ellipse(ellipse2);
        //new_ellipse2.draw_canvas(ctx)
        //graph_figures.push(new_ellipse2)

        //const label_new2: Graph.ILabel = { text: "fix", font: "14px Arial", padding: 10 }
        const rhomb: Graph.IRhombus = { id: "meow1", type: "rhomb", x: 550, y: 200, width: 100, height: 60, rotation: 0, color: "green", isEdgeDash: false }
        const new_rhomb = new Graph.Rhombus(rhomb);
        graph.addNode(new_rhomb);
        new_rhomb.draw_canvas(ctx)
        graph_figures.push(new_rhomb)

        const rect_data2: Graph.IRectangle = { id: "opal", type: "rectangle", x: 150, y: 100, width: 100, color: "red", height: 60, isEdgeDash: false }
        const new_rect = new Graph.Rectangle(rect_data2);
        graph.addNode(new_rect);
        new_rect.draw_canvas(ctx)
        graph_figures.push(new_rect)

        //const label_new_star: Graph.ILabel = { text: "new", font: "14px Arial", padding: 10 }
        const base_star_data: Graph.IStar = { id: "loook", type: "star", x_C: 500, y_C: 550, color: "#C8A2C8", rad: 30, amount_points: 7, m: 2, isEdgeDash: false, rotation: 0 }
        const new_star_cloud = new Graph.Star(base_star_data);
        graph.addNode(new_star_cloud);
        new_star_cloud.draw_canvas(ctx)
        graph_figures.push(new_star_cloud)

        const base_star_data2: Graph.IStar = { id: "another star", type: "star", x_C: 700, y_C: 550, color: "", rad: 30, amount_points: 5, m: 2.5, isEdgeDash: false, rotation: 0 }
        const new_star_cloud2 = new Graph.Star(base_star_data2);
        graph.addNode(new_star_cloud2);
        new_star_cloud.draw_canvas(ctx)
        graph_figures.push(new_star_cloud2)
    }

    const check_test_image = false;

    if (check_test_image) {


        const circle_image: Graph.ICircle = { id: "cat in a cup", type: "circle", x: 370, y: 420, radius: 60, isEdgeDash: true, image_src: img.imageBase64_small }
        const new_circle = new Graph.Circle(circle_image);
        //new_circle.draw_canvas(ctx)
        graph.addNode(new_circle);
        graph_figures.push(new_circle)

        const label3: Graph.ILabel = { text: "\u041F\u0440\u0438\u0432\u0435\u0442", color: 'white', font: "Bold 18px Arial", padding: 10 }
        const triangle_data: Graph.ITriangle = { id: "dsdvcdsvs", type: "triangle", x_1: 210, y_1: 210, x_2: 310, y_2: 300, x_3: 110, y_3: 350, isEdgeDash: true, color: "#621a6e", label_info: label3, image_src: img.imageBase64_small, image_rotation: 0, image_scale: 1 }
        const new_tri = new Graph.Triangle(triangle_data);
        //new_tri.draw_canvas(ctx)
        graph.addNode(new_tri);
        graph_figures.push(new_tri)

        const label4: Graph.ILabel = { text: "Coding...", color: 'black', font: "14px Arial", padding: 10 }
        const poly_data: Graph.IRegularPolygon = { id: "wqmmmd", "type": "regular polygon", "x": 620, "y": 380, "radius": 80, "number_of_edges": 6, "color": "gold", isEdgeDash: true, "rotation": 0, "info": "text", label_info: label4 }
        const new_poly = new Graph.RegularPolygon(poly_data)
        //new_poly.draw_canvas(ctx)
        graph_figures.push(new_poly)
        graph.addNode(new_poly);

        const circle_monkey: Graph.ICircle = { id: "monkey bibi", type: "circle", x: 150, y: 150, radius: 40, isEdgeDash: true, image_src: img.image_monkey }
        const new_circle2 = new Graph.Circle(circle_monkey);
        //new_circle.draw_canvas(ctx)
        graph.addNode(new_circle2);
        graph_figures.push(new_circle2)

        const circle_tiger: Graph.ICircle = { id: "tiger", type: "circle", x: 550, y: 200, radius: 50, isEdgeDash: false, image_src: img.imageBase64_tiger }
        const new_circle_tiger = new Graph.Circle(circle_tiger);
        //new_circle.draw_canvas(ctx)
        graph.addNode(new_circle_tiger);
        graph_figures.push(new_circle_tiger)

        const rect_data: Graph.IRectangle = { id: "mad_people", type: "rectangle", x: 720, y: 150, color: "teal", height: 120, width: 200, isEdgeDash: false, image_src: img.image_egor_ruslan }
        const new_rect = new Graph.Rectangle(rect_data);
        graph.addNode(new_rect);
        graph_figures.push(new_rect)

        const rect_cat_meme: Graph.IRectangle = { id: "okak", type: "rectangle", x: 50, y: 420, color: "black", height: 180, width: 180, isEdgeDash: false, image_src: img.image_okak }
        const node_rect_cat_meme = new Graph.Rectangle(rect_cat_meme);
        graph.addNode(node_rect_cat_meme);
        graph_figures.push(node_rect_cat_meme)

        const label_new: Graph.ILabel = { text: "окак", color: 'gold', font: "14px Arial", padding: 10 }
        const ellipse: Graph.IEllipse = { id: "eu3hei3nnjd", type: "ellipse", x: 400, y: 200, radius_x: 50, radius_y: 70, rotation: 30, color: "blue", isEdgeDash: true, label_info: label_new, image_src: img.image_okak }
        const new_ellipse = new Graph.Ellipse(ellipse);
        //new_ellipse.draw_canvas(ctx)
        graph.addNode(new_ellipse);
        graph_figures.push(new_ellipse)

        const rhomb: Graph.IRhombus = { id: "monkey", type: "rhomb", x: 830, y: 400, width: 120, height: 80, rotation: 30, color: "green", isEdgeDash: true, image_src: img.image_monkey }
        const new_rhomb = new Graph.Rhombus(rhomb);
        graph.addNode(new_rhomb);
        graph_figures.push(new_rhomb)
    }

    const check_rounded_line = false;

    if (check_rounded_line) {
        const line: Graph.ILine = {
            id: "round",
            type: "line",
            startX: 450,
            startY: 370,
            endX: 450,
            endY: 300,
            color: "purple",
            isEdgeDash: true,
            points: [{ x: 400, y: 350 }, { x: 400, y: 320 }],
            is_corners_rounded: true,
            max_radius_of_corners: 5,
            lineWidth: 4
        }
        const new_line = new Graph.Line(line)
        new_line.draw_canvas(ctx);
        graph.addEdge(new_line);
        graph_figures.push(new_line);

        const line2: Graph.ILine = {
            id: "round_second",
            type: "line",
            startX: 310,
            startY: 210,
            endX: 430,
            endY: 260,
            color: "green",
            isEdgeDash: false,
            points: [{ x: 330, y: 210 }, { x: 350, y: 270 }, { x: 390, y: 230 }],
            is_corners_rounded: true,
            rotation: 0,
            lineWidth: 3,
            max_radius_of_corners: 10
        }
        const new_line_round = new Graph.Line(line2)
        new_line_round.draw_canvas(ctx);
        graph.addEdge(new_line_round);
        graph_figures.push(new_line_round);
    }

    const test_metro1: boolean = false
    const test_metro2: boolean = false
    const test_skillTree: boolean = false

    if (test_metro1) {
        const circles: Graph.ICircle[] = [
            // Красная линия
            { id: "r1", type: "circle", x: 50, y: 100, radius: 8, color: "#e74c3c", isEdgeDash: false, label_info: { text: "Северный вокзал \n fhfhfhfhfhfh", color: 'black', font: "12px Arial", padding: 5 } },
            { id: "r2", type: "circle", x: 150, y: 100, radius: 12, color: "#e74c3c", isEdgeDash: false, label_info: { text: "Центральная", color: 'black', font: "12px Arial", padding: 5 } },
            { id: "r3", type: "circle", x: 250, y: 100, radius: 8, color: "#e74c3c", isEdgeDash: false, label_info: { text: "Южный парк", color: 'black', font: "12px Arial", padding: 5 } },

            // Синяя линия
            { id: "b1", type: "circle", x: 50, y: 200, radius: 8, color: "#3498db", isEdgeDash: false, label_info: { text: "Западный район", color: 'black', font: "12px Arial", padding: 5 } },
            { id: "b2", type: "circle", x: 150, y: 200, radius: 12, color: "#3498db", isEdgeDash: false, label_info: { text: "Университет", color: 'black', font: "12px Arial", padding: 5 } },

            // Зеленая линия
            { id: "g1", type: "circle", x: 150, y: 50, radius: 8, color: "#2ecc71", isEdgeDash: false, label_info: { text: "Ботанический сад", color: 'black', font: "12px Arial", padding: 5 } },
            { id: "g2", type: "circle", x: 150, y: 150, radius: 8, color: "#2ecc71", isEdgeDash: false, label_info: { text: "Стадион", color: 'black', font: "12px Arial", padding: 5 } },
        ]

        const lines: Graph.ILine[] = [
            // Красная линия (горизонтальная)
            { id: "red1", type: "line", startX: 50, startY: 100, endX: 150, endY: 100, color: "#e74c3c", isEdgeDash: false },
            { id: "red2", type: "line", startX: 150, startY: 100, endX: 250, endY: 100, color: "#e74c3c", isEdgeDash: false },

            // Синяя линия (горизонтальная)
            { id: "blue1", type: "line", startX: 50, startY: 200, endX: 150, endY: 200, color: "#3498db", isEdgeDash: false },

            // Зеленая линия (вертикальная)
            { id: "green1", type: "line", startX: 150, startY: 50, endX: 150, endY: 100, color: "#2ecc71", isEdgeDash: false },
            { id: "green2", type: "line", startX: 150, startY: 100, endX: 150, endY: 150, color: "#2ecc71", isEdgeDash: false },
            { id: "green3", type: "line", startX: 150, startY: 150, endX: 150, endY: 200, color: "#2ecc71", isEdgeDash: false },

            // Переходы (пунктирные линии)
            {
                id: "trans1", type: "line", startX: 150, startY: 100, endX: 150, endY: 200, color: "#7f8c8d", isEdgeDash: true, points: [
                    { x: 160, y: 120 },
                    { x: 160, y: 180 }
                ]
            },
        ]


        lines.forEach(line => {
            const new_line = new Graph.Line(line)
            new_line.draw_canvas(ctx);
            graph.addEdge(new_line);
            graph_figures.push(new_line);
        })

        circles.forEach(circle => {
            const new_circle = new Graph.Circle(circle)
            new_circle.draw_canvas(ctx);
            graph.addNode(new_circle);
            graph_figures.push(new_circle);
        })
    }

    if (test_metro2) {
        const circles2: Graph.ICircle[] = [
            // Красная линия (5 станций)
            { id: "r1", type: "circle", x: 100, y: 250, radius: 8, color: "#e74c3c", label_info: { text: "Западный вокзал", color: '#2c3e50', font: "12px Arial", padding: 10, position: "above" } },
            { id: "r2", type: "circle", x: 250, y: 300, radius: 12, color: "#e74c3c", label_info: { text: "Центральный узел", color: '#2c3e50', font: "bold 12px Arial", padding: 10, position: "above" } },
            { id: "r3", type: "circle", x: 400, y: 250, radius: 8, color: "#e74c3c", label_info: { text: "Парк Победы", color: '#2c3e50', font: "12px Arial", padding: 10, position: "above" } },
            { id: "r4", type: "circle", x: 550, y: 300, radius: 8, color: "#e74c3c", label_info: { text: "Восточный терминал", color: '#2c3e50', font: "12px Arial", padding: 10, position: "under" } },
            { id: "r5", type: "circle", x: 700, y: 250, radius: 8, color: "#e74c3c", label_info: { text: "Аэропорт", color: '#2c3e50', font: "12px Arial", padding: 10, position: "above" } },

            // Синяя линия (4 станции)
            { id: "b1", type: "circle", x: 100, y: 450, radius: 8, color: "#3498db", label_info: { text: "Южный порт", color: '#2c3e50', font: "12px Arial", padding: 10, position: "under" } },
            { id: "b2", type: "circle", x: 300, y: 400, radius: 12, color: "#3498db", label_info: { text: "Деловой квартал", color: '#2c3e50', font: "bold 12px Arial", padding: 10, position: "under" } },
            { id: "b3", type: "circle", x: 500, y: 450, radius: 8, color: "#3498db", label_info: { text: "Промышленная зона", color: '#2c3e50', font: "12px Arial", padding: 10, position: "under" } },
            { id: "b4", type: "circle", x: 700, y: 400, radius: 8, color: "#3498db", label_info: { text: "Технологический парк", color: '#2c3e50', font: "12px Arial", padding: 10, position: "under" } },

            // Зеленая линия (5 станций)
            { id: "g1", type: "circle", x: 150, y: 600, radius: 8, color: "#2ecc71", label_info: { text: "Ботанический сад", color: '#2c3e50', font: "12px Arial", padding: 10, position: "under" } },
            { id: "g2", type: "circle", x: 300, y: 550, radius: 12, color: "#2ecc71", label_info: { text: "Спортивный комплекс", color: '#2c3e50', font: "bold 12px Arial", padding: 10, position: "above" } },
            { id: "g3", type: "circle", x: 450, y: 600, radius: 8, color: "#2ecc71", label_info: { text: "Университетский городок", color: '#2c3e50', font: "12px Arial", padding: 10, position: "under" } },
            { id: "g4", type: "circle", x: 600, y: 550, radius: 8, color: "#2ecc71", label_info: { text: "Медицинский центр", color: '#2c3e50', font: "12px Arial", padding: 10, position: "under" } },
            { id: "g5", type: "circle", x: 750, y: 600, radius: 8, color: "#2ecc71", label_info: { text: "Научный институт", color: '#2c3e50', font: "12px Arial", padding: 10, position: "under" } },

            // Фиолетовая линия (4 станции)
            { id: "p1", type: "circle", x: 200, y: 350, radius: 8, color: "#9b59b6", label_info: { text: "Исторический музей", color: '#2c3e50', font: "12px Arial", padding: 10, position: "under" } },
            { id: "p2", type: "circle", x: 400, y: 350, radius: 12, color: "#9b59b6", label_info: { text: "Культурный центр", color: '#2c3e50', font: "bold 12px Arial", padding: 10, position: "under" } },
            { id: "p3", type: "circle", x: 600, y: 350, radius: 8, color: "#9b59b6", label_info: { text: "Филармония", color: '#2c3e50', font: "12px Arial", padding: 10, position: "under" } },
            { id: "p4", type: "circle", x: 750, y: 380, radius: 8, color: "#9b59b6", label_info: { text: "Арт-квартал", color: '#2c3e50', font: "12px Arial", padding: 10, position: "above" } },

            // Оранжевая линия (3 станции)
            { id: "o1", type: "circle", x: 100, y: 500, radius: 8, color: "#e67e22", label_info: { text: "Торговый район", color: '#2c3e50', font: "12px Arial", padding: 10, position: "under" } },
            { id: "o2", type: "circle", x: 350, y: 480, radius: 12, color: "#e67e22", label_info: { text: "Центральный рынок", color: '#2c3e50', font: "bold 12px Arial", padding: 10, position: "under" } },
            { id: "o3", type: "circle", x: 600, y: 500, radius: 8, color: "#e67e22", label_info: { text: "Выставочный центр", color: '#2c3e50', font: "12px Arial", padding: 10, position: "above" } },

            // Коричневая линия (3 станции)
            { id: "br1", type: "circle", x: 250, y: 450, radius: 8, color: "#795548", label_info: { text: "Парк культуры", color: '#2c3e50', font: "12px Arial", padding: 10, position: "under" } },
            { id: "br2", type: "circle", x: 500, y: 500, radius: 8, color: "#795548", label_info: { text: "Речная набережная", color: '#2c3e50', font: "12px Arial", padding: 10, position: "under" } },
            { id: "br3", type: "circle", x: 700, y: 550, radius: 8, color: "#795548", label_info: { text: "Лесной массив", color: '#2c3e50', font: "12px Arial", padding: 10, position: "above" } }
        ]

        const lines2: Graph.ILine[] = [
            // Красная линия (с изгибами)
            {
                id: "red_line",
                type: "line",
                startX: 100,
                startY: 250,
                endX: 700,
                endY: 250,
                color: "#e74c3c",
                points: [
                    { x: 150, y: 280 },
                    { x: 250, y: 300 },
                    { x: 400, y: 250 },
                    { x: 550, y: 300 },
                    { x: 650, y: 280 }
                ],
                is_corners_rounded: true,
                max_radius_of_corners: 25,
                lineWidth: 2
            },

            // Синяя линия (диагональная)
            {
                id: "blue_line",
                type: "line",
                startX: 100,
                startY: 450,
                endX: 700,
                endY: 400,
                color: "#3498db",
                points: [
                    { x: 200, y: 430 },
                    { x: 300, y: 400 },
                    { x: 500, y: 450 },
                    { x: 600, y: 420 }
                ],
                is_corners_rounded: true,
                max_radius_of_corners: 30,
                lineWidth: 2
            },

            // Зеленая линия (зигзагообразная)
            {
                id: "green_line",
                type: "line",
                startX: 150,
                startY: 600,
                endX: 750,
                endY: 600,
                color: "#2ecc71",
                points: [
                    { x: 250, y: 580 },
                    { x: 300, y: 550 },
                    { x: 450, y: 600 },
                    { x: 600, y: 550 },
                    { x: 700, y: 570 }
                ],
                is_corners_rounded: true,
                max_radius_of_corners: 28,
                lineWidth: 2
            },

            // Фиолетовая линия (полукруг)
            {
                id: "purple_line",
                type: "line",
                startX: 200,
                startY: 350,
                endX: 750,
                endY: 380,
                color: "#9b59b6",
                points: [
                    { x: 300, y: 330 },
                    { x: 400, y: 350 },
                    { x: 500, y: 340 },
                    { x: 600, y: 350 },
                    { x: 700, y: 360 }
                ],
                is_corners_rounded: true,
                max_radius_of_corners: 20,
                lineWidth: 2
            },

            // Оранжевая линия (прямая с изгибом)
            {
                id: "orange_line",
                type: "line",
                startX: 100,
                startY: 500,
                endX: 600,
                endY: 500,
                color: "#e67e22",
                points: [
                    { x: 200, y: 490 },
                    { x: 350, y: 480 },
                    { x: 500, y: 510 }
                ],
                is_corners_rounded: true,
                max_radius_of_corners: 22,
                lineWidth: 2
            },

            // Коричневая линия (короткая ветка)
            {
                id: "brown_line",
                type: "line",
                startX: 250,
                startY: 450,
                endX: 700,
                endY: 550,
                color: "#795548",
                points: [
                    { x: 350, y: 480 },
                    { x: 500, y: 500 },
                    { x: 600, y: 530 }
                ],
                is_corners_rounded: true,
                max_radius_of_corners: 18,
                lineWidth: 2
            },

            // Переходы (пунктирные линии)
            {
                id: "transfer1",
                type: "line",
                startX: 250,
                startY: 300,
                endX: 300,
                endY: 400,
                color: "#7f8c8d",
                isEdgeDash: true,
                is_corners_rounded: true,
                max_radius_of_corners: 15,
                lineWidth: 2
            },
            {
                id: "transfer2",
                type: "line",
                startX: 300,
                startY: 550,
                endX: 300,
                endY: 400,
                color: "#7f8c8d",
                isEdgeDash: true,
                points: [
                    { x: 320, y: 450 }
                ],
                is_corners_rounded: true,
                max_radius_of_corners: 12,
                lineWidth: 2
            },
            {
                id: "transfer3",
                type: "line",
                startX: 400,
                startY: 250,
                endX: 400,
                endY: 350,
                color: "#7f8c8d",
                isEdgeDash: true,
                is_corners_rounded: true,
                max_radius_of_corners: 10,
                lineWidth: 2
            },
            {
                id: "transfer4",
                type: "line",
                startX: 500,
                startY: 450,
                endX: 450,
                endY: 600,
                color: "#7f8c8d",
                isEdgeDash: true,
                points: [
                    { x: 470, y: 520 }
                ],
                is_corners_rounded: true,
                max_radius_of_corners: 15,
                lineWidth: 2
            },
            {
                id: "transfer5",
                type: "line",
                startX: 700,
                startY: 400,
                endX: 700,
                endY: 250,
                color: "#7f8c8d",
                isEdgeDash: true,
                points: [
                    { x: 680, y: 320 }
                ],
                is_corners_rounded: true,
                max_radius_of_corners: 20,
                lineWidth: 2
            }
        ]

        lines2.forEach(line => {
            const new_line = new Graph.Line(line)
            new_line.draw_canvas(ctx);
            graph.addEdge(new_line);
            graph_figures.push(new_line);
        })

        circles2.forEach(circle => {
            const new_circle = new Graph.Circle(circle)
            new_circle.draw_canvas(ctx);
            graph.addNode(new_circle);
            graph_figures.push(new_circle);
        })
    }

    if (test_skillTree) {
        const poeTreeNodes: Graph.ICircle[] = [
            // Центральные ноды (стартовые и ключевые)
            { id: "start", type: "circle", x: 400, y: 300, radius: 18, color: "#F5D76E", label_info: { text: "Сердце Древа", color: '#2c3e50', font: "bold 16px Arial", padding: 15, position: "above" } },
            { id: "keystone1", type: "circle", x: 150, y: 100, radius: 22, color: "#C0392B", label_info: { text: "Берсерк", color: 'blue', font: "bold 14px Arial", padding: 20, position: "left" } },
            { id: "keystone2", type: "circle", x: 650, y: 100, radius: 22, color: "#27AE60", label_info: { text: "Призрачный Беглец", color: 'blue', font: "bold 14px Arial", padding: 20, position: "right" } },
            { id: "keystone3", type: "circle", x: 400, y: 550, radius: 22, color: "#2980B9", label_info: { text: "Архимаг", color: 'blue', font: "bold 14px Arial", padding: 20, position: "under" } },

            // Верхняя ветка (красная - сила)
            { id: "str1", type: "circle", x: 250, y: 150, radius: 10, color: "#E74C3C" },
            { id: "str2", type: "circle", x: 200, y: 200, radius: 12, color: "#E74C3C" },
            { id: "str3", type: "circle", x: 150, y: 250, radius: 14, color: "#C0392B" },
            { id: "str4", type: "circle", x: 100, y: 180, radius: 10, color: "#E74C3C" },
            { id: "str5", type: "circle", x: 180, y: 120, radius: 8, color: "#E74C3C" },

            // Правая ветка (зеленая - ловкость)
            { id: "dex1", type: "circle", x: 500, y: 150, radius: 10, color: "#2ECC71" },
            { id: "dex2", type: "circle", x: 550, y: 200, radius: 12, color: "#2ECC71" },
            { id: "dex3", type: "circle", x: 600, y: 250, radius: 14, color: "#27AE60" },
            { id: "dex4", type: "circle", x: 650, y: 180, radius: 10, color: "#2ECC71" },
            { id: "dex5", type: "circle", x: 580, y: 120, radius: 8, color: "#2ECC71" },

            // Нижняя ветка (синяя - интеллект)
            { id: "int1", type: "circle", x: 350, y: 400, radius: 10, color: "#3498DB" },
            { id: "int2", type: "circle", x: 400, y: 450, radius: 12, color: "#3498DB" },
            { id: "int3", type: "circle", x: 450, y: 500, radius: 14, color: "#2980B9" },
            { id: "int4", type: "circle", x: 300, y: 480, radius: 10, color: "#3498DB" },
            { id: "int5", type: "circle", x: 500, y: 480, radius: 10, color: "#3498DB" },

            // Гибридные зоны (левая нижняя - сила/ловкость)
            { id: "hybrid1", type: "circle", x: 250, y: 350, radius: 12, color: "#E67E22" },
            { id: "hybrid2", type: "circle", x: 200, y: 400, radius: 14, color: "#D35400" },
            { id: "hybrid3", type: "circle", x: 150, y: 450, radius: 10, color: "#E67E22" },
            { id: "hybrid4", type: "circle", x: 280, y: 420, radius: 8, color: "#E67E22" },

            // Гибридные зоны (правая нижняя - ловкость/интеллект)
            { id: "hybrid5", type: "circle", x: 550, y: 350, radius: 12, color: "#1ABC9C" },
            { id: "hybrid6", type: "circle", x: 600, y: 400, radius: 14, color: "#16A085" },
            { id: "hybrid7", type: "circle", x: 650, y: 450, radius: 10, color: "#1ABC9C" },
            { id: "hybrid8", type: "circle", x: 520, y: 420, radius: 8, color: "#1ABC9C" },

            // Центральные кластеры
            { id: "cluster1", type: "circle", x: 300, y: 250, radius: 16, color: "#9B59B6", label_info: { text: "Рунный Узел", color: 'blue', font: "bold 12px Arial", padding: 15, position: "left" } },
            { id: "cluster2", type: "circle", x: 500, y: 250, radius: 16, color: "#F1C40F", label_info: { text: "Солнечный Круг", color: '#2c3e50', font: "bold 12px Arial", padding: 15, position: "right" } },
            { id: "cluster3", type: "circle", x: 400, y: 200, radius: 14, color: "#34495E" },
            { id: "cluster4", type: "circle", x: 350, y: 150, radius: 10, color: "#95A5A6" },
            { id: "cluster5", type: "circle", x: 450, y: 150, radius: 10, color: "#95A5A6" },

            // Специальные ноды
            { id: "unique1", type: "circle", x: 100, y: 350, radius: 20, color: "#8E44AD", label_info: { text: "Длань Хаоса", color: 'blue', font: "bold 14px Arial", padding: 18, position: "left" } },
            { id: "unique2", type: "circle", x: 700, y: 350, radius: 20, color: "#D35400", label_info: { text: "Огненная Крепость", color: 'blue', font: "bold 14px Arial", padding: 18, position: "right" } },
            { id: "unique3", type: "circle", x: 400, y: 100, radius: 24, color: "#C0392B", label_info: { text: "Небесный Трон", color: 'blue', font: "bold 16px Arial", padding: 22, position: "above" } },
            { id: "jewel1", type: "circle", x: 250, y: 280, radius: 8, color: "#F1C40F" }, //, shape: "diamond"
            { id: "jewel2", type: "circle", x: 550, y: 280, radius: 8, color: "#F1C40F" }, //, shape: "diamond"
            { id: "jewel3", type: "circle", x: 400, y: 380, radius: 8, color: "#F1C40F" } //, shape: "diamond"
        ];

        const poeTreeConnections: Graph.ILine[] = [
            // Основные ветви от центра
            {
                id: "main_str", type: "line", startX: 400, startY: 318, endX: 250, endY: 150, color: "#E74C3C", lineWidth: 5,
                points: [{ x: 350, y: 200 }]
            },
            {
                id: "main_dex", type: "line", startX: 400, startY: 318, endX: 550, endY: 150, color: "#2ECC71", lineWidth: 5,
                points: [{ x: 450, y: 200 }]
            },
            {
                id: "main_int", type: "line", startX: 400, startY: 318, endX: 400, endY: 450, color: "#3498DB", lineWidth: 5,
                points: [{ x: 380, y: 380 }]
            },

            // Верхняя красная ветка с изгибами
            {
                id: "str_line1", type: "line", startX: 250, startY: 150, endX: 200, endY: 200, color: "#E74C3C", lineWidth: 4,
                points: [{ x: 220, y: 170 }]
            },
            { id: "str_line2", type: "line", startX: 200, startY: 200, endX: 150, endY: 250, color: "#C0392B", lineWidth: 4 },
            {
                id: "str_line3", type: "line", startX: 150, startY: 250, endX: 150, endY: 100, color: "#E74C3C", lineWidth: 4,
                points: [{ x: 130, y: 180 }]
            },
            { id: "str_line4", type: "line", startX: 250, startY: 150, endX: 180, endY: 120, color: "#E74C3C", lineWidth: 3 },
            {
                id: "str_line5", type: "line", startX: 180, startY: 120, endX: 100, endY: 180, color: "#E74C3C", lineWidth: 3,
                points: [{ x: 140, y: 100 }]
            },

            // Зеленая ветка с обходными путями
            { id: "dex_line1", type: "line", startX: 550, startY: 150, endX: 550, endY: 200, color: "#2ECC71", lineWidth: 4 },
            {
                id: "dex_line2", type: "line", startX: 550, startY: 200, endX: 600, endY: 250, color: "#27AE60", lineWidth: 4,
                points: [{ x: 580, y: 220 }]
            },
            {
                id: "dex_line3", type: "line", startX: 600, startY: 250, endX: 650, endY: 180, color: "#2ECC71", lineWidth: 4,
                points: [{ x: 630, y: 220 }]
            },
            { id: "dex_line4", type: "line", startX: 550, startY: 150, endX: 580, endY: 120, color: "#2ECC71", lineWidth: 3 },
            {
                id: "dex_line5", type: "line", startX: 580, startY: 120, endX: 650, endY: 180, color: "#2ECC71", lineWidth: 3,
                points: [{ x: 620, y: 140 }]
            },

            // Синяя ветка с S-образными изгибами
            {
                id: "int_line1", type: "line", startX: 400, startY: 450, endX: 350, endY: 400, color: "#3498DB", lineWidth: 4,
                points: [{ x: 380, y: 420 }]
            },
            {
                id: "int_line2", type: "line", startX: 400, startY: 450, endX: 450, endY: 500, color: "#2980B9", lineWidth: 4,
                points: [{ x: 420, y: 480 }]
            },
            {
                id: "int_line3", type: "line", startX: 450, startY: 500, endX: 400, endY: 550, color: "#3498DB", lineWidth: 5,
                points: [{ x: 430, y: 530 }]
            },
            {
                id: "int_line4", type: "line", startX: 350, startY: 400, endX: 300, endY: 480, color: "#3498DB", lineWidth: 3,
                points: [{ x: 320, y: 430 }, { x: 300, y: 450 }]
            },
            {
                id: "int_line5", type: "line", startX: 450, startY: 500, endX: 500, endY: 480, color: "#3498DB", lineWidth: 3,
                points: [{ x: 470, y: 490 }]
            },

            // Гибридные соединения с избеганием пересечений
            {
                id: "hyb_left1", type: "line", startX: 250, startY: 350, endX: 200, endY: 400, color: "#E67E22", lineWidth: 4,
                points: [{ x: 220, y: 370 }]
            },
            {
                id: "hyb_left2", type: "line", startX: 200, startY: 400, endX: 150, endY: 450, color: "#D35400", lineWidth: 4,
                points: [{ x: 170, y: 420 }]
            },
            {
                id: "hyb_left3", type: "line", startX: 250, startY: 350, endX: 280, endY: 420, color: "#E67E22", lineWidth: 3,
                points: [{ x: 260, y: 380 }]
            },
            {
                id: "hyb_left4", type: "line", startX: 150, startY: 250, endX: 100, endY: 350, color: "#8E44AD", lineWidth: 5,
                points: [{ x: 120, y: 280 }, { x: 100, y: 300 }]
            },

            {
                id: "hyb_right1", type: "line", startX: 550, startY: 350, endX: 600, endY: 400, color: "#1ABC9C", lineWidth: 4,
                points: [{ x: 570, y: 370 }]
            },
            {
                id: "hyb_right2", type: "line", startX: 600, startY: 400, endX: 650, endY: 450, color: "#16A085", lineWidth: 4,
                points: [{ x: 630, y: 420 }]
            },
            {
                id: "hyb_right3", type: "line", startX: 550, startY: 350, endX: 520, endY: 420, color: "#1ABC9C", lineWidth: 3,
                points: [{ x: 540, y: 380 }]
            },
            {
                id: "hyb_right4", type: "line", startX: 600, startY: 250, endX: 700, endY: 350, color: "#D35400", lineWidth: 5,
                points: [{ x: 650, y: 280 }, { x: 700, y: 300 }]
            },

            // Кластерные соединения с закруглениями
            {
                id: "cluster_conn1", type: "line", startX: 400, startY: 318, endX: 300, endY: 250, color: "#9B59B6", lineWidth: 4,
                is_corners_rounded: true, max_radius_of_corners: 30, points: [{ x: 350, y: 280 }]
            },
            {
                id: "cluster_conn2", type: "line", startX: 400, startY: 318, endX: 500, endY: 250, color: "#F1C40F", lineWidth: 4,
                is_corners_rounded: true, max_radius_of_corners: 30, points: [{ x: 450, y: 280 }]
            },
            {
                id: "cluster_conn3", type: "line", startX: 300, startY: 250, endX: 350, endY: 150, color: "#95A5A6", lineWidth: 3,
                points: [{ x: 320, y: 200 }]
            },
            {
                id: "cluster_conn4", type: "line", startX: 500, startY: 250, endX: 450, endY: 150, color: "#95A5A6", lineWidth: 3,
                points: [{ x: 480, y: 200 }]
            },
            {
                id: "cluster_conn5", type: "line", startX: 400, startY: 200, endX: 400, endY: 150, color: "#34495E", lineWidth: 3,
                points: [{ x: 390, y: 180 }]
            },

            // Вертикальные соединения с обходом
            {
                id: "vert1", type: "line", startX: 400, startY: 200, endX: 400, endY: 100, color: "#C0392B", lineWidth: 6,
                is_corners_rounded: true, max_radius_of_corners: 40, points: [{ x: 380, y: 150 }]
            },
            {
                id: "vert2", type: "line", startX: 300, startY: 150, endX: 400, endY: 100, color: "#95A5A6", lineWidth: 3,
                points: [{ x: 350, y: 120 }]
            },
            {
                id: "vert3", type: "line", startX: 450, startY: 150, endX: 400, endY: 100, color: "#95A5A6", lineWidth: 3,
                points: [{ x: 420, y: 120 }]
            },

            // Специальные соединения для уникальных нод
            {
                id: "unique_conn1", type: "line", startX: 150, startY: 450, endX: 100, endY: 350, color: "#8E44AD", lineWidth: 5,
                is_corners_rounded: true, max_radius_of_corners: 25, points: [{ x: 120, y: 400 }]
            },
            {
                id: "unique_conn2", type: "line", startX: 650, startY: 450, endX: 700, endY: 350, color: "#D35400", lineWidth: 5,
                is_corners_rounded: true, max_radius_of_corners: 25, points: [{ x: 680, y: 400 }]
            },

            // Соединения для алмазных нод (украшения)
            {
                id: "jewel_conn1", type: "line", startX: 300, startY: 250, endX: 250, endY: 280, color: "#F1C40F", lineWidth: 2,
                isEdgeDash: true
            },
            {
                id: "jewel_conn2", type: "line", startX: 500, startY: 250, endX: 550, endY: 280, color: "#F1C40F", lineWidth: 2,
                isEdgeDash: true
            },
            {
                id: "jewel_conn3", type: "line", startX: 400, startY: 450, endX: 400, endY: 380, color: "#F1C40F", lineWidth: 2,
                isEdgeDash: true
            },

            // Межкластерные соединения
            {
                id: "cross1", type: "line", startX: 300, startY: 250, endX: 250, endY: 350, color: "#9B59B6", lineWidth: 4,
                points: [{ x: 270, y: 280 }, { x: 250, y: 310 }]
            },
            {
                id: "cross2", type: "line", startX: 500, startY: 250, endX: 550, endY: 350, color: "#F1C40F", lineWidth: 4,
                points: [{ x: 520, y: 280 }, { x: 550, y: 310 }]
            },
            {
                id: "cross3", type: "line", startX: 250, startY: 350, endX: 400, endY: 450, color: "#E67E22", lineWidth: 4,
                points: [{ x: 300, y: 400 }]
            },
            {
                id: "cross4", type: "line", startX: 550, startY: 350, endX: 400, endY: 450, color: "#1ABC9C", lineWidth: 4,
                points: [{ x: 500, y: 400 }]
            }
        ];

        poeTreeConnections.forEach(line => {
            const new_line = new Graph.Line(line)
            new_line.draw_canvas(ctx);
            graph.addEdge(new_line);
            graph_figures.push(new_line);
        })

        poeTreeNodes.forEach(circle => {
            const new_circle = new Graph.Circle(circle)
            new_circle.draw_canvas(ctx);
            graph.addNode(new_circle);
            graph_figures.push(new_circle);
        })
    }


    const test_uml = false;

    if (test_uml) {
        const nodes: Graph.DataFigure[] = [
            // Класс Student
            {
                id: "classStudent",
                type: "rectangle",
                x: 50,
                y: 50,
                width: 150,
                height: 80,
                color: "#2c3e50", // Темно-синий
                label_info: { text: "Student", color: "#ecf0f1", font: "14px Arial", position: "center" }
            },
            // Класс Course
            {
                id: "classCourse",
                type: "rectangle",
                x: 250,
                y: 50,
                width: 150,
                height: 80,
                color: "#2c3e50",
                label_info: { text: "Course", color: "#ecf0f1", font: "14px Arial", position: "center" }
            },
            // Класс Instructor
            {
                id: "classInstructor",
                type: "rectangle",
                x: 450,
                y: 50,
                width: 150,
                height: 80,
                color: "#2c3e50",
                label_info: { text: "Instructor", color: "#ecf0f1", font: "14px Arial", position: "center" }
            },
            // Класс Department
            {
                id: "classDepartment",
                type: "rectangle",
                x: 650,
                y: 50,
                width: 150,
                height: 80,
                color: "#2c3e50",
                label_info: { text: "Department", color: "#ecf0f1", font: "14px Arial", position: "center" }
            },
            // Класс University
            {
                id: "classUniversity",
                type: "rectangle",
                x: 400,
                y: 200,
                width: 150,
                height: 80,
                color: "#2c3e50",
                label_info: { text: "University", color: "#ecf0f1", font: "14px Arial", position: "center" }
            },
            // Интерфейс Enrollable
            {
                id: "interfaceEnrollable",
                type: "rhombus",
                x: 400,
                y: 350,
                width: 100,
                height: 60,
                color: "#34495e", // Синий серый
                label_info: { text: "Enrollable", color: "#ecf0f1", font: "14px Arial", position: "center" }
            },
            // Интерфейс Assessable
            {
                id: "interfaceAssessable",
                type: "rhombus",
                x: 600,
                y: 350,
                width: 100,
                height: 60,
                color: "#34495e",
                label_info: { text: "Assessable", color: "#ecf0f1", font: "14px Arial", position: "center" }
            },
            // Класс Transcript
            {
                id: "classTranscript",
                type: "rectangle",
                x: 50,
                y: 200,
                width: 150,
                height: 80,
                color: "#2c3e50",
                label_info: { text: "Transcript", color: "#ecf0f1", font: "14px Arial", position: "center" }
            },
            // Класс Grade
            {
                id: "classGrade",
                type: "rectangle",
                x: 250,
                y: 200,
                width: 150,
                height: 80,
                color: "#2c3e50",
                label_info: { text: "Grade", color: "#ecf0f1", font: "14px Arial", position: "center" }
            },
            // Класс Schedule
            {
                id: "classSchedule",
                type: "rectangle",
                x: 450,
                y: 200,
                width: 150,
                height: 80,
                color: "#2c3e50",
                label_info: { text: "Schedule", color: "#ecf0f1", font: "14px Arial", position: "center" }
            },
            // Класс Assignment
            {
                id: "classAssignment",
                type: "rectangle",
                x: 650,
                y: 200,
                width: 150,
                height: 80,
                color: "#2c3e50",
                label_info: { text: "Assignment", color: "#ecf0f1", font: "14px Arial", position: "center" }
            },
            // Класс Exam
            {
                id: "classExam",
                type: "rectangle",
                x: 400,
                y: 450,
                width: 150,
                height: 80,
                color: "#2c3e50",
                label_info: { text: "Exam", color: "#ecf0f1", font: "14px Arial", position: "center" }
            },
            // Эллипс Enrollment
            {
                id: "ellipseEnrollment",
                type: "ellipse",
                x: 125,
                y: 350,
                width: 200,
                height: 100,
                color: "#95a5a6", // Светло-серый
                label_info: { text: "Enrollment", color: "#2c3e50", font: "14px Arial", position: "center" }
            },
            // Эллипс Evaluation
            {
                id: "ellipseEvaluation",
                type: "ellipse",
                x: 125,
                y: 500,
                width: 200,
                height: 100,
                color: "#95a5a6",
                label_info: { text: "Evaluation", color: "#2c3e50", font: "14px Arial", position: "center" }
            }
        ];

        // Связи
        const lines: Graph.ILine[] = [
            // Ассоциация между Student и Enrollment
            {
                id: "associationStudentEnrollment",
                type: "line",
                startX: 125,
                startY: 130,
                endX: 225,
                endY: 350,
                color: "#95a5a6", // Серый
                lineWidth: 2,
                points: [],
                is_corners_rounded: true,
                max_radius_of_corners: 5
            },
            // Ассоциация между Course и Enrollment
            {
                id: "associationCourseEnrollment",
                type: "line",
                startX: 325,
                startY: 130,
                endX: 225,
                endY: 350,
                color: "#95a5a6",
                lineWidth: 2,
                points: [],
                is_corners_rounded: true,
                max_radius_of_corners: 5
            },
            // Ассоциация между Instructor и Course
            {
                id: "associationInstructorCourse",
                type: "line",
                startX: 525,
                startY: 130,
                endX: 325,
                endY: 130,
                color: "#95a5a6",
                lineWidth: 2,
                points: [],
                is_corners_rounded: true,
                max_radius_of_corners: 5
            },
            // Ассоциация между Course и Department
            {
                id: "associationCourseDepartment",
                type: "line",
                startX: 325,
                startY: 130,
                endX: 725,
                endY: 130,
                color: "#95a5a6",
                lineWidth: 2,
                points: [],
                is_corners_rounded: true,
                max_radius_of_corners: 5
            },
            // Реализация интерфейса Enrollable классом Course
            {
                id: "implementationCourseEnrollable",
                type: "line",
                startX: 400,
                startY: 350,
                endX: 325,
                endY: 130,
                color: "#95a5a6",
                lineWidth: 2,
                points: [],
                is_corners_rounded: true,
                max_radius_of_corners: 5
            },
            // Ассоциация между Student и Transcript
            {
                id: "associationStudentTranscript",
                type: "line",
                startX: 125,
                startY: 130,
                endX: 125,
                endY: 280,
                color: "#95a5a6",
                lineWidth: 2,
                points: [],
                is_corners_rounded: true,
                max_radius_of_corners: 5
            },
            // Ассоциация между Transcript и Grade
            {
                id: "associationTranscriptGrade",
                type: "line",
                startX: 125,
                startY: 280,
                endX: 325,
                endY: 280,
                color: "#95a5a6",
                lineWidth: 2,
                points: [],
                is_corners_rounded: true,
                max_radius_of_corners: 5
            },
            // Ассоциация между Course и Schedule
            {
                id: "associationCourseSchedule",
                type: "line",
                startX: 325,
                startY: 130,
                endX: 525,
                endY: 280,
                color: "#95a5a6",
                lineWidth: 2,
                points: [],
                is_corners_rounded: true,
                max_radius_of_corners: 5
            },
            // Ассоциация между Course и Assignment
            {
                id: "associationCourseAssignment",
                type: "line",
                startX: 325,
                startY: 130,
                endX: 725,
                endY: 280,
                color: "#95a5a6",
                lineWidth: 2,
                points: [],
                is_corners_rounded: true,
                max_radius_of_corners: 5
            },
            // Ассоциация между Course и Exam
            {
                id: "associationCourseExam",
                type: "line",
                startX: 325,
                startY: 130,
                endX: 475,
                endY: 480,
                color: "#95a5a6",
                lineWidth: 2,
                points: [],
                is_corners_rounded: true,
                max_radius_of_corners: 5
            },
            // Ассоциация между Transcript и Evaluation
            {
                id: "associationTranscriptEvaluation",
                type: "line",
                startX: 125,
                startY: 280,
                endX: 225,
                endY: 500,
                color: "#95a5a6",
                lineWidth: 2,
                points: [],
                is_corners_rounded: true,
                max_radius_of_corners: 5
            },
            // Реализация интерфейса Assessable классом Exam
            {
                id: "implementationExamAssessable",
                type: "line",
                startX: 400,
                startY: 450,
                endX: 600,
                endY: 350,
                color: "#95a5a6",
                lineWidth: 2,
                points: [],
                is_corners_rounded: true,
                max_radius_of_corners: 5
            }
        ];


        lines.forEach(line => {
            const new_line = new Graph.Line(line)
            new_line.draw_canvas(ctx);
            graph.addEdge(new_line);
            graph_figures.push(new_line);
        })

        nodes.forEach(node => {
            switch (node.type) {
                case 'circle': {
                    const new_circle = new Graph.Circle((node as Graph.ICircle))
                    new_circle.draw_canvas(ctx);
                    graph.addNode(new_circle);
                    graph_figures.push(new_circle);
                    break;
                }
                case 'rectangle': {
                    const tmp_object = new Graph.Rectangle((node as Graph.IRectangle))
                    tmp_object.draw_canvas(ctx);
                    graph.addNode(tmp_object);
                    graph_figures.push(tmp_object);
                    break;
                }
                case 'rhombus': {
                    const tmp_object = new Graph.Rhombus((node as Graph.IRhombus))
                    tmp_object.draw_canvas(ctx);
                    graph.addNode(tmp_object);
                    graph_figures.push(tmp_object);
                    break;
                }
                case 'ellipse': {
                    const tmp_object = new Graph.Ellipse((node as Graph.IEllipse))
                    tmp_object.draw_canvas(ctx);
                    graph.addNode(tmp_object);
                    graph_figures.push(tmp_object);
                    break;
                }
            }

        })
    }
}