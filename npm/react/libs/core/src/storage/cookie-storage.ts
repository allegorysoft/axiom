export interface CookieOptions {
  path?: string;
  domain?: string;
  expires?: Date | number;
  maxAge?: number;
  secure?: boolean;
  sameSite?: 'strict' | 'lax' | 'none';
}

const DEFAULTS: CookieOptions = { path: '/', sameSite: 'lax' };

export function getCookie<T = string>(name: string): T | null {
  if (typeof document === 'undefined') return null;
  const target = encodeURIComponent(name);

  for (const part of document.cookie.split('; ')) {
    const eq = part.indexOf('=');
    const key = eq === -1 ? part : part.slice(0, eq);
    if (key === target) {
      const raw = decodeURIComponent(eq === -1 ? '' : part.slice(eq + 1));
      try {
        return JSON.parse(raw) as T;
      } catch {
        return raw as T;
      }
    }
  }
  return null;
}

export function setCookie<T>(
  name: string,
  value: T,
  options: CookieOptions = {},
): void {
  if (typeof document === 'undefined') return;

  const o = { ...DEFAULTS, ...options };
  let str = `${encodeURIComponent(name)}=${encodeURIComponent(JSON.stringify(value))}`;

  if (o.path) str += `; path=${o.path}`;
  if (o.domain) str += `; domain=${o.domain}`;

  if (typeof o.maxAge === 'number') {
    str += `; max-age=${Math.floor(o.maxAge)}`;
  } else if (o.expires != null) {
    const d =
      typeof o.expires === 'number'
        ? new Date(Date.now() + o.expires * 86400000)
        : o.expires;
    str += `; expires=${d.toUTCString()}`;
  }

  if (o.secure) str += '; secure';
  if (o.sameSite) str += `; samesite=${o.sameSite}`;

  document.cookie = str;
}

export function removeCookie(name: string, options: CookieOptions = {}): void {
  setCookie(name, '', { ...options, maxAge: 0 });
}
