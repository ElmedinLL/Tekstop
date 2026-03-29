import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from 'react'
import axios from 'axios'
import { api, setupAuthInterceptors } from '../lib/api'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? '/api/v1'

const authClient = axios.create({
  baseURL: apiBaseUrl,
  withCredentials: true,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 15_000,
})

type AuthResponseDto = {
  accessToken: string
  accessTokenExpiresAtUtc: string
  refreshToken: string
}

type CurrentUserProfile = {
  userId: string
  email: string
  firstName: string
  lastName: string
  profilePicture?: string | null
  roles: string[]
}

type LoginInput = {
  email: string
  password: string
}

type RegisterInput = {
  email: string
  password: string
  confirmPassword: string
  firstName: string
  lastName: string
}

type AuthContextValue = {
  user: CurrentUserProfile | null
  accessToken: string | null
  isAuthenticated: boolean
  isInitializing: boolean
  login: (input: LoginInput) => Promise<void>
  register: (input: RegisterInput) => Promise<void>
  logout: () => Promise<void>
  refresh: () => Promise<string | null>
  /** Reloads `/auth/me` (e.g. after profile or avatar update). */
  refreshUser: () => Promise<void>
  /** Merges into cached user (e.g. after profile/avatar API without new JWT). */
  updateLocalUser: (patch: Partial<CurrentUserProfile>) => void
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

type AuthProviderProps = {
  children: ReactNode
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [accessToken, setAccessToken] = useState<string | null>(null)
  const [user, setUser] = useState<CurrentUserProfile | null>(null)
  const [isInitializing, setIsInitializing] = useState(true)
  const refreshPromiseRef = useRef<Promise<string | null> | null>(null)
  const refreshTokenRef = useRef<string | null>(null)
  /** Mirrors access token state but updates synchronously so /auth/me works immediately after login/register. */
  const accessTokenRef = useRef<string | null>(null)

  const clearSession = useCallback(() => {
    setAccessToken(null)
    accessTokenRef.current = null
    setUser(null)
    refreshTokenRef.current = null
  }, [])

  const fetchMe = useCallback(async () => {
    const response = await api.get<CurrentUserProfile>('/auth/me')
    setUser(response.data)
    return response.data
  }, [])

  const refresh = useCallback(async (): Promise<string | null> => {
    if (refreshPromiseRef.current) {
      return refreshPromiseRef.current
    }

    refreshPromiseRef.current = (async () => {
      try {
        const rt = refreshTokenRef.current
        if (!rt) {
          return null
        }
        const response = await authClient.post<AuthResponseDto>('/auth/refresh', {
          refreshToken: rt,
        })
        refreshTokenRef.current = response.data.refreshToken
        accessTokenRef.current = response.data.accessToken
        setAccessToken(response.data.accessToken)
        return response.data.accessToken
      } catch {
        clearSession()
        return null
      } finally {
        refreshPromiseRef.current = null
      }
    })()

    return refreshPromiseRef.current
  }, [clearSession])

  const establishSession = useCallback(
    async (tokens: { accessToken: string; refreshToken: string }) => {
      refreshTokenRef.current = tokens.refreshToken
      accessTokenRef.current = tokens.accessToken
      setAccessToken(tokens.accessToken)
      await fetchMe()
    },
    [fetchMe],
  )

  const login = useCallback(
    async (input: LoginInput) => {
      const response = await authClient.post<AuthResponseDto>('/auth/login', input)
      await establishSession(response.data)
    },
    [establishSession],
  )

  const register = useCallback(
    async (input: RegisterInput) => {
      const response = await authClient.post<AuthResponseDto>('/auth/register', input)
      await establishSession(response.data)
    },
    [establishSession],
  )

  const refreshUser = useCallback(async () => {
    try {
      if (accessToken) {
        await fetchMe()
      }
    } catch {
      // ignore; interceptor may sign out on 401
    }
  }, [accessToken, fetchMe])

  const updateLocalUser = useCallback((patch: Partial<CurrentUserProfile>) => {
    setUser((prev) => (prev ? { ...prev, ...patch } : prev))
  }, [])

  const logout = useCallback(async () => {
    try {
      const rt = refreshTokenRef.current
      if (rt) {
        await api.post('/auth/logout', { refreshToken: rt })
      }
    } finally {
      clearSession()
    }
  }, [clearSession])

  useEffect(() => {
    setupAuthInterceptors({
      getAccessToken: () => accessTokenRef.current,
      refreshAccessToken: refresh,
      onAuthFailure: clearSession,
    })
  }, [refresh, clearSession])

  useEffect(() => {
    let isMounted = true

    ;(async () => {
      try {
        const token = await refresh()
        if (!token || !isMounted) {
          return
        }
        await fetchMe()
      } finally {
        if (isMounted) {
          setIsInitializing(false)
        }
      }
    })()

    return () => {
      isMounted = false
    }
  }, [refresh, fetchMe])

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      accessToken,
      isAuthenticated: Boolean(accessToken && user),
      isInitializing,
      login,
      register,
      logout,
      refresh,
      refreshUser,
      updateLocalUser,
    }),
    [user, accessToken, isInitializing, login, register, logout, refresh, refreshUser, updateLocalUser],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider')
  }
  return context
}
