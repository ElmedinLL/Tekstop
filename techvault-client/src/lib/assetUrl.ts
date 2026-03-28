/**
 * Resolves relative API static paths (e.g. `/images/...`) against the API origin when
 * `VITE_API_BASE_URL` is an absolute URL (e.g. `http://localhost:5000/api/v1`).
 */
export function resolveApiAssetUrl(path: string | null | undefined): string {
  if (path == null || path === '') {
    return ''
  }
  if (path.startsWith('http://') || path.startsWith('https://')) {
    return path
  }
  const apiBase = import.meta.env.VITE_API_BASE_URL ?? ''
  if (apiBase.startsWith('http://') || apiBase.startsWith('https://')) {
    const origin = new URL(apiBase).origin
    const normalized = path.startsWith('/') ? path : `/${path}`
    return `${origin}${normalized}`
  }
  return path
}
