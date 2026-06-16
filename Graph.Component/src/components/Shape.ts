export interface IShape {
    //id: number
    type: string
    rotation?: number
    info?: string
    wasInside: boolean
}

export interface ICircle extends IShape {
    x: number
    y: number
    radius: number
    color?: string
    //is edge puctir
}

export interface IRectangle extends IShape {
    x: number
    y: number
    width: number
    height: number
    color?: string
    //is edge puctir
}

export interface ITriangle extends IShape {
    x_1: number
    y_1: number
    x_2: number
    y_2: number
    x_3: number
    y_3: number
    color?: string
    //is edge puctir
}

export interface IRegularPolygon extends IShape {
    radius: number
    number_of_edges: number
    x: number
    y: number
    color?: string
    //is edge puctir
}

export interface ICustomDescription {
    typeName: string;
    points: Array<{ x: number; y: number }>;
    curve: Array<{ isCurved: boolean; cp1x: number; cp1y: number; cp2x: number; cp2y: number; }>;
    //is edge puctir
}

export interface ICustomShape extends IShape {
    x_center: number;
    y_center: number;
    color?: string
    //is edge puctir
}

export interface ILine extends IShape {
    startX: number
    startY: number
    endX: number
    endY: number
    color?: string
    label?: string | null
    // is puctir
}

//interface IArrow extends ILine {
//    is_begging_arrow: boolean
//    is_end_arrow: boolean
//}

export interface CanvasSize {
    width: number
    height: number
}

export function isILine(arg: any): arg is ILine {
    return arg && arg.type && arg.type == 'line';
}
export function isICircle(arg: any): arg is ICircle {
    return arg && arg.type && arg.type == 'circle';
}

export function isIRectangle(arg: any): arg is IRectangle {
    return arg && arg.type && arg.type == 'rectangle';
}

export type DataFigure = ILine | ICircle | IRectangle | ITriangle | IRegularPolygon | IShape | ICustomShape
export type DataShapes = Line | Circle | Rectangle | Triangle | RegularPolygon | CustomShape | Node | null

export function dataProcessing(data: DataFigure[], ctx: CanvasRenderingContext2D) {
    for (const item of data) {
        switch (item.type) {
            case 'line': {
                //draw_line(ctx, item as ILine);
                const tmp_line = new Line(item as ILine);
                tmp_line.draw_canvas(ctx);

                //if (isILine(item)) {
                //    draw_line(ctx, item);
                //}
                break;
            }                
            case 'circle': {
                //draw_circle(ctx, item as ICircle);

                const tmp_circle = new Circle(item as ICircle);
                tmp_circle.draw_canvas(ctx);

                //if (isICircle(item)) {
                //    draw_circle(ctx, item);
                //}
                break;
            }                
            case 'rectangle': {
                //draw_rectangle(ctx, item as IRectangle);
                const tmp_rect = new Rectangle(item as IRectangle)
                tmp_rect.draw_canvas(ctx);
                //if (isIRectangle(item)) {
                //    draw_rectangle(ctx, item);
                //}
                break;
            }
            case 'triangle': {
                const tmp_tri = new Triangle(item as ITriangle)
                tmp_tri.draw_canvas(ctx);
                break;
            }
            case 'regular polygon': {
                const tmp_reg_polygon = new RegularPolygon(item as IRegularPolygon)
                tmp_reg_polygon.draw_canvas(ctx);
                break;
            }
        }
    }
}

export function draw_circle(ctx: CanvasRenderingContext2D, circle: ICircle) {
    if (!ctx) return;
    if (circle.type != 'circle') return;

    if (typeof circle.rotation === 'undefined') {
        circle.rotation = 0
    }

    const radian = circle.rotation! * Math.PI / 180;

    ctx.save();
    ctx.translate(circle.x, circle.y);
    ctx.rotate(radian);

    ctx.beginPath();
    ctx.arc(0, 0, circle.radius, 0, 2 * Math.PI)
    ctx.stroke();
    if (typeof circle.color !== 'undefined' && circle.color.length > 0) {
        ctx.fillStyle = circle.color!;
        ctx.fill();
    }
    ctx.closePath();
    ctx.restore();
}

