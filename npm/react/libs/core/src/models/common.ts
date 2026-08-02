export type Awaitable<T> = T | Promise<T>;

export interface Provider<T> {
  provide(): Awaitable<T>;
}
