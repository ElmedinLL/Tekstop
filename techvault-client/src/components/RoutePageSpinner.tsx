/** Shown while a lazy route chunk is loading (navigation between pages). */
export function RoutePageSpinner() {
  return (
    <div
      className="flex min-h-[40vh] flex-col items-center justify-center gap-3 px-4 py-16"
      role="status"
      aria-live="polite"
      aria-label="Loading page"
    >
      <div
        className="h-9 w-9 animate-spin rounded-full border-2 border-slate-200 border-t-blue-600"
        aria-hidden
      />
      <p className="text-sm text-slate-500">Loading…</p>
    </div>
  )
}
