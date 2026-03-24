import { useQuery } from '@tanstack/react-query'
import { api } from '../lib/api'

async function fetchHealth(): Promise<{ status: string }> {
  try {
    const { data } = await api.get<{ status: string }>('/health')
    return data
  } catch {
    return { status: 'No API at VITE_API_BASE_URL (expected until backend is running)' }
  }
}

export function HomePage() {
  const { data, isPending } = useQuery({
    queryKey: ['health'],
    queryFn: fetchHealth,
  })

  return (
    <div className="mx-auto max-w-3xl px-4 py-10">
      <h1 className="text-3xl font-semibold tracking-tight">techvault-client</h1>
      <p className="mt-2 text-slate-600">
        React 18, Vite, Tailwind CSS v3, React Router v6, Axios, and TanStack Query.
      </p>
      <section className="mt-8 rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
        <h2 className="text-sm font-medium text-slate-500">Sample React Query + Axios</h2>
        <p className="mt-2 font-mono text-sm">
          {isPending && 'Loading…'}
          {!isPending && data && `GET /health → ${JSON.stringify(data)}`}
        </p>
        <p className="mt-2 text-xs text-slate-500">
          Set <code className="rounded bg-slate-100 px-1">VITE_API_BASE_URL</code> in{' '}
          <code className="rounded bg-slate-100 px-1">.env</code> (see <code className="rounded bg-slate-100 px-1">.env.example</code>).
        </p>
      </section>
    </div>
  )
}
