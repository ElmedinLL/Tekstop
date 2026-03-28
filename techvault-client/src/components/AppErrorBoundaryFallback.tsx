import type { FallbackProps } from 'react-error-boundary'
import { isNetworkError } from '../lib/errorUi'
import { IconAlert, IconNetworkOff } from './error/ErrorIllustrations'
import {
  ErrorPageCard,
  ErrorPrimaryButton,
  ErrorSecondaryLink,
  StandaloneErrorFrame,
} from './error/ErrorPageShell'

/**
 * Top-level error boundary: failed React renders, or TanStack Query errors thrown
 * to the boundary (5xx / network) with `throwOnError` + reset via QueryErrorResetBoundary.
 */
export function AppErrorBoundaryFallback({ error, resetErrorBoundary }: FallbackProps) {
  const network = isNetworkError(error)

  if (network) {
    return (
      <StandaloneErrorFrame>
        <ErrorPageCard
          badge="Connection"
          icon={<IconNetworkOff className="h-10 w-10" />}
          title="You’re offline or the server is unreachable"
          description="Check your internet connection and make sure the API is running. Then try loading the page again."
          actions={
            <>
              <ErrorPrimaryButton onClick={resetErrorBoundary}>Try again</ErrorPrimaryButton>
              <ErrorSecondaryLink to="/">Go to home</ErrorSecondaryLink>
            </>
          }
        />
      </StandaloneErrorFrame>
    )
  }

  const detail =
    error instanceof Error && error.message.trim().length > 0
      ? error.message
      : 'An unexpected error occurred. If this keeps happening, try refreshing the page.'

  return (
    <StandaloneErrorFrame>
      <ErrorPageCard
        badge="Error"
        icon={<IconAlert className="h-10 w-10" />}
        title="Something went wrong"
        description={detail}
        actions={
          <>
            <ErrorPrimaryButton onClick={resetErrorBoundary}>Try again</ErrorPrimaryButton>
            <ErrorSecondaryLink to="/">Go to home</ErrorSecondaryLink>
          </>
        }
      />
    </StandaloneErrorFrame>
  )
}
