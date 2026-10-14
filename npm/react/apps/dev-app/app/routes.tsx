import { createBrowserRouter } from 'react-router';
import { accountRoutes } from '@axiomframework/react-account';
import {
  AppLayout,
  ErrorBoundary,
} from '@axiomframework/react-theme/components';

export const routes = createBrowserRouter([
  {
    path: import.meta.env.BASE_URL,
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
            path: 'about',
            lazy: () => import('./routes/about'),
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
