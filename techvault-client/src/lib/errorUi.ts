import { isAxiosError } from 'axios'

/** True when the request failed before a normal HTTP response (offline, DNS, CORS, timeout, etc.). */
export function isNetworkError(error: unknown): boolean {
  if (!isAxiosError(error)) {
    return false
  }
  if (error.code === 'ERR_NETWORK' || error.code === 'ECONNABORTED') {
    return true
  }
  if (error.response == null) {
    return true
  }
  return false
}
