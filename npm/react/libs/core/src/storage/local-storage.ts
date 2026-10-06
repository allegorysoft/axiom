export function readLocalStorage<T>(
  key: string,
  fallback: T,
  merge?: (stored: Partial<T>, fallback: T) => T,
): T {
  if (typeof window === 'undefined') return fallback;

  try {
    const raw = window.localStorage.getItem(key);
    if (raw === null) return fallback;

    const parsed = JSON.parse(raw) as T;
    return merge ? merge(parsed as Partial<T>, fallback) : parsed;
  } catch {
    return fallback;
  }
}

export function writeLocalStorage<T>(key: string, value: T): void {
  if (typeof window === 'undefined') return;

  try {
    window.localStorage.setItem(key, JSON.stringify(value));
  } catch {}
}

export function removeLocalStorage(key: string): void {
  if (typeof window === 'undefined') return;

  try {
    window.localStorage.removeItem(key);
  } catch {}
}

export function onLocalStorageChange<T>(
  key: string,
  handler: (value: T | null) => void,
): () => void {
  if (typeof window === 'undefined') return () => {};

  const listener = (event: StorageEvent) => {
    if (event.key !== key) return;
    if (event.newValue === null) return handler(null);

    try {
      handler(JSON.parse(event.newValue) as T);
    } catch {}
  };

  window.addEventListener('storage', listener);
  return () => window.removeEventListener('storage', listener);
}