export function draw_rectangle(ctx: CanvasRenderingContext2D, rect: IRectangle) {
    if (!ctx) return;
    if (rect.type != 'rectangle') return;

    if (typeof rect.rotation === 'undefined') {
        rect.rotation = 0;
    }

    ctx.save();

    const radian = rect.rotation! * Math.PI / 180;

    ctx.translate(rect.x + rect.width / 2, rect.y + rect.height / 2);
    ctx.rotate(radian);

    if (typeof rect.color !== 'undefined' && rect.color.length > 0) {
        ctx.fillStyle = rect.color!;
        ctx.fillRect(rect.width / 2 * (-1), rect.height / 2 * (-1), rect.width, rect.height)

    } else {
        //ctx.setLineDash([6]); // future: dashed line
        ctx.strokeRect(rect.width / 2 * (-1), rect.height / 2 * (-1), rect.width, rect.height);
    }

    ctx.restore();
}

//TODO: label
export function draw_line(ctx: CanvasRenderingContext2D, line: ILine) {
    if (!ctx) return;
    if (line.type != 'line') return;
    
    if (typeof line.rotation === 'undefined') {
        line.rotation = 0
    }

    const radian = line.rotation! * Math.PI / 180;
    const centerX = (line.startX + line.endX) / 2;
    const centerY = (line.startY + line.endY) / 2;

    ctx.save();

    ctx.translate(centerX, centerY);
    ctx.rotate(radian);

    ctx.beginPath();
    ctx.moveTo(-((line.endX - line.startX) / 2), -((line.endY - line.startY) / 2));
    ctx.lineTo(((line.endX - line.startX) / 2), ((line.endY - line.startY) / 2));
    if (typeof line.color !== 'undefined' && line.color.length > 0) {
        ctx.strokeStyle = line.color!;
    }
    ctx.stroke();
    ctx.closePath();

    ctx.restore();
}

export class Node implements IShape {
    //private _id: number;
    private _type: string;
    private _rotation?: number;
    private _color?: string;
    private _info?: string;
    private _wasInside: boolean = false;


    constructor(type: string, color?: string, rotation?: number, info?: string) {
        this._type = type
        //if (type != 'circle') return;
        if (typeof rotation !== 'undefined') {
            this._rotation = rotation
        }
        this._color = color
        this._info = info
    }

    draw_canvas(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;
        // TODO
    }

    draw_hovered(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

    }

    draw_clicked(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

    }

    //TODO:
    is_inside(mouseX: number, mouseY: number) {
        return (false
            //mouseX >= this.x - this.radius &&
            //mouseX <= this.x + this.radius &&
            //mouseY >= this.y - this.radius &&
            //mouseY <= this.y + this.radius
        )
    }

    get type() {
        return this._type;
    }

    get rotation() {
        return this._rotation;
    }

    get color() {
        return this._color;
    }

    get info() {
        return this._info;
    }

    get wasInside() {
        return this._wasInside;
    }
}


export class CustomShape implements IShape {
    //private _id: number;
    private _type: string;
    private _rotation?: number;
    private _x_center: number;
    private _y_center: number;
    private points: Array<{ x: number; y: number }> = [];
    private curve: Array<{ isCurved: boolean; cp1x: number; cp1y: number; cp2x: number; cp2y: number; }> = [];
    private _color?: string;
    private _info?: string;
    private _wasInside: boolean = false;
    private _scale: number;


    constructor(custom: ICustomShape, description: ICustomDescription) {
        //if (custom.type != description.typeName) return;
        this._type = description.typeName

        if (typeof custom.rotation !== 'undefined') {
            this._rotation = custom.rotation
        }
        this._color = custom.color
        this._info = custom.info
        this._x_center = custom.x_center
        this._y_center = custom.y_center
        this.points = description.points.slice()

        // check length points == curve
        this.curve = description.curve.slice()

        this._scale = 1;
    }

    

