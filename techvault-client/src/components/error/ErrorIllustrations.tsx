export function Icon404({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 64 64" fill="none" aria-hidden>
      <circle cx="32" cy="32" r="28" className="stroke-slate-200" strokeWidth="2" />
      <path
        d="M20 24h8v8h-8v-8zm16 0h8v8h-8v-8zM24 40h16"
        className="stroke-slate-500"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  )
}

export function IconNetworkOff({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 64 64" fill="none" aria-hidden>
      <path
        d="M12 44l40-24M18 50l28-16M32 20v8M32 36v8"
        className="stroke-slate-300"
        strokeWidth="2"
        strokeLinecap="round"
      />
      <path
        d="M14 50L50 14"
        className="stroke-rose-500"
        strokeWidth="2.5"
        strokeLinecap="round"
      />
      <circle cx="32" cy="54" r="3" className="fill-slate-400" />
    </svg>
  )
}

export function IconAlert({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 64 64" fill="none" aria-hidden>
      <path
        d="M32 12L8 52h48L32 12z"
        className="stroke-amber-500"
        strokeWidth="2"
        strokeLinejoin="round"
      />
      <path d="M32 24v16M32 44h.01" className="stroke-amber-600" strokeWidth="2.5" strokeLinecap="round" />
    </svg>
  )
}
