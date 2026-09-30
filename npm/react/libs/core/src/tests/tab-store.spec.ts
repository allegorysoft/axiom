import { beforeEach, describe, expect, it } from 'vitest';
import { DEFAULT_MENU_GROUP } from '../menu/menu';
import type { Tab, TabStore } from '../tab/tab';
import { createTabStore } from '../tab/tab-store';

const makeTab = (title: string, isActive = false): Tab =>
  ({ title, isActive, component: (() => null) as never });

describe('tab-store', () => {
  let store: TabStore;

  beforeEach(() => {
    store = createTabStore();
  });

  describe('state', () => {
    it('starts with the default group', () => {
      expect(store.get().groups).toEqual([
        { title: DEFAULT_MENU_GROUP, isActive: false, children: [] },
      ]);
    });

    it('is isolated between instances', () => {
      const a = createTabStore();
      const b = createTabStore();

      a.add(makeTab('only-in-a'));

      expect(a.get().groups[0].children).toHaveLength(1);
      expect(b.get().groups[0].children).toHaveLength(0);
    });
  });

  describe('groups', () => {
    it('getGroup returns the default group', () => {
      expect(store.getGroup()).toEqual({
        title: DEFAULT_MENU_GROUP,
        isActive: false,
        children: [],
      });
    });

    it('addGroup creates a group', () => {
      const group = store.addGroup('Checkout');

      expect(group).toEqual({
        title: 'Checkout',
        isActive: false,
        children: [],
      });
      expect(store.getGroup('Checkout')).toEqual(group);
    });

    it('addGroup returns the existing group without duplicating', () => {
      const first = store.addGroup('Checkout');
      const second = store.addGroup('Checkout');

      expect(second).toBe(first);
      expect(store.get().groups).toHaveLength(2);
    });

    it('removeGroup removes a group', () => {
      store.addGroup('Checkout');
      store.removeGroup('Checkout');

      expect(store.getGroup('Checkout')).toBeUndefined();
      expect(store.get().groups).toHaveLength(1);
    });

    it('removeGroup is a no-op when group is missing', () => {
      store.removeGroup('Missing');
      expect(store.get().groups).toHaveLength(1);
    });
  });

  describe('add', () => {
    it('adds a tab to the default group', () => {
      store.add(makeTab('Home'));

      expect(store.get().groups[0].children).toEqual([
        expect.objectContaining({ title: 'Home' }),
      ]);
    });

    it('creates the group on demand', () => {
      store.add(makeTab('Home'), 'Checkout');

      expect(store.getGroup('Checkout')?.children).toEqual([
        expect.objectContaining({ title: 'Home' }),
      ]);
    });

    it('adds a tab under a parent tab', () => {
      store.add(makeTab('Home'));
      store.add(makeTab('Sub'), DEFAULT_MENU_GROUP, 'Home');

      const home = store.find('Home');
      expect(home?.children).toEqual([
        expect.objectContaining({ title: 'Sub' }),
      ]);
    });

    it('throws when the parent does not exist', () => {
      expect(() =>
        store.add(makeTab('Sub'), DEFAULT_MENU_GROUP, 'Missing'),
      ).toThrow(/parent "Missing" not found/);
    });
  });

  describe('remove', () => {
    it('removes a tab', () => {
      store.add(makeTab('Home'));
      store.remove('Home');

      expect(store.find('Home')).toBeUndefined();
    });

    it('is a no-op when the tab is missing', () => {
      store.add(makeTab('Home'));
      store.remove('Missing');

      expect(store.get().groups[0].children).toHaveLength(1);
    });
  });

  describe('update', () => {
    it('applies a partial patch', () => {
      store.add(makeTab('Home'));
      store.update('Home', { isActive: true });

      expect(store.find('Home')?.isActive).toBe(true);
    });

    it('applies a functional patch', () => {
      store.add(makeTab('Home', false));
      store.update('Home', (tab) => ({ ...tab, isActive: !tab.isActive }));

      expect(store.find('Home')?.isActive).toBe(true);
    });
  });

  describe('toggle', () => {
    it('flips isActive', () => {
      store.add(makeTab('Home', false));

      store.toggle('Home');
      expect(store.find('Home')?.isActive).toBe(true);

      store.toggle('Home');
      expect(store.find('Home')?.isActive).toBe(false);
    });
  });

  describe('find', () => {
    it('finds a tab in a specific group', () => {
      store.add(makeTab('Home'), 'Checkout');

      expect(store.find('Home', 'Checkout')?.title).toBe('Home');
      expect(store.find('Home', DEFAULT_MENU_GROUP)).toBeUndefined();
    });
  });

  describe('subscriptions', () => {
    it('notifies subscribers on state changes', () => {
      let calls = 0;
      store.subscribe(() => {
        calls += 1;
      });

      store.add(makeTab('Home'));
      store.toggle('Home');
      store.removeGroup('Missing');

      expect(calls).toBe(2);
    });
  });
});