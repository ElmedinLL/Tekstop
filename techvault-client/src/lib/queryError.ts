import { isAxiosError } from 'axios'

/** When true, the error is rethrown during render so an error boundary can catch it. */
export function shouldThrowQueryErrorToBoundary(error: unknown): boolean {
  if (isAxiosError(error)) {
    const status = error.response?.status
    if (status !== undefined && status >= 400 && status < 500) {
      return false
    }
  }
  return true
}
