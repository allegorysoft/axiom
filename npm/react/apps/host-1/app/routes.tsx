import { createBrowserRouter } from 'react-router';
import {
  AppLayout,
  ErrorBoundary,
} from '@axiomframework/react-theme/components';
import { module1Routes } from '@axiomframework/react-module-1';

export const routes = createBrowserRouter([
  {
    path: '/',
    // path: import.meta.env.BASE_URL,
    ErrorBoundary,
    Component: AppLayout,
    children: module1Routes(),
  },
]);
