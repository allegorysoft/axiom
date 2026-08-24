import { StrictMode } from 'react';
import { RouterProvider } from 'react-router/dom';
import { routes } from './routes';

export const App = () => {
  return (
    <StrictMode>
      <RouterProvider router={routes} />
    </StrictMode>
  );
};
