import { StrictMode } from 'react';
import { RouterProvider } from 'react-router/dom';
import { ThemeProvider } from '@axiomframework/react-core';
import { TooltipProvider } from '@axiomframework/react-theme/components';
import { routes } from './routes';

export const App = () => {
  return (
    <StrictMode>
      <ThemeProvider>
        <TooltipProvider>
          <RouterProvider router={routes} />
        </TooltipProvider>
      </ThemeProvider>
    </StrictMode>
  );
};
