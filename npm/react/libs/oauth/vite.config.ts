/// <reference types='vitest' />
import * as path from 'path';
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import dts from 'vite-plugin-dts';
import { viteStaticCopy } from 'vite-plugin-static-copy';

export default defineConfig(() => ({
  root: import.meta.dirname,
  resolve: { tsconfigPaths: true },
  cacheDir: '../../node_modules/.vite/libs/oauth',
  plugins: [
    react(),
    viteStaticCopy({ targets: [{ src: ['*.md', 'package.json'], dest: '.' }] }),
    dts({
      entryRoot: 'src',
      tsconfigPath: path.join(import.meta.dirname, 'tsconfig.lib.json'),
      pathsToAliases: false,
    }),
  ],
  build: {
    target: 'esnext',
    cssCodeSplit: true,
    outDir: '../../dist/libs/oauth',
    emptyOutDir: true,
    lib: {
      entry: {
        index: 'src/index',
      },
      name: 'oauth',
      formats: ['es' as const],
    },
    rolldownOptions: {
      external: ['react', 'openid-client', '@axiomframework/react-core'],
      output: {
        preserveModules: true,
        preserveModulesRoot: path.join(import.meta.dirname, 'src'),
      },
    },
  },
}));
