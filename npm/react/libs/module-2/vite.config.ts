/// <reference types='vitest' />
import * as path from 'path';
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import dts from 'vite-plugin-dts';
import { viteStaticCopy } from 'vite-plugin-static-copy';

export default defineConfig(() => ({
  root: import.meta.dirname,
  resolve: { tsconfigPaths: true },
  cacheDir: '../../node_modules/.vite/libs/module-2',
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
    outDir: '../../dist/libs/module-2',
    emptyOutDir: true,
    lib: {
      entry: {
        index: 'src/index',
      },
      name: 'module-2',
      formats: ['es' as const],
    },
    rolldownOptions: {
      external: [
        /^react$/,
        /^react\/jsx-runtime$/,
        /^@axiomframework/,
        'react-router',
        '@hugeicons/react',
        '@hugeicons/core-free-icons',
      ],
      output: {
        preserveModules: true,
        preserveModulesRoot: path.join(import.meta.dirname, 'src'),
      },
    },
  },
}));
