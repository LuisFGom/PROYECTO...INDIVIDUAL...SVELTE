import { vitePreprocess } from '@sveltejs/vite-plugin-svelte'

export default {
  preprocess: vitePreprocess(),
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
}
