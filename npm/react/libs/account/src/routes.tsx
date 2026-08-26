import type { RouteObject } from 'react-router';

export function accountRoutes(): RouteObject[] {
  return [
    {
      lazy: () => import('./components/account-layout'),
      children: [
        {
          path: 'login',
          lazy: () => import('./components/login/login'),
        },
        {
          path: 'sign-up',
          lazy: () => import('./components/sign-up/sign-up'),
        },
      ],
    },
  ];
}
