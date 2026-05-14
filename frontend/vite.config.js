import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "")
  const feedbackApiTarget = env.VITE_FEEDBACK_API_URL || "http://localhost:6001"

  return {
    plugins: [react()],
    server: {
      proxy: {
        "/feedback-api": {
          target: feedbackApiTarget,
          changeOrigin: true,
          secure: false,
          rewrite: (path) => path.replace(/^\/feedback-api/, ""),
        },
      },
    },
  }
})
