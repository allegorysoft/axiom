/// <reference types='vitest' />
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';
import path from 'path';
import { fileURLToPath } from 'url';

export default defineConfig(() => ({
  base: '/module-1',
  root: path.dirname(fileURLToPath(import.meta.url)),
  resolve: { tsconfigPaths: true },
  plugins: [react(), tailwindcss()],
  optimizeDeps: {
    include: ['react', 'react-dom', 'react-router'],
  },
  build: {
    outDir: '../../dist/apps/host-1',
  },
}));
