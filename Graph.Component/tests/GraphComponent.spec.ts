import { flushPromises, mount } from '@vue/test-utils';
import { nextTick } from 'vue';
import { describe, expect, it } from 'vitest';
import GraphComponent from '../src/components/GraphComponent.vue';
import {
    getLatestCanvasContext,
    runAnimationFrames
} from './setupCanvas';
import {
    achievementGraphXml,
    refreshedAchievementGraphXml
} from './fixtures/achievementGraphXml';

async function mountGraph(xml = achievementGraphXml) {
    const wrapper = mount(GraphComponent, {
        attachTo: document.body,
        props: {
            xmlData: xml
        }
    });

    await nextTick();
    await flushPromises();
    runAnimationFrames();

    return wrapper;
}

function findById(items: any[], id: string): any {
    const item = items.find((candidate) => candidate?._id === id);

    if (!item) {
        throw new Error(`Graph item ${id} was not found`);
    }

    return item;
}

describe('GraphComponent', () => {
    it('creates graph objects from XML and applies achievement status styles', async () => {
        const wrapper = await mountGraph();
        const vm = wrapper.vm as any;
        const canvas = wrapper.find('canvas').element as HTMLCanvasElement;

        const earnedNode = findById(vm.getNodes(), 'earned-node');
        const lockedNode = findById(vm.getNodes(), 'locked-node');
        const availableEdge = findById(vm.getEdges(), 'available-edge');
        const lockedEdge = findById(vm.getEdges(), 'locked-edge');

        expect(canvas.width).toBe(420);
        expect(canvas.height).toBe(260);
        expect(canvas.style.backgroundColor).toBe('rgb(240, 244, 255)');
        expect(vm.getNodes()).toHaveLength(2);
        expect(vm.getEdges()).toHaveLength(2);

        expect(earnedNode._color).toBe('#4caf50');
        expect(earnedNode._info).toBe('Достижение получено');
        expect(lockedNode._color).toBe('#cfd8dc');
        expect(lockedNode._info).toBe('Достижение пока закрыто');
        expect(lockedNode._label_info).toMatchObject({
            text: '',
            icon: 'lock'
        });

        expect(availableEdge._color).toBe('#f9a825');
        expect(availableEdge._info).toBe('Достижение доступно');
        expect(availableEdge._source?._id).toBe('earned-node');
        expect(availableEdge._target?._id).toBe('locked-node');
        expect(lockedEdge._color).toBe('#90a4ae');
        expect(lockedEdge._isEdgeDash).toBe(true);
        expect(lockedEdge._source?._id).toBe('locked-node');
        expect(lockedEdge._target?._id).toBe('earned-node');
    });

    it('centers graph nodes in the viewport after loading XML', async () => {
        const wrapper = await mountGraph();
        const vm = wrapper.vm as any;

        expect(vm.graph._scale).toBe(1);
        expect(vm.graph._offsetX).toBeCloseTo(47);
        expect(vm.graph._offsetY).toBeCloseTo(50);
    });

    it('draws edges before nodes on canvas', async () => {
        await mountGraph();

        const context = getLatestCanvasContext();
        const firstAvailableEdgeStroke = context.calls.findIndex((call) =>
            call.name === 'set:strokeStyle' && call.args[0] === '#f9a825');
        const firstLockedEdgeStroke = context.calls.findIndex((call) =>
            call.name === 'set:strokeStyle' && call.args[0] === '#90a4ae');
        const firstEarnedNodeFill = context.calls.findIndex((call) =>
            call.name === 'set:fillStyle' && call.args[0] === '#4caf50');
        const firstLockedNodeFill = context.calls.findIndex((call) =>
            call.name === 'set:fillStyle' && call.args[0] === '#cfd8dc');

        expect(firstAvailableEdgeStroke).toBeGreaterThanOrEqual(0);
        expect(firstLockedEdgeStroke).toBeGreaterThanOrEqual(0);
        expect(firstEarnedNodeFill).toBeGreaterThanOrEqual(0);
        expect(firstLockedNodeFill).toBeGreaterThanOrEqual(0);
        expect(firstAvailableEdgeStroke).toBeLessThan(firstEarnedNodeFill);
        expect(firstLockedEdgeStroke).toBeLessThan(firstLockedNodeFill);
    });

    it('updates rendered graph when XML changes', async () => {
        const wrapper = await mountGraph();
        const vm = wrapper.vm as any;

        expect(findById(vm.getNodes(), 'locked-node')._color).toBe('#cfd8dc');
        expect(findById(vm.getEdges(), 'available-edge')._color).toBe('#f9a825');

        await wrapper.setProps({
            xmlData: refreshedAchievementGraphXml
        });
        await nextTick();
        await flushPromises();
        runAnimationFrames();

        expect(vm.getNodes()).toHaveLength(2);
        expect(vm.getEdges()).toHaveLength(1);
        expect(findById(vm.getNodes(), 'locked-node')._color).toBe('#4caf50');
        expect(findById(vm.getEdges(), 'available-edge')._color).toBe('#2e7d32');
    });

    it('supports zooming and panning without breaking hit testing', async () => {
        const wrapper = await mountGraph();
        const vm = wrapper.vm as any;
        const canvas = wrapper.find('canvas').element as HTMLCanvasElement;
        const context = getLatestCanvasContext();

        canvas.dispatchEvent(new WheelEvent('wheel', {
            clientX: 100,
            clientY: 80,
            deltaY: -1,
            bubbles: true,
            cancelable: true
        }));
        runAnimationFrames();

        expect(context.calls).toContainEqual({
            name: 'setTransform',
            args: [1.1, 0, 0, 1.1, 41.699999999999996, 47]
        });
        expect(vm.getObjectAt(130, 135)?._id).toBe('earned-node');

        canvas.dispatchEvent(new MouseEvent('mousedown', {
            clientX: 10,
            clientY: 10,
            button: 0,
            bubbles: true
        }));
        canvas.dispatchEvent(new MouseEvent('mousemove', {
            clientX: 30,
            clientY: 45,
            bubbles: true
        }));
        runAnimationFrames();

        expect(context.calls).toContainEqual({
            name: 'setTransform',
            args: [1.1, 0, 0, 1.1, 61.699999999999996, 82]
        });
    });

    it('clears graph data through public API', async () => {
        const wrapper = await mountGraph();
        const vm = wrapper.vm as any;

        expect(vm.getNodes()).toHaveLength(2);
        expect(vm.getEdges()).toHaveLength(2);

        vm.clearGraph();
        runAnimationFrames();

        expect(vm.getNodes()).toHaveLength(0);
        expect(vm.getEdges()).toHaveLength(0);
    });
});
