import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { HelmetProvider } from 'react-helmet-async'
import { BrowserRouter } from 'react-router-dom'
import { QueryClientProvider, QueryErrorResetBoundary } from '@tanstack/react-query'
import { ErrorBoundary } from 'react-error-boundary'
import 'sonner/dist/styles.css'
import './index.css'
import App from './App'
import { AuthProvider } from './auth/AuthContext'
import { AppToaster } from './components/AppToaster'
import { AppErrorBoundaryFallback } from './components/AppErrorBoundaryFallback'
import { createQueryClient } from './lib/queryClient'

const queryClient = createQueryClient()

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <QueryErrorResetBoundary>
        {({ reset }) => (
          <ErrorBoundary FallbackComponent={AppErrorBoundaryFallback} onReset={reset}>
            <AuthProvider>
              <BrowserRouter
                future={{
                  v7_startTransition: true,
                  v7_relativeSplatPath: true,
                }}
              >
                <HelmetProvider>
                  <App />
                  <AppToaster />
                </HelmetProvider>
              </BrowserRouter>
            </AuthProvider>
          </ErrorBoundary>
        )}
      </QueryErrorResetBoundary>
    </QueryClientProvider>
  </StrictMode>,
)
