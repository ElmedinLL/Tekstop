import type { FallbackProps } from 'react-error-boundary'

/** Shown when a query opts into `throwOnError` (5xx / network) and renders fail. */
export function QueryErrorFallback({ error, resetErrorBoundary }: FallbackProps) {
  return (
    <div className="mx-auto flex min-h-[50vh] max-w-lg flex-col items-center justify-center gap-4 px-4 py-16 text-center">
      <h1 className="text-xl font-semibold text-slate-900">We couldn&apos;t load this page</h1>
      <p className="text-sm text-slate-600">
        {error instanceof Error ? error.message : 'An unexpected error occurred.'}
      </p>
      <button
        type="button"
        className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
        onClick={resetErrorBoundary}
      >
        Try again
      </button>
    </div>
  )
}
