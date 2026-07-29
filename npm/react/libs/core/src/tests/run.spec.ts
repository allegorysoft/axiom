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
const setPlatform = (platform: Platform) =>
  getPlatformMock.mockReturnValue(platform);

const makeInitializer = (
  overrides: Partial<ApplicationInitializer> = {},
): ApplicationInitializer => ({
  configure: vi.fn<ConfigureFn>(),
  ...overrides,
});

type Method = 'configure' | 'postConfigure';

const makeFailingInitializer = (
  method: Method = 'configure',
): ApplicationInitializer =>
  makeInitializer({
    [method]: vi.fn<ConfigureFn>().mockRejectedValue(new Error('boom')),
  });

describe('runInitializers', () => {
  beforeEach(() => setPlatform('client'));
  afterEach(() => vi.clearAllMocks());

  it('resolves when there are no initializers', async () => {
    await expect(runInitializers([])).resolves.toBeUndefined();
  });

  describe('platform filtering', () => {
    const platformMatrix = [
      ['server', 'server', true],
      ['server', 'client', false],
      ['both', 'server', true],
      ['both', 'client', true],
      [undefined, 'client', true],
      [undefined, 'server', false],
    ] as const;

    it.each(platformMatrix)(
      'initializer platform %s on %s platform runs: %s',
      async (initPlatform, currentPlatform, shouldRun) => {
        setPlatform(currentPlatform);
        const initializer = makeInitializer({ platform: initPlatform });

        await runInitializers([initializer]);

        const expectedCalls = shouldRun ? 1 : 0;
        expect(initializer.configure).toHaveBeenCalledTimes(expectedCalls);
      },
    );
  });

  describe('execution phases and concurrency', () => {
    it('runs matching configure and postConfigure hooks with the current platform', async () => {
      setPlatform('server');
      const initializer = makeInitializer({
        platform: 'server',
        postConfigure: vi.fn<ConfigureFn>(),
      });

      await runInitializers([initializer]);

      const expectedArg = { platform: 'server' };
      expect(initializer.configure).toHaveBeenCalledExactlyOnceWith(
        expectedArg,
      );
      expect(initializer.postConfigure).toHaveBeenCalledExactlyOnceWith(
        expectedArg,
      );
    });

    it('runs all configure functions before any postConfigure functions', async () => {
      const calls: string[] = [];
      const track = (name: string) =>
        vi.fn<ConfigureFn>(async () => {
          calls.push(name);
        });

      const a = makeInitializer({
        configure: track('a.configure'),
        postConfigure: track('a.postConfigure'),
      });
      const b = makeInitializer({
        configure: track('b.configure'),
        postConfigure: track('b.postConfigure'),
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

      expect(order).toEqual([
        'slow:start',
        'fast:start',
        'fast:end',
        'slow:end',
      ]);
    });

    it('runs postConfigure even when configure is omitted', async () => {
      const initializer = makeInitializer({
        configure: undefined,
        postConfigure: vi.fn<ConfigureFn>(),
      });

      await runInitializers([initializer]);

      expect(initializer.configure).toBeUndefined();
      expect(initializer.postConfigure).toHaveBeenCalledOnce();
    });
  });

  describe('error handling', () => {
    it('does not run postConfigure if configure fails', async () => {
      const failing = makeFailingInitializer();
      const other = makeInitializer({ postConfigure: vi.fn<ConfigureFn>() });

      await expect(runInitializers([failing, other])).rejects.toBeInstanceOf(
        AggregateError,
      );
      expect(other.postConfigure).not.toHaveBeenCalled();
    });

    it('waits for sibling configure functions before rejecting', async () => {
      const failing = makeFailingInitializer();
      const sibling = makeInitializer();

      await expect(runInitializers([failing, sibling])).rejects.toThrow();
      expect(sibling.configure).toHaveBeenCalledOnce();
    });

    it.each([
      ['configure', makeFailingInitializer('configure')],
      ['postConfigure', makeFailingInitializer('postConfigure')],
    ])(
      'wraps failures from %s in InitializerError instances',
      async (_, initializer) => {
        await expect(runInitializers([initializer])).rejects.toMatchObject({
          errors: [expect.any(InitializerError)],
        });
      },
    );

    it('aggregates multiple configure failures', async () => {
      const first = makeFailingInitializer();
      const second = makeFailingInitializer();

      await expect(runInitializers([first, second])).rejects.toMatchObject({
        errors: [expect.any(InitializerError), expect.any(InitializerError)],
      });
    });
  });
});
