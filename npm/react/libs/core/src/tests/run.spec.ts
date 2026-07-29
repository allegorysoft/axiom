import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import {
  type Platform,
  type ConfigureFn,
  type ApplicationInitializer,
  InitializerError,
} from '../models/application';
import { getPlatform } from '../utils/platform-utils';
import { runInitializers } from '../initializer/run';

vi.mock('../utils/platform-utils', () => ({
  getPlatform: vi.fn(),
}));

const getPlatformMock = vi.mocked(getPlatform);

const setPlatform = (platform: Platform) => {
  getPlatformMock.mockReturnValue(platform);
};

const makeInitializer = (
  overrides: Partial<ApplicationInitializer> = {},
): ApplicationInitializer => ({
  configure: vi.fn<ConfigureFn>(),
  ...overrides,
});

beforeEach(() => {
  setPlatform('client');
});

afterEach(() => {
  vi.clearAllMocks();
});

describe('runInitializers', () => {
  it('resolves when there are no initializers', async () => {
    await expect(runInitializers([])).resolves.toBeUndefined();
  });

  it('runs configure for every matching initializer', async () => {
    const a = makeInitializer();
    const b = makeInitializer();

    await runInitializers([a, b]);

    expect(a.configure).toHaveBeenCalledOnce();
    expect(b.configure).toHaveBeenCalledOnce();
  });

  it('passes the current platform to configure', async () => {
    setPlatform('server');

    const initializer = makeInitializer({ platform: 'server' });

    await runInitializers([initializer]);

    expect(initializer.configure).toHaveBeenCalledExactlyOnceWith({
      platform: 'server',
    });
  });

  it('passes the current platform to postConfigure', async () => {
    setPlatform('server');

    const initializer = makeInitializer({
      platform: 'server',
      postConfigure: vi.fn<ConfigureFn>(),
    });

    await runInitializers([initializer]);

    expect(initializer.postConfigure).toHaveBeenCalledExactlyOnceWith({
      platform: 'server',
    });
  });

  it('skips initializers whose platform does not match', async () => {
    const initializer = makeInitializer({ platform: 'server' });

    await runInitializers([initializer]);

    expect(initializer.configure).not.toHaveBeenCalled();
  });

  it('runs "both" platform initializers on any platform', async () => {
    setPlatform('server');

    const initializer = makeInitializer({ platform: 'both' });

    await runInitializers([initializer]);

    expect(initializer.configure).toHaveBeenCalledOnce();
  });

  it('treats an unspecified platform as client', async () => {
    const initializer = makeInitializer({ platform: undefined });

    await runInitializers([initializer]);

    expect(initializer.configure).toHaveBeenCalledOnce();
  });

  it('runs all configure functions before any postConfigure functions', async () => {
    const calls: string[] = [];

    const a = makeInitializer({
      configure: vi.fn<ConfigureFn>(async () => {
        calls.push('a.configure');
      }),
      postConfigure: vi.fn<ConfigureFn>(async () => {
        calls.push('a.postConfigure');
      }),
    });

    const b = makeInitializer({
      configure: vi.fn<ConfigureFn>(async () => {
        calls.push('b.configure');
      }),
      postConfigure: vi.fn<ConfigureFn>(async () => {
        calls.push('b.postConfigure');
      }),
    });

    await runInitializers([a, b]);

    expect(calls).toEqual([
      'a.configure',
      'b.configure',
      'a.postConfigure',
      'b.postConfigure',
    ]);
  });

  it('runs configure functions concurrently within the phase', async () => {
    const order: string[] = [];

    const slow = makeInitializer({
      configure: vi.fn<ConfigureFn>(async () => {
        order.push('slow:start');
        await new Promise((resolve) => setTimeout(resolve, 10));
        order.push('slow:end');
      }),
    });

    const fast = makeInitializer({
      configure: vi.fn<ConfigureFn>(async () => {
        order.push('fast:start');
        order.push('fast:end');
      }),
    });

    await runInitializers([slow, fast]);

    expect(order).toEqual(['slow:start', 'fast:start', 'fast:end', 'slow:end']);
  });

  it('runs postConfigure even when configure is omitted', async () => {
    const initializer = makeInitializer({
      configure: undefined,
      postConfigure: vi.fn<ConfigureFn>(),
    });

    await runInitializers([initializer]);

    expect(initializer.configure).eq(undefined);
    expect(initializer.postConfigure).toHaveBeenCalledOnce();
  });

  it('does not run postConfigure if configure fails', async () => {
    const failing = makeInitializer({
      configure: vi.fn<ConfigureFn>(async () => {
        throw new Error('boom');
      }),
    });

    const other = makeInitializer({
      postConfigure: vi.fn<ConfigureFn>(),
    });

    await expect(runInitializers([failing, other])).rejects.toBeInstanceOf(
      AggregateError,
    );

    expect(other.postConfigure).not.toHaveBeenCalled();
  });

  it('waits for sibling configure functions before rejecting', async () => {
    const failing = makeInitializer({
      configure: vi.fn<ConfigureFn>(async () => {
        throw new Error('boom');
      }),
    });

    const sibling = makeInitializer();

    await expect(runInitializers([failing, sibling])).rejects.toThrow();

    expect(sibling.configure).toHaveBeenCalledOnce();
  });

  it('wraps failures in InitializerError instances', async () => {
    const failing = makeInitializer({
      configure: vi.fn<ConfigureFn>(async () => {
        throw new Error('boom');
      }),
    });

    await expect(runInitializers([failing])).rejects.toMatchObject({
      errors: [expect.any(InitializerError)],
    });
  });

  it('collects postConfigure failures', async () => {
    const initializer = makeInitializer({
      postConfigure: vi.fn<ConfigureFn>(async () => {
        throw new Error('post failed');
      }),
    });

    await expect(runInitializers([initializer])).rejects.toMatchObject({
      errors: [expect.any(InitializerError)],
    });
  });

  it('aggregates multiple configure failures', async () => {
    const first = makeInitializer({
      configure: vi.fn<ConfigureFn>(async () => {
        throw new Error('first');
      }),
    });

    const second = makeInitializer({
      configure: vi.fn<ConfigureFn>(async () => {
        throw new Error('second');
      }),
    });

    await expect(runInitializers([first, second])).rejects.toMatchObject({
      errors: [expect.any(InitializerError), expect.any(InitializerError)],
    });
  });

  it('ignores failures from filtered-out initializers', async () => {
    const initializer = makeInitializer({
      platform: 'server',
      configure: vi.fn<ConfigureFn>(async () => {
        throw new Error('should never run');
      }),
    });

    await expect(runInitializers([initializer])).resolves.toBeUndefined();

    expect(initializer.configure).not.toHaveBeenCalled();
  });
});
