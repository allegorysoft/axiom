/// <reference types='vitest' />
import * as path from 'path';
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import dts from 'vite-plugin-dts';
import { nxCopyAssetsPlugin } from '@nx/vite/plugins/nx-copy-assets.plugin';

export default defineConfig(() => ({
  root: import.meta.dirname,
  resolve: { tsconfigPaths: true },
  cacheDir: '../../node_modules/.vite/libs/core',
  plugins: [
    react(),
    nxCopyAssetsPlugin(['*.md']),
    dts({
      entryRoot: 'src',
      tsconfigPath: path.join(import.meta.dirname, 'tsconfig.lib.json'),
      pathsToAliases: false,
    }),
  ],
  build: {
    target: 'esnext',
    cssCodeSplit: true,
    outDir: '../../dist/libs/core',
    emptyOutDir: true,
    lib: {
      entry: {
        index: 'src/index',
      },
      name: 'core',
      formats: ['es' as const],
    },
    rolldownOptions: {
      external: [],
      output: {
        preserveModules: true,
      },
    },
  },
}));
