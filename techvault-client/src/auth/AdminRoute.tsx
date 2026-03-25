import type { ReactNode } from 'react'
import { Navigate } from 'react-router-dom'
import { useAuth } from './AuthContext'
import { ProtectedRoute } from './ProtectedRoute'

function hasAdminRole(roles: string[] | undefined): boolean {
  return roles?.some((r) => r.toLowerCase() === 'admin') ?? false
}

function RequireAdminRole({ children }: { children: ReactNode }) {
  const { user } = useAuth()

  if (!hasAdminRole(user?.roles)) {
    return <Navigate to="/" replace />
  }

  return <>{children}</>
}

/** Requires sign-in (via `ProtectedRoute`) and an Admin role; otherwise redirects home. */
export function AdminRoute({ children }: { children: ReactNode }) {
  return (
    <ProtectedRoute>
      <RequireAdminRole>{children}</RequireAdminRole>
    </ProtectedRoute>
  )
}
