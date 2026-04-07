import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";
import tailwindcss from "@tailwindcss/vite";
import fs from "fs";

export default defineConfig({
  plugins: [vue(), tailwindcss()],
  server: {
    host: "0.0.0.0",
    port: 3000,
    https: {
      key: fs.readFileSync("C:/mkcert/172.20.10.2+2-key.pem"),
      cert: fs.readFileSync("C:/mkcert/172.20.10.2+2.pem"),
    },
  },
});
