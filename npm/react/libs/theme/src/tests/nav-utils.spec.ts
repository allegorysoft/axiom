import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';

import type { Nav, NavGroup } from '@axiomframework/react-core';
import { isBranchActive, findStack } from '../components/app-sidebar/nav-utils';

const nav = (n: Partial<Nav>): Nav => n as Nav;
const group = (children: Nav[]): NavGroup => ({ children } as NavGroup);

describe('isBranchActive', () => {
  beforeEach(() => {
    vi.stubEnv('BASE_URL', '/');
  });

  afterEach(() => {
    vi.unstubAllEnvs();
  });

  it('is active when the item url matches the pathname', () => {
    const item = nav({ url: '/dashboard' });

    expect(isBranchActive(item, '/dashboard')).toBe(true);
  });

  it('is not active when the item url does not match', () => {
    const item = nav({ url: '/dashboard' });

    expect(isBranchActive(item, '/settings')).toBe(false);
  });

  it('is active when a descendant matches', () => {
    const item = nav({
      children: [nav({ url: '/a' }), nav({ url: '/b' })],
    });

    expect(isBranchActive(item, '/b')).toBe(true);
  });

  it('ignores trailing slashes', () => {
    const item = nav({ url: '/dashboard' });

    expect(isBranchActive(item, '/dashboard/')).toBe(true);
  });

  it('is active when a pathname only differs by the BASE_URL prefix', () => {
    vi.stubEnv('BASE_URL', '/my-app/');

    const item = nav({ url: '/dashboard' });

    expect(isBranchActive(item, '/my-app/dashboard')).toBe(true);
  });

  it('is active when the group name equals the module name', () => {
    vi.stubEnv('BASE_URL', '/tenant-management/');

    const item = nav({ url: '/tenant-management/tenants' });

    expect(isBranchActive(item, '/tenant-management/tenants')).toBe(true);
  });
});

describe('findStack', () => {
  beforeEach(() => {
    vi.stubEnv('BASE_URL', '/');
  });

  afterEach(() => {
    vi.unstubAllEnvs();
  });

  it('returns an empty stack when nothing matches', () => {
    const groups = [group([nav({ url: '/a' })])];

    expect(findStack(groups, '/missing')).toEqual([]);
  });

  it('returns the switch-panel that contains the matching path', () => {
    const panel = nav({
      mode: 'switch-panel',
      children: [nav({ url: '/tenants' })],
    });

    const stack = findStack([group([panel])], '/tenants');

    expect(stack).toEqual([panel]);
  });

  it('returns the full chain of nested switch-panels', () => {
    const inner = nav({
      mode: 'switch-panel',
      children: [nav({ url: '/target' })],
    });
    const outer = nav({ mode: 'switch-panel', children: [inner] });

    const stack = findStack([group([outer])], '/target');

    expect(stack).toEqual([outer, inner]);
  });

  it('finds a match inside a switch-panel group', () => {
    const panel = nav({
      mode: 'switch-panel',
      groups: [group([nav({ url: '/inside' })])],
    });

    const stack = findStack([group([panel])], '/inside');

    expect(stack).toEqual([panel]);
  });

  it('matches when the pathname only differs by the BASE_URL prefix', () => {
    vi.stubEnv('BASE_URL', '/my-app/');

    const panel = nav({
      mode: 'switch-panel',
      children: [nav({ url: '/tenants' })],
    });

    const stack = findStack([group([panel])], '/my-app/tenants');

    expect(stack).toEqual([panel]);
  });
});