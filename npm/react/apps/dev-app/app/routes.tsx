import { createBrowserRouter } from 'react-router';
import { accountRoutes } from '@axiomframework/react-account';
import {
  AppLayout,
  ErrorBoundary,
} from '@axiomframework/react-theme/components';

export const routes = createBrowserRouter([
  {
    path: '',
    ErrorBoundary,
    children: [
      {
        Component: AppLayout,
        children: [
          {
            index: true,
            lazy: () => import('./routes/home'),
          },
          {
            path: 'identity-management',
            children: [
              {
                path: 'users',
                handle: { pageTitleInToolbar: true },
                lazy: () => import('./users/users'),
              },
              {
                path: 'roles',
                handle: { pageTitleInToolbar: true },
                lazy: () => import('./routes/roles'),
              },
            ],
          },
          {
            path: 'tenant-management',
            children: [
              {
                path: 'tenants',
                handle: { pageTitleInToolbar: true },
                lazy: () => import('./routes/tenants'),
              },
            ],
          },
          {
            path: 'panels',
            children: [
              {
                path: 'users',
                children: [
                  {
                    path: 'roles',
                    children: [
                      {
                        path: 'viewer',
                        lazy: () => import('./routes/roles-viewer'),
                      },
                    ],
                  },
                ],
              },
            ],
          },
          {
            path: 'panels',
            children: [
              {
                path: 'billing',
                children: [
                  {
                    path: 'subscriptions',
                    children: [
                      {
                        path: 'wallets',
                        children: [
                          {
                            path: 'paypal',
                            lazy: () => import('./routes/wallets-paypal'),
                          },
                        ],
                      },
                    ],
                  },
                ],
              },
            ],
          },
        ],
      },
      {
        path: 'account',
        children: accountRoutes(),
      },
    ],
  },
]);
