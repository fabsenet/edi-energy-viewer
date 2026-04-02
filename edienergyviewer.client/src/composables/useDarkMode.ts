import { computed } from 'vue';
import { useTheme } from 'vuetify';

export function useDarkMode() {
  const theme = useTheme();

  const isDark = computed(() => theme.global.current.value.dark);

  function toggleDarkMode() {
    const next = isDark.value ? 'light' : 'dark';
    theme.change(next);
    localStorage.setItem('theme', next);
  }

  return { isDark, toggleDarkMode };
}
