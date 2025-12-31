import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  build: {
    outDir: process.env.VITE_BUILD_OUT_DIR ?? "dist",
    chunkSizeWarningLimit: 600,
    // Let Vite handle automatic chunking to avoid circular dependency issues
    // Route-based code splitting (React.lazy) will still work for application code
  },
});
