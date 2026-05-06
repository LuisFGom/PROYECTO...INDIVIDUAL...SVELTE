import { defineConfig } from 'vite'
import { svelte } from '@sveltejs/vite-plugin-svelte'

export default defineConfig({
  plugins: [
    svelte({
      onwarn: (warning, handler) => {
        // Ignorar warnings de accesibilidad (A11y)
        if (warning.code.startsWith('a11y-')) {
          return
        }
        // Ignorar CSS selectors sin usar
        if (warning.code === 'unused-selector') {
          return
        }
        handler(warning)
      }
    })
  ],
  server: {
    port: 5173,
    strictPort: false,
    hmr: {
      overlay: false
    }
  }
})
