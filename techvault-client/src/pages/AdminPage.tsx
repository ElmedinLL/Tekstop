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
      <div className="mt-6 flex flex-wrap gap-4">
        <Link
          className="rounded-md bg-blue-600 px-4 py-2 text-sm font-semibold text-white shadow-sm hover:bg-blue-700"
          to="/admin/orders"
        >
          Orders
        </Link>
        <Link className="text-sm font-medium text-blue-600 hover:underline" to="/">
          Home
        </Link>
      </div>
    </div>
  )
}
