import { Badge } from './ui/badge';

export function HttpStatusBadge({ code }: { code: number }) {
  const variant =
    code >= 100 && code < 200
      ? 'info'
      : code >= 200 && code < 300
        ? 'success'
        : code >= 300 && code < 400
          ? 'warning'
          : code >= 400 && code < 600
            ? 'destructive'
            : 'secondary';
  return <Badge variant={variant}>{code}</Badge>;
}
