import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  build: {
    outDir: process.env.VITE_BUILD_OUT_DIR ?? "dist",
    chunkSizeWarningLimit: 600,
    rollupOptions: {
      output: {
        manualChunks: (id) => {
          // Vendor chunks for better caching
          if (id.includes("node_modules")) {
            // MUI core libraries
            if (
              id.includes("@mui/material") ||
              id.includes("@mui/system") ||
              id.includes("@emotion")
            ) {
              return "vendor-mui";
            }
            // MUI X components (Data Grid, Date Pickers)
            if (id.includes("@mui/x-")) {
              return "vendor-mui-x";
            }
            // Azure MSAL
            if (id.includes("@azure/msal")) {
              return "vendor-msal";
            }
            // React Query
            if (id.includes("@tanstack/react-query")) {
              return "vendor-react-query";
            }
            // React Router
            if (id.includes("react-router")) {
              return "vendor-router";
            }
            // i18next
            if (id.includes("i18next")) {
              return "vendor-i18n";
            }
            // Luxon (date library)
            if (id.includes("luxon")) {
              return "vendor-luxon";
            }
            // React core
            if (id.includes("react") || id.includes("react-dom")) {
              return "vendor-react";
            }
            // Other vendor libraries
            return "vendor";
          }
        },
      },
    },
  },
});