    draw_canvas(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;
        ctx.save();

        ctx.translate(this.x_center, this.y_center);
        //ctx.rotate(radian);

        ctx.beginPath();

        ctx.moveTo(this.points[0].x, this.points[0].y);
        for (let i = 1; i < this.points.length; i++) {
            //if (typeof this.curve[i - 1].cp1x !== 'undefined' && typeof this.curve[i - 1].cp1y !== 'undefined' && typeof this.curve[i - 1].cp2x !== 'undefined' && typeof this.curve[i - 1].cp2y !== 'undefined')
            if (this.curve[i - 1].isCurved) {
                ctx.bezierCurveTo(this.curve[i - 1].cp1x, this.curve[i - 1].cp1y, this.curve[i - 1].cp2x, this.curve[i - 1].cp2y, this.points[i].x, this.points[i].y);
            } else {
                ctx.lineTo(this.points[i].x, this.points[i].y);
            }
        }
        ctx.closePath();

        //ctx.lineWidth = 5;

        if (typeof this.color !== 'undefined' && this.color.length > 0) {
            ctx.fillStyle = this.color!;
            ctx.fill();
        }
        
        //ctx.strokeStyle = '#0000ff';
        ctx.stroke();

        ctx.restore();
    }

    draw_hovered(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

    }

    draw_clicked(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

    }

    //TODO:
    is_inside(mouseX: number, mouseY: number) {
        return (false
            //mouseX >= this.x - this.radius &&
            //mouseX <= this.x + this.radius &&
            //mouseY >= this.y - this.radius &&
            //mouseY <= this.y + this.radius
        )
    }

    get type() {
        return this._type;
    }

    get rotation() {
        return this._rotation;
    }

    get x_center() {
        return this._x_center;
    }

    get y_center() {
        return this._y_center;
    }

    get color() {
        return this._color;
    }
    get scale() {
        return this._scale;
    }

    get info() {
        return this._info;
    }

    get wasInside() {
        return this._wasInside;
    }
}
export class Rectangle implements IRectangle {
    //private _id: number;
    private _type: string = 'rectangle';
    private _rotation?: number = 0;
    private _x: number;
    private _y: number;
    private _width: number;
    private _height: number;
    private _color?: string;
    private _info?: string;
    private _wasInside: boolean = false;

    //constructor(x: number, y: number, width: number, height: number, color?: string, rotation?: number, info?: string) {
    constructor(rect: IRectangle) { 
        //this.type = type
        if (typeof rect.rotation !== 'undefined') {
            this._rotation = rect.rotation
        }
        this._x = rect.x
        this._y = rect.y
        this._width = rect.width
        this._height = rect.height
        this._color = rect.color
        this._info = rect.info
    }

    draw_canvas(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        ctx.save();

        const radian = this.rotation! * Math.PI / 180;

        ctx.translate(this.x + this.width / 2, this.y + this.height / 2);
        ctx.rotate(radian);

        if (typeof this.color !== 'undefined' && this.color.length > 0) {
            ctx.fillStyle = this.color!;
            ctx.fillRect(this.width / 2 * (-1), this.height / 2 * (-1), this.width, this.height)

        } else {
            //ctx.setLineDash([6]); // future: dashed line
            ctx.strokeRect(this.width / 2 * (-1), this.height / 2 * (-1), this.width, this.height);
        }

        ctx.restore();
    }

    draw_hovered(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        ctx.save();

        const radian = this.rotation! * Math.PI / 180;

        ctx.translate(this.x + this.width / 2, this.y + this.height / 2);
        ctx.rotate(radian);

        ctx.fillStyle = 'skyblue';
        ctx.fillRect(this.width / 2 * (-1), this.height / 2 * (-1), this.width, this.height)

        ctx.restore();

    }

    draw_clicked(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        ctx.save();

        const radian = this.rotation! * Math.PI / 180;

        ctx.translate(this.x + this.width / 2, this.y + this.height / 2);
        ctx.rotate(radian);

        ctx.fillStyle = '#b57281';
        ctx.fillRect(this.width / 2 * (-1), this.height / 2 * (-1), this.width, this.height)

        ctx.restore();
    }

    is_inside(mouseX: number, mouseY: number) {
        return (
            mouseX >= this.x &&
            mouseX <= this.x + this.width &&
            mouseY >= this.y &&
            mouseY <= this.y + this.height
            )
    }

    get type() {
        return this._type;
    }

    get rotation() {
        return this._rotation;
    }

    get x() {
        return this._x;
    }

