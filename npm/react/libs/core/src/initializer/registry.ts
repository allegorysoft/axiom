import type { ApplicationInitializer } from '../models/initializer';

const registry = new Set<ApplicationInitializer>();

export function provideInitializers(
  ...initializers: ApplicationInitializer[]
): void {
  for (const initializer of initializers) {
    if (initializer.configure || initializer.postConfigure) {
      registry.add(initializer);
    }
  }
}

export function getInitializers(): ApplicationInitializer[] {
  return [...registry.values()];
}
