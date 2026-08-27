import {
  createBrowserRouter,
  isRouteErrorResponse,
  Outlet,
  useRouteError,
} from 'react-router';
import { accountRoutes } from '@axiomframework/react-account';
import { Header } from './header';

export const routes = createBrowserRouter([
  {
    path: '/',
    ErrorBoundary,
    children: [
      {
        path: '',
        Component: Layout,
        children: [
          {
            index: true,
            lazy: () => import('./routes/home'),
          },
          {
            path: 'about',
            lazy: () => import('./routes/about'),
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

function Layout() {
  return (
    <>
      <Header />
      <main className="mx-3 my-2">
        <Outlet />
      </main>
    </>
  );
}

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
