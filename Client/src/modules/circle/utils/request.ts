// utils/request.ts
import axios from 'axios'

const service = axios.create({
  baseURL: 'http://120.26.118.70:5001',
  timeout: 5000,
  headers: {
    Cookie: 'auth=auth',
  },
})

service.interceptors.request.use(
  (config) => {
    Object.assign(config.headers, {
      Cookie:
        'auth=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyIjp7ImlkIjoxLCJsb2NhbGUiOiJ6aC1jbiIsInZpZXdNb2RlIjoibW9zYWljIiwic2luZ2xlQ2xpY2siOmZhbHNlLCJwZXJtIjp7ImFkbWluIjp0cnVlLCJleGVjdXRlIjp0cnVlLCJjcmVhdGUiOnRydWUsInJlbmFtZSI6dHJ1ZSwibW9kaWZ5Ijp0cnVlLCJkZWxldGUiOnRydWUsInNoYXJlIjp0cnVlLCJkb3dubG9hZCI6dHJ1ZX0sImNvbW1hbmRzIjpbImJhc2giLCJscyIsInNoIiwid2dldCIsImNobW9kIiwicm0iLCJta2RpciIsInB3ZCJdLCJsb2NrUGFzc3dvcmQiOmZhbHNlLCJoaWRlRG90ZmlsZXMiOmZhbHNlLCJkYXRlRm9ybWF0IjpmYWxzZSwidXNlcm5hbWUiOiJhZG1pbiJ9LCJpc3MiOiJGaWxlIEJyb3dzZXIiLCJleHAiOjE3NTY4OTUyMjYsImlhdCI6MTc1Njg4ODAyNn0.peafDWEB1Ep2qxHFhLj07KGndpk0tb2S3zvqV-m4O3I',
    })
    return config
  },
  (error) => Promise.reject(error),
)

export default service
