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
      // 认证相关接口代理到认证服务
      '^/api/auth': {
        target: 'http://localhost:5001',
        changeOrigin: true,
        secure: false,
        configure: (proxy, _options) => {
          proxy.on('error', (err, _req, _res) => {
            console.log('❌ Auth API proxy error', err)
          })
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            console.log('🔐 Auth Request:', req.method, req.url)
            console.log('🎯 Target (Auth):', proxyReq.path)
          })
          proxy.on('proxyRes', (proxyRes, req, _res) => {
            console.log('🔐 Auth Response:', proxyRes.statusCode, req.url)
            if (proxyRes.statusCode >= 400) {
              console.log('❌ Auth Error status:', proxyRes.statusCode)
            }
          })
        },
      },

      // 用户数据相关接口代理到用户数据服务
      '^/api/user': {
        target: 'http://localhost:5002',
        changeOrigin: true,
        secure: false,
        configure: (proxy, _options) => {
          proxy.on('error', (err, _req, _res) => {
            console.log('❌ User API proxy error', err)
          })
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            console.log('👤 User Request:', req.method, req.url)
            console.log('🎯 Target (User):', proxyReq.path)
          })
          proxy.on('proxyRes', (proxyRes, req, _res) => {
            console.log('👤 User Response:', proxyRes.statusCode, req.url)
            if (proxyRes.statusCode >= 400) {
              console.log('❌ User Error status:', proxyRes.statusCode)
            }
          })
        },
      },

      // 发帖相关接口代理到发帖服务
      '^/api/posts': {
        target: 'http://localhost:5006',
        changeOrigin: true,
        secure: false,
        configure: (proxy, _options) => {
          proxy.on('error', (err, _req, _res) => {
            console.log('❌ Posts API proxy error', err)
          })
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            console.log('📝 Posts Request:', req.method, req.url)
            console.log('🎯 Target (Posts):', proxyReq.path)
          })
          proxy.on('proxyRes', (proxyRes, req, _res) => {
            console.log('📝 Posts Response:', proxyRes.statusCode, req.url)
            if (proxyRes.statusCode >= 400) {
              console.log('❌ Posts Error status:', proxyRes.statusCode)
            }
          })
        },
      },

      // 媒体文件相关接口代理到媒体服务
      '^/api/media': {
        target: 'http://localhost:5004',
        changeOrigin: true,
        secure: false,
        configure: (proxy, _options) => {
          proxy.on('error', (err, _req, _res) => {
            console.log('❌ Media API proxy error', err)
          })
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            console.log('📷 Media Request:', req.method, req.url)
            console.log('🎯 Target (Media):', proxyReq.path)
          })
          proxy.on('proxyRes', (proxyRes, req, _res) => {
            console.log('📷 Media Response:', proxyRes.statusCode, req.url)
          })
        },
      },

      // 标签相关接口代理到标签服务
      '^/api/tags': {
        target: 'http://localhost:5005',
        changeOrigin: true,
        secure: false,
        configure: (proxy, _options) => {
          proxy.on('error', (err, _req, _res) => {
            console.log('❌ Tags API proxy error', err)
          })
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            console.log('🏷️ Tags Request:', req.method, req.url)
            console.log('🎯 Target (Tags):', proxyReq.path)
          })
          proxy.on('proxyRes', (proxyRes, req, _res) => {
            console.log('🏷️ Tags Response:', proxyRes.statusCode, req.url)
          })
        },
      },

      // 消息相关接口代理到消息服务
      '^/api/messages': {
        target: 'http://localhost:5030',
        changeOrigin: true,
        secure: false,
        configure: (proxy, _options) => {
          proxy.on('error', (err, _req, _res) => {
            console.log('❌ Messages API proxy error', err)
          })
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            console.log('💬 Messages Request:', req.method, req.url)
            console.log('🎯 Target (Messages):', proxyReq.path)
          })
          proxy.on('proxyRes', (proxyRes, req, _res) => {
            console.log('💬 Messages Response:', proxyRes.statusCode, req.url)
          })
        },
      },

      // 通知相关接口代理到通知服务
      '^/api/notices': {
        target: 'http://localhost:5103',
        changeOrigin: true,
        secure: false,
        configure: (proxy, _options) => {
          proxy.on('error', (err, _req, _res) => {
            console.log('❌ Notices API proxy error', err)
          })
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            console.log('🔔 Notices Request:', req.method, req.url)
            console.log('🎯 Target (Notices):', proxyReq.path)
          })
          proxy.on('proxyRes', (proxyRes, req, _res) => {
            console.log('🔔 Notices Response:', proxyRes.statusCode, req.url)
          })
        },
      },

      // 关注相关接口代理到关注服务
      '^/api/follow': {
        target: 'http://localhost:5251',
        changeOrigin: true,
        secure: false,
        configure: (proxy, _options) => {
          proxy.on('error', (err, _req, _res) => {
            console.log('❌ Follow API proxy error', err)
          })
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            console.log('👥 Follow Request:', req.method, req.url)
            console.log('🎯 Target (Follow):', proxyReq.path)
          })
          proxy.on('proxyRes', (proxyRes, req, _res) => {
            console.log('👥 Follow Response:', proxyRes.statusCode, req.url)
          })
        },
      },

      // 举报相关接口代理到举报服务
      '^/api/reports': {
        target: 'http://localhost:5173',
        changeOrigin: true,
        secure: false,
        configure: (proxy, _options) => {
          proxy.on('error', (err, _req, _res) => {
            console.log('❌ Reports API proxy error', err)
          })
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            console.log('🚨 Reports Request:', req.method, req.url)
            console.log('🎯 Target (Reports):', proxyReq.path)
          })
          proxy.on('proxyRes', (proxyRes, req, _res) => {
            console.log('🚨 Reports Response:', proxyRes.statusCode, req.url)
          })
        },
      },

      // 其他所有 API 请求代理到社区服务 (5080)
      '/api': {
        target: 'http://localhost:5080',
        changeOrigin: true,
        secure: false,
        configure: (proxy, _options) => {
          proxy.on('error', (err, _req, _res) => {
            console.log('❌ Community API proxy error', err)
          })
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            console.log('🏘️ Community Request:', req.method, req.url)
            console.log('🎯 Target (Community):', proxyReq.path)
          })
          proxy.on('proxyRes', (proxyRes, req, _res) => {
            console.log('🏘️ Community Response:', proxyRes.statusCode, req.url)
            if (proxyRes.statusCode >= 400) {
              console.log('❌ Community Error status:', proxyRes.statusCode)
            }
          })
        },
      },

      // 文件代理配置保持不变
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
