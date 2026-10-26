import { beforeEach, describe, expect, it } from 'vitest';
import { DEFAULT_MENU_GROUP } from '../menu/menu';
import type { Tab, TabStore } from '../menu/tab/tab';
import { createTabStore } from '../menu/tab/tab-store';

const makeTab = (title: string, isActive = false): Tab => ({
  title,
  isActive,
  component: (() => null) as never,
});

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

    it('getGroup returns undefined for an unknown group', () => {
      expect(store.getGroup('Missing')).toBeUndefined();
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

    it('addGroup applies options', () => {
      const child = makeTab('Cart');
      const group = store.addGroup('Checkout', {
        isActive: true,
        children: [child],
      });

      expect(group).toEqual({
        title: 'Checkout',
        isActive: true,
        children: [child],
      });
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
      const before = store.get();
      store.removeGroup('Missing');

      expect(store.get()).toBe(before);
    });
  });

  describe('patchGroup', () => {
    it('patches an existing group and returns the patched group', () => {
      store.addGroup('Checkout');
      const patched = store.patchGroup('Checkout', { isActive: true });

      expect(patched).toEqual({
        title: 'Checkout',
        isActive: true,
        children: [],
      });
      expect(store.getGroup('Checkout')).toEqual(patched);
    });

    it('patches the default group', () => {
      const patched = store.patchGroup(DEFAULT_MENU_GROUP, {
        isActive: true,
      });

      expect(patched.isActive).toBe(true);
      expect(store.getGroup()?.isActive).toBe(true);
    });

    it('preserves existing children when patching', () => {
      store.add(makeTab('Home'));
      store.patchGroup(DEFAULT_MENU_GROUP, { isActive: true });

      const group = store.getGroup();
      expect(group?.isActive).toBe(true);
      expect(group?.children).toHaveLength(1);
      expect(group?.children[0].title).toBe('Home');
    });

    it('does not mutate the previous group object', () => {
      const original = store.addGroup('Checkout');
      store.patchGroup('Checkout', { isActive: true });

      expect(original.isActive).toBe(false);
      expect(store.getGroup('Checkout')?.isActive).toBe(true);
    });

    it('returns the same reference that is stored in the state', () => {
      store.addGroup('Checkout');
      const patched = store.patchGroup('Checkout', { isActive: true });

      expect(store.getGroup('Checkout')).toBe(patched);
    });

    it('does not touch other groups', () => {
      store.addGroup('Checkout');
      const untouched = store.getGroup(DEFAULT_MENU_GROUP);

      store.patchGroup('Checkout', { isActive: true });

      expect(store.getGroup(DEFAULT_MENU_GROUP)).toBe(untouched);
    });

    it('throws when the group does not exist', () => {
      expect(() => store.patchGroup('Missing', { isActive: true })).toThrow(
        /group "Missing" not found/,
      );
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

    it('removes a nested tab', () => {
      store.add(makeTab('Home'));
      store.add(makeTab('Sub'), DEFAULT_MENU_GROUP, 'Home');
      store.remove('Sub');

      expect(store.find('Home')?.children).toEqual([]);
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

    it('updates a nested tab', () => {
      store.add(makeTab('Home'));
      store.add(makeTab('Sub', false), DEFAULT_MENU_GROUP, 'Home');
      store.update('Sub', { isActive: true });

      expect(store.find('Sub')?.isActive).toBe(true);
    });

    it('is a no-op when the tab is missing', () => {
      const before = store.get();
      store.update('Missing', { isActive: true });

      expect(store.get()).toBe(before);
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

    it('toggles a nested tab', () => {
      store.add(makeTab('Home'));
      store.add(makeTab('Sub', false), DEFAULT_MENU_GROUP, 'Home');

      store.toggle('Sub');
      expect(store.find('Sub')?.isActive).toBe(true);
    });

    it('is a no-op when the tab is missing', () => {
      const before = store.get();
      store.toggle('Missing');

      expect(store.get()).toBe(before);
    });
  });

  describe('find', () => {
    it('finds a tab in a specific group', () => {
      store.add(makeTab('Home'), 'Checkout');

      expect(store.find('Home', 'Checkout')?.title).toBe('Home');
      expect(store.find('Home', DEFAULT_MENU_GROUP)).toBeUndefined();
    });

    it('returns undefined for an unknown group', () => {
      expect(store.find('Home', 'Missing')).toBeUndefined();
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

    it('stops notifying after unsubscribe', () => {
      let calls = 0;
      const unsubscribe = store.subscribe(() => {
        calls += 1;
      });

      store.add(makeTab('Home'));
      unsubscribe();
      store.add(makeTab('About'));

      expect(calls).toBe(1);
    });
  });
});