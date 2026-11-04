import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import type { WorkspaceTab } from '../menu/workspace-tab/workspace-tab';
import { workspaceTabStore as store } from '../menu/workspace-tab/workspace-tab-store';

const tab = (
  url: string,
  options: Partial<WorkspaceTab> = {},
): WorkspaceTab => ({
  url,
  title: url,
  active: false,
  ...options,
});

beforeEach(() => store.set(() => ({ tabs: [] })));
afterEach(() => store.set(() => ({ tabs: [] })));

describe('workspace-tab-store', () => {
  it('starts empty and activates the first added tab', () => {
    expect(store.get().tabs).toEqual([]);
    store.add(tab('/'));
    expect(store.get().tabs).toEqual([tab('/', { active: true })]);
    expect(store.find('/missing')).toBeUndefined();
  });

  it('preserves the active tab unless the added tab is active', () => {
    store.add(tab('/one'));
    store.add(tab('/two'));
    expect(store.find('/one')?.active).toBe(true);
    expect(store.find('/two')?.active).toBe(false);

    store.add(tab('/three', { active: true }));
    expect(store.get().tabs.map((item) => item.active)).toEqual([
      false,
      false,
      true,
    ]);
  });

  it('finds URLs and prevents duplicates regardless of case', () => {
    store.add(tab('/Home'));
    store.add(tab('/other', { active: true }));
    const before = store.get();
    store.add(tab('/HOME', { title: 'Duplicate', active: true }));
    expect(store.get()).toBe(before);
    expect(store.find('/home')).toEqual(tab('/Home', { active: false }));
  });

  it('focuses the existing empty tab instead of adding another', () => {
    store.add(tab('/empty/one', { empty: true }));
    store.add(tab('/other', { active: true }));
    store.add(tab('/empty/two', { empty: true, active: true }));
    expect(store.get().tabs).toEqual([
      tab('/empty/one', { empty: true, active: true }),
      tab('/other'),
    ]);
  });

  it('replaces an ephemeral tab in place and preserves permanent tabs', () => {
    store.add(tab('/permanent'));
    store.add(tab('/preview', { ephemeral: true, active: true }));
    store.add(tab('/last'));
    store.add(tab('/replacement', { ephemeral: true, active: true }));
    expect(store.get().tabs).toEqual([
      tab('/permanent'),
      tab('/replacement', { ephemeral: true, active: true }),
      tab('/last'),
    ]);
  });

  it('prefers the active empty tab over an existing ephemeral tab', () => {
    store.add(tab('/preview', { ephemeral: true }));
    store.add(tab('/empty', { empty: true, active: true }));
    store.add(tab('/next', { ephemeral: true, active: true }));
    expect(store.get().tabs).toEqual([
      tab('/preview', { ephemeral: true }),
      tab('/next', { ephemeral: true, active: true }),
    ]);
  });

  it('does not replace an inactive empty tab', () => {
    store.add(tab('/empty', { empty: true }));
    store.add(tab('/permanent', { active: true }));
    store.add(tab('/preview', { ephemeral: true, active: true }));
    expect(store.get().tabs.map((item) => item.url)).toEqual([
      '/empty',
      '/permanent',
      '/preview',
    ]);
  });

  it('patches fields case-insensitively without mutating previous state', () => {
    store.add(tab('/preview', { ephemeral: true }));
    store.add(tab('/other'));
    const before = store.get().tabs;
    store.patch('/PREVIEW', {
      url: '/kept',
      title: 'Kept',
      icon: 'icon',
      ephemeral: false,
    });
    expect(store.get().tabs).toEqual([
      tab('/kept', {
        title: 'Kept',
        icon: 'icon',
        active: true,
        ephemeral: false,
      }),
      tab('/other'),
    ]);
    expect(before[0]).toEqual(
      tab('/preview', { ephemeral: true, active: true }),
    );
    store.add(tab('/new-preview', { ephemeral: true, active: true }));
    expect(store.find('/kept')).toBeDefined();
    expect(store.get().tabs).toHaveLength(3);
  });

  it('activates an existing tab case-insensitively and ignores unknown URLs', () => {
    store.add(tab('/one'));
    store.add(tab('/two'));
    store.setActive('/TWO');
    expect(store.get().tabs.map((item) => item.active)).toEqual([false, true]);
    const before = store.get();
    store.setActive('/missing');
    expect(store.get()).toBe(before);
    store.patch('/missing', { title: 'Missing' });
    store.remove('/missing');
    expect(store.get().tabs).toEqual(before.tabs);
  });

  it.each([
    ['/one', '/two'],
    ['/two', '/three'],
    ['/three', '/two'],
  ])('removing active %s activates %s', (removed, active) => {
    ['/one', '/two', '/three'].forEach((url) => store.add(tab(url)));
    store.setActive(removed);
    store.remove(removed.toUpperCase());
    expect(store.find(removed)).toBeUndefined();
    expect(
      store
        .get()
        .tabs.filter((item) => item.active)
        .map((item) => item.url),
    ).toEqual([active]);
  });

  it('preserves focus when removing an inactive tab and allows removing the last tab', () => {
    store.add(tab('/one'));
    store.add(tab('/two'));
    store.remove('/TWO');
    expect(store.get().tabs).toEqual([tab('/one', { active: true })]);
    store.remove('/one');
    expect(store.get().tabs).toEqual([]);
  });

  it('notifies subscribers and supports unsubscribing', () => {
    const listener = vi.fn();
    const unsubscribe = store.subscribe(listener);
    try {
      store.add(tab('/one'));
      store.patch('/one', { title: 'Updated' });
      store.add(tab('/two'));
      store.setActive('/two');
      store.remove('/one');
      expect(listener).toHaveBeenCalledTimes(5);
      unsubscribe();
      store.remove('/two');
      expect(listener).toHaveBeenCalledTimes(5);
    } finally {
      unsubscribe();
    }
  });
});
