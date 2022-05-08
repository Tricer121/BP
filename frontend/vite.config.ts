import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";
import vuetify from "@vuetify/vite-plugin";
import mkcert from "vite-plugin-mkcert";

const path = require("path");

// https://vitejs.dev/config/
export default defineConfig({
  server: {   
    port: 8080
  },
  plugins: [
    vue(),
    vuetify({
      autoImport: true,
    }),
  ],
  define: { "process.env": {} },
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "src"),
      "leaflet/dist/images/layers.png$": path.resolve(
        __dirname,
        "../node_modules/leaflet/dist/images/layers.png"
      ),
      "leaflet/dist/images/layers-2x.png$": path.resolve(
        __dirname,
        "../node_modules/leaflet/dist/images/layers-2x.png"
      ),
      "leaflet/dist/images/marker-icon.png$": path.resolve(
        __dirname,
        "../node_modules/leaflet/dist/images/marker-icon.png"
      ),
      "leaflet/dist/images/marker-icon-2x.png$": path.resolve(
        __dirname,
        "../node_modules/leaflet/dist/images/marker-icon-2x.png"
      ),
      "leaflet/dist/images/marker-shadow.png$": path.resolve(
        __dirname,
        "../node_modules/leaflet/dist/images/marker-shadow.png"
      ),
    },
  },
  /* remove the need to specify .vue files https://vitejs.dev/config/#resolve-extensions
  resolve: {
    extensions: [
      '.js',
      '.json',
      '.jsx',
      '.mjs',
      '.ts',
      '.tsx',
      '.vue',
    ]
  },
  */
});
