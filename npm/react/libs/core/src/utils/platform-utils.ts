import type { Platform } from '../models/application';

export function isPlatformServer(): boolean {
  return getPlatform() === 'server';
}

export function getPlatform(): Platform {
  if (typeof import.meta !== 'undefined' && import.meta.env.SSR) {
    return 'server';
  }

  return typeof window === 'undefined' ||
    typeof document === 'undefined' ||
    typeof document.createElement !== 'function'
    ? 'server'
    : 'client';
}
