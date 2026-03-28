import { isAxiosError } from 'axios'

/**
 * When true, the error is rethrown during render so an error boundary can catch it.
 * Axios/API errors (including 5xx from missing DB tables) stay in React Query so pages do not go blank.
 */
export function shouldThrowQueryErrorToBoundary(error: unknown): boolean {
  if (isAxiosError(error)) {
    return false
  }
  return true
}
