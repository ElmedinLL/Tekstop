import { Link } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'

export function AccountPage() {
  const { user, logout } = useAuth()

  return (
    <div className="mx-auto max-w-3xl px-4 py-10">
      <h1 className="text-2xl font-semibold tracking-tight text-slate-900">Account</h1>
      <p className="mt-2 text-slate-600">
        Signed in as{' '}
        <span className="font-medium text-slate-900">
          {user?.firstName} {user?.lastName}
        </span>{' '}
        ({user?.email}).
      </p>
      <div className="mt-6 flex flex-wrap gap-3">
        <button
          type="button"
          onClick={() => void logout()}
          className="rounded-md border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 shadow-sm hover:bg-slate-50"
        >
          Sign out
        </button>
        <Link className="rounded-md bg-blue-600 px-4 py-2 text-sm font-semibold text-white shadow-sm hover:bg-blue-700" to="/">
          Home
        </Link>
      </div>
    </div>
  )
}
