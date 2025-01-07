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


const vuetify = createVuetify({
  components,
  directives,
  locale: {
    locale: 'de',
    fallback: 'en',
    messages: { de, en },
  },
})

createApp(App).use(vuetify).mount('#app')