    get y() {
        return this._y;
    }

    get width() {
        return this._width;
    }

    get height() {
        return this._height;
    }

    get color() {
        return this._color;
    }

    get info() {
        return this._info;
    }

    get wasInside() {
        return this._wasInside;
    }
}

//TODO: rotation
export class Triangle implements ITriangle {
    //private _id: number;
    private _type: string = 'triangle';
    private _rotation?: number;
    private _x_1: number
    private _y_1: number
    private _x_2: number
    private _y_2: number
    private _x_3: number
    private _y_3: number
    private _color?: string;
    private _info?: string;
    private _wasInside: boolean = false;

    //constructor(x_1: number, y_1: number, x_2: number, y_2: number, x_3: number, y_3: number, color?: string, rotation?: number, info?: string) {
    constructor(triangle: ITriangle) {
        //this.type = type
        //if (type != 'circle') return;
        if (typeof triangle.rotation !== 'undefined') {
            this._rotation = triangle.rotation
        }
        this._x_1 = triangle.x_1
        this._y_1 = triangle.y_1
        this._x_2 = triangle.x_2
        this._y_2 = triangle.y_2
        this._x_3 = triangle.x_3
        this._y_3 = triangle.y_3
        this._color = triangle.color
        this._info = triangle.info
    }

    // TODO: ROTATION
    draw_canvas(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        const radian = this.rotation! * Math.PI / 180;

        //ctx.save();
        //ctx.translate(this.x_1, this.y_1);
        //ctx.rotate(radian);

        ctx.beginPath();

        ctx.moveTo(this.x_1, this.y_1);
        ctx.lineTo(this.x_2, this.y_2);
        ctx.lineTo(this.x_3, this.y_3);
        ctx.closePath();

        // the outline
        //ctx.lineJoin = "round";
        //ctx.lineWidth = 10;
        //ctx.strokeStyle = '#666666';
        ctx.stroke();

        if (typeof this.color !== 'undefined' && this.color.length > 0) {
            ctx.fillStyle = this.color!;
            ctx.fill();
        }

        //ctx.restore();
    }

    draw_hovered(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        const radian = this.rotation! * Math.PI / 180;

        //ctx.save();
        //ctx.translate(this.x_1, this.y_1);
        //ctx.rotate(radian);

        ctx.beginPath();

        ctx.moveTo(this.x_1, this.y_1);
        ctx.lineTo(this.x_2, this.y_2);
        ctx.lineTo(this.x_3, this.y_3);
        ctx.closePath();

        // the outline
        //ctx.lineJoin = "round";
        //ctx.lineWidth = 10;
        //ctx.strokeStyle = '#666666';
        ctx.stroke();

        ctx.fillStyle = 'skyblue';
        ctx.fill();
        

        //ctx.restore();
    }

    draw_clicked(ctx: CanvasRenderingContext2D) {
        if(!ctx) return;

        const radian = this.rotation! * Math.PI / 180;

        //ctx.save();
        //ctx.translate(this.x_1, this.y_1);
        //ctx.rotate(radian);

        ctx.beginPath();

        ctx.moveTo(this.x_1, this.y_1);
        ctx.lineTo(this.x_2, this.y_2);
        ctx.lineTo(this.x_3, this.y_3);
        ctx.closePath();

        // the outline
        //ctx.lineJoin = "round";
        //ctx.lineWidth = 10;
        //ctx.strokeStyle = '#666666';
        ctx.stroke();

        ctx.fillStyle = '#b57281';
        ctx.fill();
    }


    is_inside(mouseX: number, mouseY: number) {
        /* Calculate area of triangle ABC */
        const area_A = this.area_of_shape(this.x_1, this.y_1, this.x_2, this.y_2, this.x_3, this.y_3)

        /* Calculate area of triangle PBC */
        const area_A1 = this.area_of_shape(mouseX, mouseY, this.x_2, this.y_2, this.x_3, this.y_3)

        /* Calculate area of triangle PAC */
        const area_A2 = this.area_of_shape(this.x_1, this.y_1, mouseX, mouseY, this.x_3, this.y_3)

        /* Calculate area of triangle PAB */
        const area_A3 = this.area_of_shape(this.x_1, this.y_1, this.x_2, this.y_2, mouseX, mouseY)

        /* Check if sum of A1, A2 and A3 is same as A */
        return (area_A == area_A1 + area_A2 + area_A3);
    }

