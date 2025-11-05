import { reactRouter } from "@react-router/dev/vite";
import tailwindcss from "@tailwindcss/vite";
import { defineConfig, loadEnv } from "vite";
import tsconfigPaths from "vite-tsconfig-paths";

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');
  const API = process.env.services__apiservice__https__0 || process.env.services__apiservice__http__0;
  return {
    plugins: [tailwindcss(), reactRouter(), tsconfigPaths()],
    server: {
      port: parseInt(env.VITE_PORT),
      proxy: {
        // "apiservice" is the name of the API in AppHost.cs.
        '/api': {
          target: API,
          changeOrigin: true,
          secure: false
        },
        '/auth': {
          target: API,
          changeOrigin: true,
          secure: false
        }
      }
    },
    build: {
      outDir: 'dist',
      rollupOptions: {
        input: './index.html'
      }
    }
  }
})