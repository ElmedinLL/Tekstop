import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'
import { visualizer } from 'rollup-plugin-visualizer'

// P234: ANALYZE — run `vite build --mode analyze` (see npm script `analyze`) to emit `dist/stats.html`.
// 502 Bad Gateway on /api/* usually means nothing is listening on this URL — start TechVault.API (`dotnet run`) first.
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const apiTarget = env.VITE_PROXY_TARGET?.trim() || 'http://localhost:5092'

  return {
    plugins: [
      react(),
      ...(mode === 'analyze'
        ? [
            visualizer({
              filename: 'dist/stats.html',
              gzipSize: true,
              brotliSize: true,
              template: 'treemap',
              open: false,
            }),
          ]
        : []),
    ],
    server: {
      proxy: {
        '/api': {
          target: apiTarget,
          changeOrigin: true,
          secure: false,
        },
        '/images': {
          target: apiTarget,
          changeOrigin: true,
          secure: false,
        },
      },
    },
  }
})