    area_of_shape(x1: number, y1: number, x2: number, y2: number, x3: number, y3: number) {
        return Math.abs((x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2)) / 2.0);
    } 

    get type() {
        return this._type;
    }

    get rotation() {
        return this._rotation;
    }

    get x_1() {
        return this._x_1;
    }

    get y_1() {
        return this._y_1;
    }

    get x_2() {
        return this._x_2;
    }

    get y_2() {
        return this._y_2;
    }

    get x_3() {
        return this._x_3;
    }

    get y_3() {
        return this._y_3;
    }

    get color() {
        return this._color;
    }

    get info() {
        return this._info;
    }

    get wasInside() {
        return this._wasInside;
    }
}

export class Circle implements ICircle {
    //private _id: number;
    private _type: string = 'circle';
    private _rotation?: number;
    private _x: number;
    private _y: number;
    private _radius: number;
    private _color?: string;
    private _info?: string;
    private _wasInside: boolean = false;

    //constructor(x: number, y: number, radius: number, color?: string, rotation?: number, info?: string) {
    constructor(circle: ICircle) {
        //this.type = type
        //if (type != 'circle') return;
        if (typeof circle.rotation !== 'undefined') {
            this._rotation = circle.rotation
        }
        this._x = circle.x
        this._y = circle.y
        this._radius = circle.radius
        this._color = circle.color
        this._info = circle.info
    }

    draw_canvas(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        const radian = this.rotation! * Math.PI / 180;

        ctx.save();
        ctx.translate(this.x, this.y);
        ctx.rotate(radian);

        ctx.beginPath();
        ctx.arc(0, 0, this.radius, 0, 2 * Math.PI)
        ctx.stroke();
        if (typeof this.color !== 'undefined' && this.color.length > 0) {
            ctx.fillStyle = this.color!;
            ctx.fill();
        }
        ctx.closePath();
        ctx.restore();
    }

    draw_hovered(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        const radian = this.rotation! * Math.PI / 180;

        ctx.save();
        ctx.translate(this.x, this.y);
        ctx.rotate(radian);

        ctx.beginPath();
        ctx.arc(0, 0, this.radius, 0, 2 * Math.PI)
        ctx.stroke();
        //if (typeof this.color !== 'undefined' && this.color.length > 0) {
        //    ctx.fillStyle = this.color!;
        //    ctx.fill();
        //}

        ctx.fillStyle = 'skyblue';
        ctx.fill();

        ctx.closePath();
        ctx.restore();
    }

    draw_clicked(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        const radian = this.rotation! * Math.PI / 180;

        ctx.save();
        ctx.translate(this.x, this.y);
        ctx.rotate(radian);

        ctx.beginPath();
        ctx.arc(0, 0, this.radius, 0, 2 * Math.PI)
        ctx.stroke();
        //if (typeof this.color !== 'undefined' && this.color.length > 0) {
        //    ctx.fillStyle = this.color!;
        //    ctx.fill();
        //}

        ctx.fillStyle = '#b57281';
        ctx.fill();

        ctx.closePath();
        ctx.restore();
    }

    is_inside(mouseX: number, mouseY: number) {
        return (
            mouseX >= this.x - this.radius &&
            mouseX <= this.x + this.radius &&
            mouseY >= this.y - this.radius &&
            mouseY <= this.y + this.radius
        )
    }

    get type() {
        return this._type;
    }

    get rotation() {
        return this._rotation;
    }

    get x() {
        return this._x;
    }

    get y() {
        return this._y;
    }

    get radius() {
        return this._radius;
    }

    get color() {
        return this._color;
    }

    get info() {
        return this._info;
    }

    get wasInside() {
        return this._wasInside;
    }
}


export class RegularPolygon implements IRegularPolygon {
    //private _id: number;
    private _type: string = 'regular polygon';
    private _rotation?: number = 0;
    private _number_of_edges: number
    private _x: number
    private _y: number
    private _radius: number;
    private _color?: string;
    private _info?: string;
    private _wasInside: boolean = false;
    private points: Array<{ x: number; y: number }> = [];

