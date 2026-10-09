import {
  createBrowserRouter,
  isRouteErrorResponse,
  useRouteError,
} from 'react-router';
import { accountRoutes } from '@axiomframework/react-account';
import { AppLayout } from '@axiomframework/react-theme/components';

export const routes = createBrowserRouter([
  {
    path: import.meta.env.BASE_URL,
    ErrorBoundary,
    children: [
      {
        path: '',
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

function ErrorBoundary() {
  const error = useRouteError();

  if (isRouteErrorResponse(error)) {
    return (
      <>
        <h1>
          {error.status} {error.statusText}
        </h1>
        <p>{error.data}</p>
      </>
    );
  }

  if (error instanceof Error) {
    return (
      <div>
        <h1>Error</h1>
        <p>{error.name}</p>
        <p>{error.message}</p>
        <pre>{error.stack}</pre>
      </div>
    );
  }

  return <h1>Unknown Error</h1>;
}
