import type { Initializer } from './types';

const registry = new Map<string, Initializer>();

export function provideInitializer(...initializers: Initializer[]): void {
  for (const initializer of initializers) {
    registry.set(initializer.name, initializer);
  }
}

export function getInitializers(): Initializer[] {
  return [...registry.values()];
}

export function clearInitializers(): void {
  registry.clear();
}
