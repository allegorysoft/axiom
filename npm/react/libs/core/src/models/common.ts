export type Awaitable<T> = T | Promise<T>;

export interface Provider<T> {
  provide(): Awaitable<T>;
}

export interface AxiomStore<T> {
  get(): Readonly<T>;
  set(updater: (prev: T) => Partial<T>): void;
  subscribe(listener: VoidFunction): () => void;
}
