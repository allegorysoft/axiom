type ErrorMessage = {
  key: string;
  args?: Record<string, unknown> | readonly unknown[];
};

export function tryParse(message: string): ErrorMessage {
  try {
    const parsed = JSON.parse(message);
    if (parsed && typeof parsed === 'object' && 'key' in parsed) {
      return {
        key: String(parsed.key),
        args: parsed.args ?? {},
      };
    }

    return { key: message };
  } catch {
    return { key: message };
  }
}
