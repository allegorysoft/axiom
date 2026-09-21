import './root.css';

import { createRoot } from 'react-dom/client';
import { initializeApplication } from '@axiomframework/react-core';

import { configureApplication, loadEnvironment } from './config';
import { App } from './app';

const container = document.getElementById('root');
if (!container) {
  throw new Error('Root element could not found');
}

await loadEnvironment();
configureApplication();

await initializeApplication();

const root = createRoot(container);
root.render(<App />);
