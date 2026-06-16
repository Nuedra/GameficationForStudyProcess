import { flushPromises, mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import AchievementGraphPanel from '../src/components/AchievementGraphPanel.vue';
import {
    getAchievementGraphXml,
    logoutStudent,
    refreshAchievementGraphXml
} from '../src/components/achievementGraphApi';
import {
    achievementGraphXml,
    refreshedAchievementGraphXml
} from './fixtures/achievementGraphXml';

vi.mock('../src/components/achievementGraphApi', () => ({
    getAchievementGraphXml: vi.fn(),
    refreshAchievementGraphXml: vi.fn(),
    logoutStudent: vi.fn()
}));

const mockedGetGraph = vi.mocked(getAchievementGraphXml);
const mockedRefreshGraph = vi.mocked(refreshAchievementGraphXml);
const mockedLogout = vi.mocked(logoutStudent);

describe('AchievementGraphPanel', () => {
    beforeEach(() => {
        mockedGetGraph.mockReset();
        mockedRefreshGraph.mockReset();
        mockedLogout.mockReset();
    });

    it('keeps graph area empty until the student asks to draw the graph', () => {
        const wrapper = mount(AchievementGraphPanel, {
            props: {
                courseId: 'course-1',
                year: 2026
            },
            global: {
                stubs: {
                    GraphComponent: true
                }
            }
        });

        expect(wrapper.text()).toContain('Граф появится после нажатия кнопки отрисовки.');
        expect(wrapper.findComponent({ name: 'GraphComponent' }).exists()).toBe(false);
        expect(wrapper.findAll('button')[1].attributes('disabled')).toBeDefined();
        expect(mockedGetGraph).not.toHaveBeenCalled();
    });

    it('loads graph XML, refreshes it and clears it on logout', async () => {
        mockedGetGraph.mockResolvedValue(achievementGraphXml);
        mockedRefreshGraph.mockResolvedValue(refreshedAchievementGraphXml);
        mockedLogout.mockResolvedValue(undefined);

        const wrapper = mount(AchievementGraphPanel, {
            props: {
                courseId: 'course-1',
                year: 2026,
                width: 900,
                height: 500
            },
            global: {
                stubs: {
                    GraphComponent: {
                        name: 'GraphComponent',
                        props: ['xmlData', 'width', 'height'],
                        template: '<div class="graph-component-stub" />'
                    }
                }
            }
        });

        await wrapper.findAll('button')[0].trigger('click');
        await flushPromises();

        expect(mockedGetGraph).toHaveBeenCalledWith('course-1', 2026);
        expect(wrapper.findComponent({ name: 'GraphComponent' }).props()).toMatchObject({
            xmlData: achievementGraphXml,
            width: 900,
            height: 500
        });
        expect(wrapper.emitted('graph-loaded')?.[0]).toEqual([achievementGraphXml]);
        expect(wrapper.findAll('button')[1].attributes('disabled')).toBeUndefined();

        await wrapper.findAll('button')[1].trigger('click');
        await flushPromises();

        expect(mockedRefreshGraph).toHaveBeenCalledWith('course-1', 2026);
        expect(wrapper.findComponent({ name: 'GraphComponent' }).props('xmlData'))
            .toBe(refreshedAchievementGraphXml);
        expect(wrapper.emitted('graph-refreshed')?.[0]).toEqual([refreshedAchievementGraphXml]);

        await wrapper.findAll('button')[2].trigger('click');
        await flushPromises();

        expect(mockedLogout).toHaveBeenCalledTimes(1);
        expect(wrapper.emitted('logout')).toHaveLength(1);
        expect(wrapper.findComponent({ name: 'GraphComponent' }).exists()).toBe(false);
        expect(wrapper.text()).toContain('Граф появится после нажатия кнопки отрисовки.');
    });
});