    //constructor(number_of_edges: number, x: number, y: number, radius: number, color?: string, rotation?: number, info?: string) {
    constructor(polygon: IRegularPolygon) {
        //this.type = type
        //if (type != 'circle') return;
        if (typeof polygon.rotation !== 'undefined') {
            this._rotation = polygon.rotation
        }
        this._number_of_edges = polygon.number_of_edges
        this._x = polygon.x
        this._y = polygon.y
        this._radius = polygon.radius
        this._color = polygon.color
        this._info = polygon.info

        const radian = this.rotation! * Math.PI / 180;

        this.points.push({ x: (this.x + this.radius * Math.cos(0 + radian)), y: this.y + this.radius * Math.sin(0 + radian) })

        for (let i = 1; i <= this.number_of_edges; i += 1) {
            this.points.push({ x: (this.x + this.radius * Math.cos(i * 2 * Math.PI / this.number_of_edges + radian)), y: this.y + this.radius * Math.sin(i * 2 * Math.PI / this.number_of_edges + radian) })
        }
    }
    
    draw_canvas(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        ctx.save();
        //const radian = this.rotation! * Math.PI / 180;
        //ctx.translate(this.x, this.y);
        //ctx.rotate(radian);

        ctx.beginPath();
        ctx.moveTo(this.points[0].x, this.points[0].y);

        for (let i = 1; i <= this.number_of_edges; i ++) {
            ctx.lineTo(this.points[i].x, this.points[i].y);
        }

        //ctx.strokeStyle = "#000000";
        //ctx.lineWidth = 1;

        if (typeof this.color !== 'undefined' && this.color.length > 0) {
            ctx.fillStyle = this.color!;
            ctx.fill();
        }

        ctx.stroke();

        ctx.closePath();

        ctx.restore();
    }

    draw_hovered(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        ctx.save();

        ctx.beginPath();
        ctx.moveTo(this.points[0].x, this.points[0].y);

        for (let i = 1; i <= this.number_of_edges; i++) {
            ctx.lineTo(this.points[i].x, this.points[i].y);
        }

        //ctx.strokeStyle = "#000000";
        //ctx.lineWidth = 1;

        ctx.fillStyle = 'skyblue';
        ctx.fill();

        ctx.stroke();

        ctx.closePath();

        ctx.restore();
    }

    draw_clicked(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        ctx.save();

        ctx.beginPath();
        ctx.moveTo(this.points[0].x, this.points[0].y);

        for (let i = 1; i <= this.number_of_edges; i++) {
            ctx.lineTo(this.points[i].x, this.points[i].y);
        }

        //ctx.strokeStyle = "#000000";
        //ctx.lineWidth = 1;

        ctx.fillStyle = '#b57281';
        ctx.fill();

        ctx.stroke();

        ctx.closePath();

        ctx.restore();
    }

    //TODO:
    is_inside(mouseX: number, mouseY: number) {
        //const points: Array<{ x: number; y: number }> = [];

        //points.push({ x: (this.x + this.radius * Math.cos(0)), y: this.y + this.radius * Math.sin(0) })

        //for (let i = 1; i <= this.number_of_edges; i += 1) {
        //    points.push({ x: (this.x + this.radius * Math.cos(i * 2 * Math.PI / this.number_of_edges)), y: this.y + this.radius * Math.sin(i * 2 * Math.PI / this.number_of_edges) })
        //}

        let res: boolean = false;
        let j = this.number_of_edges - 1;
        for (let i = 0; i < this.number_of_edges; i++) {
            if ((this.points[i].y < mouseY && this.points[j].y >= mouseY || this.points[j].y < mouseY && this.points[i].y >= mouseY) &&
                (this.points[i].x + (mouseY - this.points[i].y) / (this.points[j].y - this.points[i].y) * (this.points[j].x - this.points[i].x) < mouseX)) {
                res = !res;
            }
            j = i
        }

        return res;
    }

    get type() {
        return this._type;
    }

    get rotation() {
        return this._rotation;
    }

    get x() {
        return this._x;
    }

    get y() {
        return this._y;
    }
    get number_of_edges() {
        return this._number_of_edges;
    }

