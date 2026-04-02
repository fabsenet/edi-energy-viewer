import '@mdi/font/css/materialdesignicons.css'
import { createApp } from 'vue'
import { de, en } from 'vuetify/locale'

// Vuetify
import 'vuetify/styles'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'

// Components
import App from './App.vue'


const storedTheme = localStorage.getItem('theme') ?? 'light';

const vuetify = createVuetify({
  components,
  directives,
  theme: {
    defaultTheme: storedTheme,
  },
  locale: {
    locale: 'de',
    fallback: 'en',
    messages: { de, en },
  },
})

createApp(App).use(vuetify).mount('#app')

// Remove the anti-flash inline styles set by index.html — Vuetify has taken over.
const html = document.documentElement;
html.style.removeProperty('color-scheme');
html.style.removeProperty('background-color');
html.style.removeProperty('color');
