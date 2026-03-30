import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

// 502 Bad Gateway on /api/* usually means nothing is listening on this URL — start TechVault.API (`dotnet run`) first.
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const apiTarget = env.VITE_PROXY_TARGET?.trim() || 'http://localhost:5092'

  return {
    plugins: [react()],
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
