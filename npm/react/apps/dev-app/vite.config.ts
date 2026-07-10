/// <reference types='vitest' />
import { defineConfig } from 'vite';
import { reactRouter } from '@react-router/dev/vite';
import tailwindcss from '@tailwindcss/vite';

export default defineConfig(() => ({
  resolve: { tsconfigPaths: true },
  plugins: [!process.env.VITEST && reactRouter(), tailwindcss()],
  cacheDir: '../../node_modules/.vite/apps/dev-app',
  optimizeDeps: {
    include: ['react', 'react-dom', 'react-router'],
  },
}));