    get radius() {
        return this._radius;
    }

    get color() {
        return this._color;
    }

    get info() {
        return this._info;
    }

    get wasInside() {
        return this._wasInside;
    }
}

export class Line implements ILine {
    //private _id: number;
    private _type: string = 'line';
    private _rotation?: number = 0;
    private _startX: number;
    private _startY: number;
    private _endX: number;
    private _endY: number;
    private _color?: string;
    private _label?: string | null;
    private _info?: string;
    private _wasInside: boolean = false;

    //constructor(startX: number, startY: number, endX: number, endY: number, color?: string, rotation?: number, label?: string | null, info?: string) {
    constructor(line: ILine) {
        //this.type = type
        //if (type != 'line') return;

        if (typeof line.rotation !== 'undefined') {
            this._rotation = line.rotation
        }
        this._startX = line.startX
        this._startY = line.startY
        this._endX = line.endX
        this._endY = line.endY
        this._color = line.color
        this._label = line.label
        this._info = line.info
    }

    draw_canvas(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        const radian = this.rotation! * Math.PI / 180;
        const centerX = (this.startX + this.endX) / 2;
        const centerY = (this.startY + this.endY) / 2;

        ctx.save();

        ctx.translate(centerX, centerY);
        ctx.rotate(radian);

        ctx.beginPath();
        ctx.moveTo(-((this.endX - this.startX) / 2), -((this.endY - this.startY) / 2));
        ctx.lineTo(((this.endX - this.startX) / 2), ((this.endY - this.startY) / 2));

        //ctx.lineWidth = 5;

        if (typeof this.color !== 'undefined' && this.color.length > 0) {
            ctx.strokeStyle = this.color!;
        }
        ctx.stroke();
        ctx.closePath();

        ctx.restore();
    }

    draw_hovered(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;
        const radian = this.rotation! * Math.PI / 180;
        const centerX = (this.startX + this.endX) / 2;
        const centerY = (this.startY + this.endY) / 2;

        ctx.save();

        ctx.translate(centerX, centerY);
        ctx.rotate(radian);

        ctx.beginPath();
        ctx.moveTo(-((this.endX - this.startX) / 2), -((this.endY - this.startY) / 2));
        ctx.lineTo(((this.endX - this.startX) / 2), ((this.endY - this.startY) / 2));
        //if (typeof this.color !== 'undefined' && this.color.length > 0) {
        ctx.strokeStyle = 'gold';
        //}
        ctx.stroke();
        ctx.closePath();

        ctx.restore();
    }

    draw_clicked(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;
        const radian = this.rotation! * Math.PI / 180;
        const centerX = (this.startX + this.endX) / 2;
        const centerY = (this.startY + this.endY) / 2;

        ctx.save();

        ctx.translate(centerX, centerY);
        ctx.rotate(radian);

        ctx.beginPath();
        ctx.moveTo(-((this.endX - this.startX) / 2), -((this.endY - this.startY) / 2));
        ctx.lineTo(((this.endX - this.startX) / 2), ((this.endY - this.startY) / 2));
        //if (typeof this.color !== 'undefined' && this.color.length > 0) {
        ctx.strokeStyle = '#b57281';
        //}
        ctx.stroke();
        ctx.closePath();

        ctx.restore();
    }

    //TODO:
    is_inside(mouseX: number, mouseY: number) {
        const dx = this.endX - this.startX,
            dy = this.endY - this.startY,
            ds = this.startX * this.endY - this.endX * this.startY;

        return (mouseX * dy - mouseY * dx === ds)

        //return (mouseX * (this.endY - this.startY) - mouseY * (this.endX - this.startX) === this.startX * this.endY - this.endX * this.startY)
    }

    get type() {
        return this._type;
    }

    get rotation() {
        return this._rotation;
    }

    get startX() {
        return this._startX;
    }

    get startY() {
        return this._startY;
    }

    get endX() {
        return this._endX;
    }

    get endY() {
        return this._endY;
    }

    get color() {
        return this._color;
    }

    get label() {
        return this._label;
    }

    get info() {
        return this._info;
    }

    get wasInside() {
        return this._wasInside;
    }
}