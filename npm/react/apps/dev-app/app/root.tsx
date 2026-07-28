import './root.css';

import {
  Links,
  Meta,
  Outlet,
  Scripts,
  ScrollRestoration,
  type MetaFunction,
  type LinksFunction,
  LoaderFunctionArgs,
  ClientLoaderFunctionArgs,
  isRouteErrorResponse,
} from 'react-router';

import { bootstrapApplication } from '@axiomframework/react-core';

import type { Route } from '+/types/routes';
import { provideEnvironment } from './providers/environment.provider';

provideEnvironment();

export async function loader({}: LoaderFunctionArgs) {
  await bootstrapApplication();
}

export async function clientLoader({}: ClientLoaderFunctionArgs) {
  await bootstrapApplication();
}

clientLoader.hydrate = true as const;

export const meta: MetaFunction = () => [
  {
    title: 'New Nx React Router App',
  },
];

export const links: LinksFunction = () => [
  { rel: 'preconnect', href: 'https://fonts.googleapis.com' },
  {
    rel: 'preconnect',
    href: 'https://fonts.gstatic.com',
    crossOrigin: 'anonymous',
  },
  {
    rel: 'stylesheet',
    href: 'https://fonts.googleapis.com/css2?family=Inter:ital,opsz,wght@0,14..32,100..900;1,14..32,100..900&display=swap',
  },
];

export function Layout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en" dir="ltr" className="dark">
      <head>
        <meta charSet="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <Meta />
        <Links />
      </head>
      <body>
        {children}
        <ScrollRestoration />
        <Scripts />
      </body>
    </html>
  );
}

export function ErrorBoundary({ error }: Route.ErrorBoundaryProps) {
  if (isRouteErrorResponse(error)) {
    return (
      <>
        <h1>
          {error.status} {error.statusText}
        </h1>
        <p>{error.data}</p>
      </>
    );
  } else if (error instanceof Error || error instanceof AggregateError) {
    return (
      <div>
        <h1>Error</h1>
        <p>{error.name}</p>
        <p>{error.message}</p>
        {/*<p>The stack trace is:</p>
        <pre>{error.stack}</pre>*/}
      </div>
    );
  } else {
    return <h1>Unknown Error</h1>;
  }
}

export default function App() {
  return <Outlet />;
}
