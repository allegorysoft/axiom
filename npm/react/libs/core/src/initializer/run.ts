import {
  type ConfigureFn,
  type ApplicationInitializer,
  InitializerError,
} from '../models/initializer';
import { getPlatform } from '../utils/platform-utils';

type TaskSelector = (
  initializer: ApplicationInitializer,
) => ConfigureFn | undefined;

export async function runInitializers(
  initializers: ApplicationInitializer[],
): Promise<void> {
  const _initializers = initializers.filter(platformFilter);
  const errors = new Array<InitializerError>();

  await run(_initializers, errors, ({ configure }) => configure);

  if (errors.length === 0) {
    await run(_initializers, errors, ({ postConfigure }) => postConfigure);
  }

  if (errors.length > 0) {
    throw new AggregateError(
      errors,
      'One or more application initializers failed.',
    );
  }
}

async function run(
  initializers: ApplicationInitializer[],
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

  const context = { platform: getPlatform() };
  const results = await Promise.allSettled(
    tasks.map(async ({ task }) => task(context)),
  );

  results.forEach((result, index) => {
    if (result.status === 'rejected') {
      errors.push(new InitializerError('' + index, result.reason));
    }
  });
}

function platformFilter(initializer: ApplicationInitializer): boolean {
  const current = getPlatform();
  const target = initializer.platform ?? 'client';

  return target === 'both' || target === current;
}
