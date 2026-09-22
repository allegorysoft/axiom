/// <reference types='vitest' />
import * as path from 'path';
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import dts from 'vite-plugin-dts';
import tailwindcss from '@tailwindcss/vite';
import { viteStaticCopy } from 'vite-plugin-static-copy';

export default defineConfig(() => ({
  root: import.meta.dirname,
  resolve: { tsconfigPaths: true },
  cacheDir: '../../node_modules/.vite/libs/theme',
  plugins: [
    react(),
    viteStaticCopy({
      targets: [
        { src: '*.md', dest: '.' },
        {
          src: 'package.json',
          dest: '.',
          transform: (content) => {
            const manifest = JSON.parse(content.toString());
            manifest.exports['./index.css'] = './index.css';
            manifest.exports['./*.css'] = './*.css';
            return JSON.stringify(manifest, null, 2);
          },
        },
      ],
    }),
    dts({
      entryRoot: 'src',
      tsconfigPath: path.join(import.meta.dirname, 'tsconfig.lib.json'),
      pathsToAliases: false,
    }),
    tailwindcss(),
  ],
  build: {
    target: 'esnext',
    cssCodeSplit: true,
    outDir: '../../dist/libs/theme',
    emptyOutDir: true,
    lib: {
      entry: {
        index: 'src/index',
        'components/index': 'src/components/index',
        'index.css': 'src/styles/index.css',
      },
      name: 'theme',
      formats: ['es' as const],
    },
    rolldownOptions: {
      external: [
        /^react$/,
        /^react-dom(\/.*)?$/,
        /^react\/jsx-runtime$/,
        /^@floating-ui/,
        /^@base-ui/,
        'recharts',
        '@axiomframework/react-core',
        '@hugeicons/react',
        '@hugeicons/core-free-icons',
        'class-variance-authority',
        'cmdk',
        'cn',
        'shadcn',
      ],
      output: {
        preserveModules: true,
        preserveModulesRoot: path.join(import.meta.dirname, 'src'),
        assetFileNames: '[name].[ext]',
      },
    },
  },
  test: {
    name: 'theme',
    environment: 'jsdom',
    setupFiles: ['./test-setup.ts'],
    globals: false,
    include: ['src/**/*.{test,spec}.{ts,tsx}'],
    watch: false,
  },
}));
