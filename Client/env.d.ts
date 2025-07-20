/// <reference types="vite/client" />

// This file is used to declare types for vue modules that do not have type definitions.
declare module '*.vue' {
  import { DefineComponent } from 'vue'
  const component: DefineComponent<{}, {}, any>
  export default component
}
