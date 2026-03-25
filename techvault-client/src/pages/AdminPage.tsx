import { Link } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'

export function AdminPage() {
  const { user } = useAuth()

  return (
    <div className="mx-auto max-w-3xl px-4 py-10">
      <h1 className="text-2xl font-semibold tracking-tight text-slate-900">Admin</h1>
      <p className="mt-2 text-slate-600">
        Signed in as {user?.email} ({user?.roles.join(', ') || 'no roles'}).
      </p>
      <Link className="mt-6 inline-block text-sm font-medium text-blue-600 hover:underline" to="/">
        Home
      </Link>
    </div>
  )
}
