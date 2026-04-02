import pluginVue from 'eslint-plugin-vue'
import vueTsEslintConfig from '@vue/eslint-config-typescript'

export default [
  {
    name: 'app/files-to-lint',
    files: ['**/*.{ts,mts,tsx,vue}'],
  },

  {
    name: 'app/files-to-ignore',
    ignores: ['**/dist/**', '**/dist-ssr/**', '**/coverage/**'],
  },
  {
    rules: {
      "vue/no-mutating-props": ["error", {
        "shallowOnly": false
      }],
      // Vuetify uses dot-notation slot names like #item.columnName which ESLint
      // parses as a slot with a modifier; allow modifiers to support this pattern.
      "vue/valid-v-slot": ["error", { "allowModifiers": true }]
    }
  },

  ...pluginVue.configs['flat/essential'],
  ...vueTsEslintConfig(),
]
