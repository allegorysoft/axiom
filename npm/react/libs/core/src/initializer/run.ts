import type { Platform } from '../models/application';
import {
  type ConfigureFn,
  type ApplicationInitializer,
  type InitializerContext,
  InitializerError,
} from '../models/initializer';

type TaskSelector = (
  initializer: ApplicationInitializer,
) => ConfigureFn | undefined;

export async function runInitializers(
  initializers: ApplicationInitializer[],
  context: InitializerContext,
): Promise<void> {
  const errors: InitializerError[] = [];

  const _initializers = initializers.filter((initializer) =>
    platformFilter(initializer, context.platform),
  );

  await run(_initializers, context, errors, ({ configure }) => configure);

  await run(
    _initializers,
    context,
    errors,
    ({ postConfigure }) => postConfigure,
  );

  if (errors.length > 0) {
    // throw new AggregateError(
    //   errors,
    //   'One or more application initializers failed.',
    // );
  }
}

async function run(
  initializers: ApplicationInitializer[],
  context: InitializerContext,
  errors: InitializerError[],
  selector: TaskSelector,
): Promise<void> {
  const tasks: Array<{ task: ConfigureFn }> = [];

  for (const initializer of initializers) {
    const task = selector(initializer);
    if (task) {
      tasks.push({ task });
    }
  }

  const results = await Promise.allSettled(
    tasks.map(({ task }) => task(context)),
  );

  results.forEach((result, index) => {
    if (result.status === 'rejected') {
      errors.push(new InitializerError('' + index, result.reason));
    }
  });
}

function platformFilter(
  initializer: ApplicationInitializer,
  current: Platform,
): boolean {
  const target = initializer.platform ?? 'client';
  return target === 'both' || target === current;
}
