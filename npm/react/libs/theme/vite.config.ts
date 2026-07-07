/// <reference types='vitest' />
import * as path from 'path';
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import dts from 'vite-plugin-dts';
import tailwindcss from '@tailwindcss/vite';
import { nxCopyAssetsPlugin } from '@nx/vite/plugins/nx-copy-assets.plugin';

export default defineConfig(() => ({
  root: import.meta.dirname,
  resolve: { tsconfigPaths: true },
  cacheDir: '../../node_modules/.vite/libs/theme',
  plugins: [
    react(),
    nxCopyAssetsPlugin(['*.md', 'src/styles/**/*.css']),
    dts({
      entryRoot: 'src',
      tsconfigPath: path.join(import.meta.dirname, 'tsconfig.lib.json'),
      pathsToAliases: false,
    }),
    tailwindcss(),
  ],
  build: {
    outDir: '../../dist/libs/theme',
    emptyOutDir: true,
    lib: {
      entry: {
        index: 'src/index',
        'components/index': 'src/components/index',
      },
      name: 'theme',
      formats: ['es' as const],
    },
    rolldownOptions: {
      external: [
        /^react/,
        'radix-ui',
        'clsx',
        'class-variance-authority',
        'tailwind-merge',
        'shadcn',
      ],
      output: {
        preserveModules: true,
      },
    },
  },
}));
