export type Platform = 'server' | 'client';

export function isPlatformServer(): boolean {
  return getPlatform() === 'server';
}

export function getPlatform(): Platform {
  if (import.meta.env.SSR) {
    return 'server';
  }

  return typeof window === 'undefined' ||
    typeof document === 'undefined' ||
    typeof document.createElement !== 'function'
    ? 'server'
    : 'client';
}
