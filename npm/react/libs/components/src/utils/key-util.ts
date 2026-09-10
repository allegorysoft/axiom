export function hasKeyProperty(
  args: unknown,
): args is { key: string; [key: string]: unknown } {
  return (
    typeof args === 'object' &&
    args !== null &&
    !Array.isArray(args) &&
    'key' in args &&
    typeof (args as Record<string, unknown>).key === 'string'
  );
}
