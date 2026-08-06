/// <reference types='vitest' />
import { defineConfig } from 'vite';
import { reactRouter } from '@react-router/dev/vite';
import tailwindcss from '@tailwindcss/vite';
import path from 'path';
import { fileURLToPath } from 'url';

export default defineConfig(() => ({
  root: path.dirname(fileURLToPath(import.meta.url)),
  resolve: { tsconfigPaths: true },
  plugins: [!process.env.VITEST && reactRouter(), tailwindcss()],
  cacheDir: '../../node_modules/.vite/apps/dev-app',
  optimizeDeps: {
    include: ['react', 'react-dom', 'react-router'],
  },
}));
