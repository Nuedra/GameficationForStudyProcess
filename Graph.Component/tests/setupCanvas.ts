import { afterEach, beforeEach, vi } from 'vitest';

export interface CanvasCall {
    name: string;
    args: unknown[];
}

export type TestCanvasContext = CanvasRenderingContext2D & {
    calls: CanvasCall[];
};

let canvasContexts = new WeakMap<HTMLCanvasElement, TestCanvasContext>();
const createdContexts: TestCanvasContext[] = [];
let animationFrameQueue: FrameRequestCallback[] = [];

function record(context: { calls: CanvasCall[] }, name: string, args: unknown[]): void {
    context.calls.push({ name, args });
}

function method(context: { calls: CanvasCall[] }, name: string): (...args: unknown[]) => void {
    return (...args: unknown[]) => record(context, name, args);
}

function defineContextProperty(context: Record<string, unknown> & { calls: CanvasCall[] }, name: string): void {
    let value: unknown;

    Object.defineProperty(context, name, {
        get: () => value,
        set: (nextValue) => {
            value = nextValue;
            record(context, `set:${name}`, [nextValue]);
        }
    });
}

function createCanvasContext(): TestCanvasContext {
    const context: Record<string, unknown> & { calls: CanvasCall[] } = {
        calls: [],
        canvas: null,
        save: undefined,
        restore: undefined,
        setTransform: undefined,
        clearRect: undefined,
        translate: undefined,
        rotate: undefined,
        scale: undefined,
        beginPath: undefined,
        closePath: undefined,
        moveTo: undefined,
        lineTo: undefined,
        quadraticCurveTo: undefined,
        bezierCurveTo: undefined,
        arcTo: undefined,
        rect: undefined,
        arc: undefined,
        ellipse: undefined,
        fill: undefined,
        stroke: undefined,
        fillRect: undefined,
        strokeRect: undefined,
        clip: undefined,
        drawImage: undefined,
        fillText: undefined,
        strokeText: undefined,
        setLineDash: undefined,
        createRadialGradient: undefined,
        measureText: undefined
    };

    [
        'save',
        'restore',
        'setTransform',
        'clearRect',
        'translate',
        'rotate',
        'scale',
        'beginPath',
        'closePath',
        'moveTo',
        'lineTo',
        'quadraticCurveTo',
        'bezierCurveTo',
        'arcTo',
        'rect',
        'arc',
        'ellipse',
        'fill',
        'stroke',
        'fillRect',
        'strokeRect',
        'clip',
        'drawImage',
        'fillText',
        'strokeText',
        'setLineDash'
    ].forEach((name) => {
        context[name] = vi.fn(method(context, name));
    });

    context.createRadialGradient = vi.fn((...args: unknown[]) => {
        record(context, 'createRadialGradient', args);

        return {
            addColorStop: vi.fn((...colorStopArgs: unknown[]) =>
                record(context, 'gradient:addColorStop', colorStopArgs))
        };
    });

    context.measureText = vi.fn((text: string) => {
        record(context, 'measureText', [text]);

        return {
            width: String(text).length * 7,
            actualBoundingBoxAscent: 8,
            actualBoundingBoxDescent: 2
        };
    });

    [
        'fillStyle',
        'strokeStyle',
        'lineWidth',
        'font',
        'textAlign',
        'textBaseline',
        'globalAlpha'
    ].forEach((name) => defineContextProperty(context, name));

    return context as TestCanvasContext;
}

export function getCreatedCanvasContexts(): TestCanvasContext[] {
    return createdContexts;
}

export function getLatestCanvasContext(): TestCanvasContext {
    const context = createdContexts[createdContexts.length - 1];

    if (!context) {
        throw new Error('Canvas context was not created');
    }

    return context;
}

export function runAnimationFrames(limit = animationFrameQueue.length): void {
    const callbacks = animationFrameQueue.splice(0, limit);

    callbacks.forEach((callback) => callback(performance.now()));
}

beforeEach(() => {
    canvasContexts = new WeakMap<HTMLCanvasElement, TestCanvasContext>();
    createdContexts.length = 0;
    animationFrameQueue = [];

    vi.stubGlobal('requestAnimationFrame', vi.fn((callback: FrameRequestCallback) => {
        animationFrameQueue.push(callback);
        return animationFrameQueue.length;
    }));
    vi.stubGlobal('cancelAnimationFrame', vi.fn());

    vi.spyOn(console, 'error').mockImplementation(() => undefined);
    vi.spyOn(console, 'warn').mockImplementation(() => undefined);

    vi.spyOn(HTMLCanvasElement.prototype, 'getContext').mockImplementation(function getContext(type: string) {
        if (type !== '2d') {
            return null;
        }

        let context = canvasContexts.get(this);

        if (!context) {
            context = createCanvasContext();
            context.canvas = this;
            canvasContexts.set(this, context);
            createdContexts.push(context);
        }

        return context;
    });

    vi.spyOn(HTMLCanvasElement.prototype, 'getBoundingClientRect').mockImplementation(function getBoundingClientRect() {
        const width = this.width || Number(this.getAttribute('width')) || 0;
        const height = this.height || Number(this.getAttribute('height')) || 0;

        return {
            x: 0,
            y: 0,
            left: 0,
            top: 0,
            right: width,
            bottom: height,
            width,
            height,
            toJSON: () => ({})
        };
    });
});

afterEach(() => {
    vi.unstubAllGlobals();
    vi.restoreAllMocks();
});
