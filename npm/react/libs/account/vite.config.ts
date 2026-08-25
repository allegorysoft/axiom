/// <reference types='vitest' />
import * as path from 'path';
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import dts from 'vite-plugin-dts';
import { viteStaticCopy } from 'vite-plugin-static-copy';

export default defineConfig(() => ({
  root: import.meta.dirname,
  resolve: { tsconfigPaths: true },
  cacheDir: '../../node_modules/.vite/libs/account',
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
    outDir: '../../dist/libs/account',
    emptyOutDir: true,
    lib: {
      entry: {
        index: 'src/index',
      },
      name: 'account',
      formats: ['es' as const],
    },
    rolldownOptions: {
      external: [
        /^react$/,
        /^react-dom(\/.*)?$/,
        /^react\/jsx-runtime$/,
        /^@base-ui/,
        'react-router',
        'lucide-react',
        'react-hook-form',
        /^@hookform/,
        /^@axiomframework/,
        'zod',
        'clsx',
        'class-variance-authority',
        'tailwind-merge',
      ],
      output: {
        preserveModules: true,
        preserveModulesRoot: path.join(import.meta.dirname, 'src'),
      },
    },
  },
}));
