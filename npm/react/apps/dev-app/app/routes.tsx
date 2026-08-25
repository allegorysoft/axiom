import {
  createBrowserRouter,
  isRouteErrorResponse,
  useRouteError,
} from 'react-router';
import { accountRoutes } from '@axiomframework/react-account';

export const routes = createBrowserRouter([
  {
    path: '/',
    ErrorBoundary,
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
        path: 'account',
        children: accountRoutes(),
      },
    ],
  },
]);

export function ErrorBoundary() {
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
