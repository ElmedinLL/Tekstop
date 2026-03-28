import { type FormEvent, useEffect, useMemo, useState } from 'react'
import { Link, useLocation, useNavigate, type Location } from 'react-router-dom'
import { isAxiosError } from 'axios'
import { useAuth } from '../auth/AuthContext'
import { Seo } from '../components/Seo'
import { evaluatePasswordStrength, passwordStrengthCriteriaLabels } from '../lib/passwordStrength'

type FieldErrors = {
  firstName?: string
  lastName?: string
  email?: string
  password?: string
  confirmPassword?: string
}

function resolvePostRegisterPath(location: Location): string {
  const state = location.state as { from?: Location } | undefined
  const candidate = state?.from?.pathname
  if (candidate && candidate !== '/login' && candidate !== '/register') {
    return candidate
  }
  return '/'
}

export function RegisterPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const { register, isAuthenticated, isInitializing } = useAuth()

  const redirectTo = useMemo(() => resolvePostRegisterPath(location), [location])

  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({})
  const [formError, setFormError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const strength = useMemo(() => evaluatePasswordStrength(password), [password])
  const criteriaLabels = passwordStrengthCriteriaLabels()

  useEffect(() => {
    if (isInitializing || !isAuthenticated) {
      return
    }
    navigate(redirectTo, { replace: true })
  }, [isAuthenticated, isInitializing, navigate, redirectTo])

  function validate(): boolean {
    const next: FieldErrors = {}
    const trimmedFirst = firstName.trim()
    const trimmedLast = lastName.trim()
    const trimmedEmail = email.trim()

    if (!trimmedFirst) {
      next.firstName = 'First name is required.'
    }

    if (!trimmedLast) {
      next.lastName = 'Last name is required.'
    }

    if (!trimmedEmail) {
      next.email = 'Email is required.'
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(trimmedEmail)) {
      next.email = 'Enter a valid email address.'
    }

    if (!password) {
      next.password = 'Password is required.'
    } else if (password.length < 8) {
      next.password = 'Password must be at least 8 characters.'
    }

    if (!confirmPassword) {
      next.confirmPassword = 'Confirm your password.'
    } else if (confirmPassword !== password) {
      next.confirmPassword = 'Passwords do not match.'
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
      await register({
        email: email.trim(),
        password,
        firstName: firstName.trim(),
        lastName: lastName.trim(),
      })
    } catch (err) {
      if (isAxiosError(err)) {
        const data = err.response?.data as {
          title?: string
          detail?: string
          errors?: Record<string, string[]>
        }

        if (data?.errors && Object.keys(data.errors).length > 0) {
          const model: FieldErrors = {}
          for (const [key, messages] of Object.entries(data.errors)) {
            const msg = messages?.[0]
            if (!msg) continue
            const normalized = key.toLowerCase()
            if (normalized.includes('firstname')) model.firstName = msg
            else if (normalized.includes('lastname')) model.lastName = msg
            else if (normalized.includes('email')) model.email = msg
            else if (normalized.includes('password')) model.password = msg
          }
          if (Object.keys(model).length > 0) {
            setFieldErrors((prev) => ({ ...prev, ...model }))
          }
        }

        const fromValidation = data?.errors
          ? Object.values(data.errors)
              .flat()
              .filter(Boolean)
              .join(' ')
              .trim()
          : ''
        setFormError(
          data?.detail?.trim() ||
            data?.title?.trim() ||
            fromValidation ||
            'Registration failed. Please try again.',
        )
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
        <Seo title="Create account" description="Checking your session…" noindex />
        <p className="text-sm text-slate-500" role="status">
          Checking session…
        </p>
      </div>
    )
  }

  const inputClass =
    'mt-1 block w-full rounded-md border border-slate-300 px-3 py-2 text-slate-900 shadow-sm placeholder:text-slate-400 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/30 disabled:cursor-not-allowed disabled:bg-slate-50'

  return (
    <div className="mx-auto max-w-md px-4 py-10">
      <Seo
        title="Create account"
        description="Create a TechVault account to save addresses, track orders, and use your wishlist."
        noindex
      />
      <h1 className="text-2xl font-semibold tracking-tight text-slate-900">Create account</h1>
      <p className="mt-1 text-sm text-slate-600">
        Already have an account?{' '}
        <Link className="font-medium text-blue-600 hover:underline" to="/login">
          Sign in
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

        <div className="grid gap-4 sm:grid-cols-2">
          <div className="sm:col-span-1">
            <label htmlFor="register-first-name" className="block text-sm font-medium text-slate-700">
              First name
            </label>
            <input
              id="register-first-name"
              name="firstName"
              type="text"
              autoComplete="given-name"
              value={firstName}
              onChange={(e) => {
                setFirstName(e.target.value)
                if (fieldErrors.firstName) {
                  setFieldErrors((prev) => ({ ...prev, firstName: undefined }))
                }
              }}
              disabled={isSubmitting}
              className={inputClass}
              aria-invalid={Boolean(fieldErrors.firstName)}
              aria-describedby={fieldErrors.firstName ? 'register-first-name-error' : undefined}
            />
            {fieldErrors.firstName && (
              <p id="register-first-name-error" className="mt-1 text-sm text-red-600" role="alert">
                {fieldErrors.firstName}
              </p>
            )}
          </div>
          <div className="sm:col-span-1">
            <label htmlFor="register-last-name" className="block text-sm font-medium text-slate-700">
              Last name
            </label>
            <input
              id="register-last-name"
              name="lastName"
              type="text"
              autoComplete="family-name"
              value={lastName}
              onChange={(e) => {
                setLastName(e.target.value)
                if (fieldErrors.lastName) {
                  setFieldErrors((prev) => ({ ...prev, lastName: undefined }))
                }
              }}
              disabled={isSubmitting}
              className={inputClass}
              aria-invalid={Boolean(fieldErrors.lastName)}
              aria-describedby={fieldErrors.lastName ? 'register-last-name-error' : undefined}
            />
            {fieldErrors.lastName && (
              <p id="register-last-name-error" className="mt-1 text-sm text-red-600" role="alert">
                {fieldErrors.lastName}
              </p>
            )}
          </div>
        </div>

        <div>
          <label htmlFor="register-email" className="block text-sm font-medium text-slate-700">
            Email
          </label>
          <input
            id="register-email"
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
            className={inputClass}
            aria-invalid={Boolean(fieldErrors.email)}
            aria-describedby={fieldErrors.email ? 'register-email-error' : undefined}
          />
          {fieldErrors.email && (
            <p id="register-email-error" className="mt-1 text-sm text-red-600" role="alert">
              {fieldErrors.email}
            </p>
          )}
        </div>

        <div>
          <label htmlFor="register-password" className="block text-sm font-medium text-slate-700">
            Password
          </label>
          <input
            id="register-password"
            name="password"
            type="password"
            autoComplete="new-password"
            value={password}
            onChange={(e) => {
              setPassword(e.target.value)
              if (fieldErrors.password) {
                setFieldErrors((prev) => ({ ...prev, password: undefined }))
              }
            }}
            disabled={isSubmitting}
            className={inputClass}
            aria-invalid={Boolean(fieldErrors.password)}
            aria-describedby="register-password-strength"
          />
          {fieldErrors.password && (
            <p className="mt-1 text-sm text-red-600" role="alert">
              {fieldErrors.password}
            </p>
          )}

          <div id="register-password-strength" className="mt-3 space-y-2">
            <div className="flex gap-1" aria-hidden>
              {[0, 1, 2, 3, 4].map((i) => (
                <div
                  key={i}
                  className={`h-1.5 flex-1 rounded-full transition-colors ${
                    strength.score > i ? strength.barClass : 'bg-slate-200'
                  }`}
                />
              ))}
            </div>
            <div className="flex items-center justify-between gap-2 text-xs">
              <span
                className={
                  strength.score === 0
                    ? 'text-slate-400'
                    : strength.score <= 2
                      ? 'font-medium text-red-700'
                      : strength.score === 3
                        ? 'font-medium text-amber-700'
                        : 'font-medium text-emerald-700'
                }
                aria-live="polite"
              >
                {password ? strength.label : 'Password strength'}
              </span>
            </div>
            <ul className="space-y-1 text-xs text-slate-600">
              {criteriaLabels.map((label, i) => (
                <li
                  key={label}
                  className={strength.criteria[i] ? 'text-emerald-700' : 'text-slate-500'}
                >
                  <span className="mr-1 inline-block w-3" aria-hidden>
                    {strength.criteria[i] ? '✓' : '○'}
                  </span>
                  {label}
                </li>
              ))}
            </ul>
          </div>
        </div>

        <div>
          <label htmlFor="register-confirm-password" className="block text-sm font-medium text-slate-700">
            Confirm password
          </label>
          <input
            id="register-confirm-password"
            name="confirmPassword"
            type="password"
            autoComplete="new-password"
            value={confirmPassword}
            onChange={(e) => {
              setConfirmPassword(e.target.value)
              if (fieldErrors.confirmPassword) {
                setFieldErrors((prev) => ({ ...prev, confirmPassword: undefined }))
              }
            }}
            disabled={isSubmitting}
            className={inputClass}
            aria-invalid={Boolean(fieldErrors.confirmPassword)}
            aria-describedby={
              fieldErrors.confirmPassword ? 'register-confirm-password-error' : undefined
            }
          />
          {fieldErrors.confirmPassword && (
            <p
              id="register-confirm-password-error"
              className="mt-1 text-sm text-red-600"
              role="alert"
            >
              {fieldErrors.confirmPassword}
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
              Creating account…
            </>
          ) : (
            'Create account'
          )}
        </button>
      </form>
    </div>
  )
}
