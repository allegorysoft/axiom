import type { RouteObject } from 'react-router';

export function accountRoutes(): RouteObject[] {
  return [
    {
      path: 'login',
      lazy: () => import('./components/login/login'),
    },
  ];
}
