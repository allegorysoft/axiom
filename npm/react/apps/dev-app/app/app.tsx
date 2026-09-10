import { StrictMode } from 'react';
import { RouterProvider } from 'react-router/dom';
import { TooltipProvider } from '@axiomframework/react-theme/components';
import { routes } from './routes';

export const App = () => {
  return (
    <StrictMode>
      <TooltipProvider>
        <RouterProvider router={routes} />
      </TooltipProvider>
    </StrictMode>
  );
};
