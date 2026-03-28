import { type FormEvent, useEffect, useMemo, useState } from 'react'
import { Link, useLocation, useNavigate, type Location } from 'react-router-dom'
import { isAxiosError } from 'axios'
import { useAuth } from '../auth/AuthContext'

type FieldErrors = {
  email?: string
  password?: string
}

/** Uses `location.state.from` when present (e.g. `<Navigate to="/login" state={{ from: location }} replace />`). */
function resolvePostLoginPath(location: Location): string {
  const state = location.state as { from?: Location } | undefined
  const candidate = state?.from?.pathname
  if (candidate && candidate !== '/login' && candidate !== '/register') {
    return candidate
  }
  return '/'
}

export function LoginPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const { login, isAuthenticated, isInitializing } = useAuth()

  const redirectTo = useMemo(() => resolvePostLoginPath(location), [location])

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({})
  const [formError, setFormError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  useEffect(() => {
    if (isInitializing || !isAuthenticated) {
      return
    }
    navigate(redirectTo, { replace: true })
  }, [isAuthenticated, isInitializing, navigate, redirectTo])

  function validate(): boolean {
    const next: FieldErrors = {}
    const trimmedEmail = email.trim()

    if (!trimmedEmail) {
      next.email = 'Email is required.'
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(trimmedEmail)) {
      next.email = 'Enter a valid email address.'
    }

    if (!password) {
      next.password = 'Password is required.'
    }

    setFieldErrors(next)
    return Object.keys(next).length === 0
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setFormError(null)

    if (!validate()) {
      return
    }

    setIsSubmitting(true)
    try {
      await login({ email: email.trim(), password })
    } catch (err) {
      if (isAxiosError(err)) {
        const status = err.response?.status
        if (status === 401) {
          setFormError('Invalid email or password.')
        } else if (status === 423) {
          setFormError('This account is temporarily locked. Try again later.')
        } else if (status === 400) {
          const data = err.response?.data as { title?: string; detail?: string; errors?: Record<string, string[]> }
          const messages = data?.errors
            ? Object.values(data.errors)
                .flat()
                .filter((m): m is string => typeof m === 'string' && m.length > 0)
            : []
          setFormError(
            messages.length > 0
              ? messages.join(' ')
              : 'Use a full email address (include @ and the domain, e.g. name@gmail.com).',
          )
        } else {
          const data = err.response?.data as { title?: string; detail?: string; errors?: Record<string, string[]> }
          if (data?.errors) {
            const messages = Object.values(data.errors).flat().filter(Boolean)
            if (messages.length > 0) {
              setFormError(messages.join(' '))
            } else {
              setFormError(data.detail ?? data.title ?? 'Sign in failed. Please try again.')
            }
          } else {
            setFormError(data?.detail ?? data.title ?? 'Sign in failed. Please try again.')
          }
        }
      } else {
        setFormError('Something went wrong. Please try again.')
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  if (isInitializing) {
    return (
      <div className="mx-auto flex min-h-[50vh] max-w-md flex-col items-center justify-center px-4 py-16">
        <p className="text-sm text-slate-500" role="status">
          Checking session…
        </p>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-md px-4 py-10">
      <h1 className="text-2xl font-semibold tracking-tight text-slate-900">Sign in</h1>
      <p className="mt-1 text-sm text-slate-600">
        Use your TechVault account.{' '}
        <Link className="font-medium text-blue-600 hover:underline" to="/register">
          Create account
        </Link>
        {' · '}
        <Link className="font-medium text-blue-600 hover:underline" to="/">
          Home
        </Link>
      </p>

      <form
        className="mt-8 space-y-5 rounded-lg border border-slate-200 bg-white p-6 shadow-sm"
        onSubmit={handleSubmit}
        noValidate
      >
        {formError && (
          <div
            className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-800"
            role="alert"
          >
            {formError}
          </div>
        )}

        <div>
          <label htmlFor="login-email" className="block text-sm font-medium text-slate-700">
            Email
          </label>
          <input
            id="login-email"
            name="email"
            type="email"
            autoComplete="email"
            value={email}
            onChange={(e) => {
              setEmail(e.target.value)
              if (fieldErrors.email) {
                setFieldErrors((prev) => ({ ...prev, email: undefined }))
              }
            }}
            disabled={isSubmitting}
            className="mt-1 block w-full rounded-md border border-slate-300 px-3 py-2 text-slate-900 shadow-sm placeholder:text-slate-400 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/30 disabled:cursor-not-allowed disabled:bg-slate-50"
            aria-invalid={Boolean(fieldErrors.email)}
            aria-describedby={fieldErrors.email ? 'login-email-error' : undefined}
          />
          {fieldErrors.email && (
            <p id="login-email-error" className="mt-1 text-sm text-red-600" role="alert">
              {fieldErrors.email}
            </p>
          )}
        </div>

        <div>
          <label htmlFor="login-password" className="block text-sm font-medium text-slate-700">
            Password
          </label>
          <input
            id="login-password"
            name="password"
            type="password"
            autoComplete="current-password"
            value={password}
            onChange={(e) => {
              setPassword(e.target.value)
              if (fieldErrors.password) {
                setFieldErrors((prev) => ({ ...prev, password: undefined }))
              }
            }}
            disabled={isSubmitting}
            className="mt-1 block w-full rounded-md border border-slate-300 px-3 py-2 text-slate-900 shadow-sm placeholder:text-slate-400 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/30 disabled:cursor-not-allowed disabled:bg-slate-50"
            aria-invalid={Boolean(fieldErrors.password)}
            aria-describedby={fieldErrors.password ? 'login-password-error' : undefined}
          />
          {fieldErrors.password && (
            <p id="login-password-error" className="mt-1 text-sm text-red-600" role="alert">
              {fieldErrors.password}
            </p>
          )}
        </div>

        <button
          type="submit"
          disabled={isSubmitting}
          className="flex w-full items-center justify-center rounded-md bg-blue-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-60"
        >
          {isSubmitting ? (
            <>
              <svg
                className="-ml-1 mr-2 h-4 w-4 animate-spin text-white"
                xmlns="http://www.w3.org/2000/svg"
                fill="none"
                viewBox="0 0 24 24"
                aria-hidden
              >
                <circle
                  className="opacity-25"
                  cx="12"
                  cy="12"
                  r="10"
                  stroke="currentColor"
                  strokeWidth="4"
                />
                <path
                  className="opacity-75"
                  fill="currentColor"
                  d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                />
              </svg>
              Signing in…
            </>
          ) : (
            'Sign in'
          )}
        </button>
      </form>
    </div>
  )
}
