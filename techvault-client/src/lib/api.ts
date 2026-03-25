import axios from 'axios'
import type { AxiosError, InternalAxiosRequestConfig } from 'axios'

const baseURL = import.meta.env.VITE_API_BASE_URL ?? '/api'

export const api = axios.create({
  baseURL,
  withCredentials: true,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 15_000,
})

type RetryableRequestConfig = InternalAxiosRequestConfig & { _retry?: boolean }

type AuthInterceptorOptions = {
  getAccessToken: () => string | null
  refreshAccessToken: () => Promise<string | null>
  onAuthFailure: () => void
}

let requestInterceptorId: number | null = null
let responseInterceptorId: number | null = null

export function setupAuthInterceptors(options: AuthInterceptorOptions) {
  if (requestInterceptorId !== null) {
    api.interceptors.request.eject(requestInterceptorId)
  }
  if (responseInterceptorId !== null) {
    api.interceptors.response.eject(responseInterceptorId)
  }

  requestInterceptorId = api.interceptors.request.use((config) => {
    const token = options.getAccessToken()
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  })

  responseInterceptorId = api.interceptors.response.use(
    (response) => response,
    async (error: AxiosError) => {
      const originalRequest = error.config as RetryableRequestConfig | undefined

      if (!originalRequest || error.response?.status !== 401 || originalRequest._retry) {
        return Promise.reject(error)
      }

      const requestUrl = originalRequest.url ?? ''
      if (requestUrl.includes('/auth/refresh') || requestUrl.includes('/auth/login') || requestUrl.includes('/auth/register')) {
        return Promise.reject(error)
      }

      originalRequest._retry = true

      try {
        const nextToken = await options.refreshAccessToken()
        if (!nextToken) {
          options.onAuthFailure()
          return Promise.reject(error)
        }

        originalRequest.headers.Authorization = `Bearer ${nextToken}`
        return api(originalRequest)
      } catch (refreshError) {
        options.onAuthFailure()
        return Promise.reject(refreshError)
      }
    },
  )
}
