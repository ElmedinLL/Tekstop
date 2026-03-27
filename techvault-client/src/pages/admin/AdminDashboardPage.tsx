import { useAuth } from '../../auth/AuthContext'

export function AdminDashboardPage() {
  const { user } = useAuth()

  return (
    <div>
      <h1 className="text-2xl font-semibold tracking-tight text-slate-900">Dashboard</h1>
      <p className="mt-2 text-slate-600">
        Signed in as <span className="font-medium text-slate-800">{user?.email}</span>
        {user?.roles?.length ? (
          <span className="text-slate-500"> · {user.roles.join(', ')}</span>
        ) : null}
        .
      </p>
      <p className="mt-4 text-sm text-slate-500">
        Use the sidebar to manage products, orders, users, categories, and coupons.
      </p>
    </div>
  )
}
