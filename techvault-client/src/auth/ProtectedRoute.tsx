import type { ReactNode } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from './AuthContext'

type ProtectedRouteProps = {
  children: ReactNode
}

/** Sends guests to `/login` with `state.from` so `LoginPage` can return them here after sign-in. */
export function ProtectedRoute({ children }: ProtectedRouteProps) {
  const { isAuthenticated, isInitializing } = useAuth()
  const location = useLocation()

  if (isInitializing) {
    return (
      <div className="mx-auto flex min-h-[40vh] max-w-md items-center justify-center px-4">
        <p className="text-sm text-slate-500" role="status">
          Checking session…
        </p>
      </div>
    )
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />
  }

  return <>{children}</>
}
