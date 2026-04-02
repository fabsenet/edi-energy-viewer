/// <reference types="vite/client" />

// TypeScript 6 requires explicit ambient declarations for CSS side-effect imports
// that don't ship their own type declarations (e.g. vuetify/styles).
declare module 'vuetify/styles' {}
