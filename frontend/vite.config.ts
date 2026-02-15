import { fileURLToPath, URL } from "node:url";
import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";
import { VitePWA } from "vite-plugin-pwa";

// https://vitejs.dev/config/
export default defineConfig({
  base: "/",
  plugins: [
    vue(),
    VitePWA({
      registerType: "autoUpdate", // Actualización automática en segundo plano
      includeAssets: ["favicon.ico", "logo.png", "icons/*.png"],

      manifest: {
        name: "Temges App",
        short_name: "Temges",
        description: "Sistema ERP para gestión de manufactura",
        theme_color: "#3f51b5", // Color azul del logo
        background_color: "#ffffff",
        display: "standalone",
        orientation: "any",
        scope: "/",
        start_url: "/",
        icons: [
          {
            src: "icons/icon-192x192.png",
            sizes: "192x192",
            type: "image/png",
            purpose: "any maskable",
          },
          {
            src: "icons/icon-512x512.png",
            sizes: "512x512",
            type: "image/png",
            purpose: "any maskable",
          },
        ],
      },

      workbox: {
        // CRITICAL: Excluir todas las llamadas API del service worker
        navigateFallback: null,
        navigateFallbackDenylist: [/^\/api/],

        // Excluir explícitamente /api de cualquier caching
        globIgnores: ["**/api/**"],

        runtimeCaching: [
          // Eliminada completamente la regla de API - que el navegador las maneje directamente
          {
            // Assets estáticos - Cache First
            urlPattern: /\.(?:png|jpg|jpeg|svg|gif|webp|ico)$/,
            handler: "CacheFirst",
            options: {
              cacheName: "images-cache",
              expiration: {
                maxEntries: 100,
                maxAgeSeconds: 60 * 60 * 24 * 30, // 30 días
              },
            },
          },
          {
            // Fuentes y CSS externos
            urlPattern: /^https:\/\/unpkg\.com\/.*/i,
            handler: "CacheFirst",
            options: {
              cacheName: "cdn-cache",
              expiration: {
                maxEntries: 30,
                maxAgeSeconds: 60 * 60 * 24 * 365, // 1 año
              },
            },
          },
        ],

        // Forzar saltar waiting y tomar control inmediato
        skipWaiting: true,
        clientsClaim: true,

        cleanupOutdatedCaches: true,
        maximumFileSizeToCacheInBytes: 5 * 1024 * 1024, // 5MB
      },

      devOptions: {
        enabled: false, // Deshabilitado en desarrollo (usar preview para probar)
      },
    }),
  ],
  server: {
    port: 8100,
  },
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url)),
    },
  },
  css: {
    preprocessorOptions: {
      scss: {
        // Vite 6+ uses modern API by default, api option can be removed
        // Keeping this explicitly for clarity
        api: "modern-compiler",
      },
    },
  },
});
