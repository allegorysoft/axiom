import {
  InitializerContext,
  InitializerError,
  type Initializer,
  type Side,
} from './types';

const currentSide = (): Side =>
  typeof document === 'undefined' ? 'server' : 'client';

export async function runInitializers(
  initializers: Initializer[],
  side: Side = currentSide(),
): Promise<string[]> {
  const completed: string[] = [];

  for (const initializer of initializers) {
    const target = initializer.side ?? 'both';
    if (target !== 'both' && target !== side) continue;

    try {
      await runOne(initializer, side);
      completed.push(initializer.name);
    } catch (error) {
      const wrapped = new InitializerError(initializer.name, error);
      if (!initializer.optional) {
        throw wrapped;
      }
      console.warn(wrapped);
    }
  }

  return completed;
}

function runOne(initializer: Initializer, side: Side): Promise<void> {
  const controller = new AbortController();
  const context: InitializerContext = { side, signal: controller.signal };
  const { timeout } = initializer;

  if (timeout === undefined || timeout <= 0) {
    return Promise.resolve(initializer.run(context));
  }

  return new Promise<void>((resolve, reject) => {
    const timer: ReturnType<typeof setTimeout> = setTimeout(() => {
      const error = new Error(`timed out after ${timeout}ms`);
      controller.abort(error);
      reject(error);
    }, timeout);

    const settle = (finish: () => void) => {
      clearTimeout(timer);
      finish();
    };

    Promise.resolve(initializer.run(context)).then(
      () => settle(resolve),
      (error: unknown) => settle(() => reject(error)),
    );
  });
}
