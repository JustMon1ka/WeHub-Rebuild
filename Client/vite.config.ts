import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue(), vueDevTools(), tailwindcss()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5080',
        changeOrigin: true,
        secure: false,
        configure: (proxy, _options) => {
          proxy.on('error', (err, _req, _res) => {
            console.log('❌ API proxy error', err)
          })
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            console.log('🚀 API Request:', req.method, req.url)
            console.log('🎯 Target:', proxyReq.path)
          })
          proxy.on('proxyRes', (proxyRes, req, _res) => {
            console.log('📥 API Response:', proxyRes.statusCode, req.url)
            if (proxyRes.statusCode >= 400) {
              console.log('❌ Error status:', proxyRes.statusCode)
            }
          })
        },
      },

      // 添加文件代理配置
      '/files': {
        target: 'http://120.26.118.70:5001',
        changeOrigin: true,
        secure: false,
        configure: (proxy, _options) => {
          proxy.on('error', (err, _req, _res) => {
            console.log('files proxy error', err)
          })
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            console.log('📁 Files Request:', req.method, req.url)
          })
          proxy.on('proxyRes', (proxyRes, req, _res) => {
            console.log('📁 Files Response:', proxyRes.statusCode, req.url)
          })
        },
      },
    },
  },
})
