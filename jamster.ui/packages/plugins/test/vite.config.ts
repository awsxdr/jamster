import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import ModuleFederationPlugin from "@originjs/vite-plugin-federation";

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    react(),
    ModuleFederationPlugin({
      name: "ui",
      filename: "remoteEntry.js",
      shared: ["react", "react-dom"],
    }),
  ],
  build: {
    target: "esnext",
    minify: true,
    cssCodeSplit: false,
  }
})