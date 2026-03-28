import type { ReactNode } from 'react'
import { Link } from 'react-router-dom'

function LogoMark({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 32 32" fill="none" aria-hidden>
      <rect width="32" height="32" rx="8" className="fill-blue-600" />
      <path
        d="M9 11h14v2H9V11zm0 5h10v2H9v-2zm0 5h7v2H9v-2z"
        className="fill-white"
        opacity="0.95"
      />
    </svg>
  )
}

/** Full viewport layout when the main app shell is not mounted (error boundary). */
export function StandaloneErrorFrame({ children }: { children: ReactNode }) {
  return (
    <div className="flex min-h-screen flex-col bg-slate-50 text-slate-900">
      <header className="border-b border-slate-200 bg-white px-4 py-3 shadow-sm sm:py-4">
        <Link
          to="/"
          className="inline-flex items-center gap-2 rounded-lg text-lg font-semibold tracking-tight text-slate-900 outline-none ring-blue-500/0 transition hover:text-blue-700 focus-visible:ring-2 focus-visible:ring-blue-500"
        >
          <LogoMark className="h-8 w-8 shrink-0" />
          TechVault
        </Link>
      </header>
      <div className="flex flex-1 flex-col items-center justify-center px-4 py-10 sm:py-14">{children}</div>
    </div>
  )
}

type ErrorPageCardProps = {
  /** Optional label like "404" or "Error" */
  badge?: string
  icon: ReactNode
  title: string
  description: string
  actions?: ReactNode
}

/** Card used inside the storefront main column or standalone frame. */
export function ErrorPageCard({ badge, icon, title, description, actions }: ErrorPageCardProps) {
  return (
    <div className="w-full max-w-md rounded-2xl border border-slate-200 bg-white p-8 text-center shadow-sm">
      {badge ? (
        <p className="text-xs font-semibold uppercase tracking-wider text-blue-600">{badge}</p>
      ) : null}
      <div
        className={`mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-slate-100 text-slate-600 ${badge ? 'mt-3' : ''}`}
      >
        {icon}
      </div>
      <h1 className="mt-5 text-xl font-semibold tracking-tight text-slate-900">{title}</h1>
      <p className="mt-3 text-sm leading-relaxed text-slate-600">{description}</p>
      {actions ? <div className="mt-8 flex flex-col gap-3 sm:flex-row sm:flex-wrap sm:justify-center">{actions}</div> : null}
    </div>
  )
}

export function ErrorPrimaryButton({
  children,
  onClick,
  type = 'button',
}: {
  children: ReactNode
  onClick?: () => void
  type?: 'button' | 'submit'
}) {
  return (
    <button
      type={type}
      onClick={onClick}
      className="inline-flex items-center justify-center rounded-lg bg-blue-600 px-5 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-blue-700 focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2"
    >
      {children}
    </button>
  )
}

export function ErrorPrimaryLink({ to, children }: { to: string; children: ReactNode }) {
  return (
    <Link
      to={to}
      className="inline-flex items-center justify-center rounded-lg bg-blue-600 px-5 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-blue-700 focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2"
    >
      {children}
    </Link>
  )
}

export function ErrorSecondaryLink({ to, children }: { to: string; children: ReactNode }) {
  return (
    <Link
      to={to}
      className="inline-flex items-center justify-center rounded-lg border border-slate-300 bg-white px-5 py-2.5 text-sm font-semibold text-slate-800 shadow-sm transition hover:bg-slate-50 focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2"
    >
      {children}
    </Link>
  )
}
