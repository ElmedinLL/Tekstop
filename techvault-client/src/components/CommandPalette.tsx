import { useEffect, useMemo, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'

export type PaletteCommand = {
  id: string
  label: string
  path: string
  keywords?: string
}

/** P218: ⌘K / Ctrl+K — quick jumps to admin areas. */
export function CommandPalette({
  commands = DEFAULT_ADMIN_COMMANDS,
}: {
  commands?: readonly PaletteCommand[]
}) {
  const [open, setOpen] = useState(false)
  const [q, setQ] = useState('')
  const [activeIndex, setActiveIndex] = useState(0)
  const navigate = useNavigate()
  const inputRef = useRef<HTMLInputElement>(null)
  const listRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if ((e.metaKey || e.ctrlKey) && e.key.toLowerCase() === 'k') {
        e.preventDefault()
        setOpen((v) => !v)
      }
      if (e.key === 'Escape' && open) {
        e.preventDefault()
        setOpen(false)
      }
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [open])

  useEffect(() => {
    if (open) {
      setQ('')
      setActiveIndex(0)
      requestAnimationFrame(() => inputRef.current?.focus())
    }
  }, [open])

  const filtered = useMemo(() => {
    const term = q.trim().toLowerCase()
    if (!term) return [...commands]
    return commands.filter(
      (c) =>
        c.label.toLowerCase().includes(term) ||
        (c.keywords && c.keywords.toLowerCase().includes(term)) ||
        c.path.toLowerCase().includes(term),
    )
  }, [commands, q])

  useEffect(() => {
    if (activeIndex >= filtered.length) setActiveIndex(0)
  }, [activeIndex, filtered.length])

  const runSelect = (index: number) => {
    const cmd = filtered[index]
    if (!cmd) return
    navigate(cmd.path)
    setOpen(false)
  }

  if (!open) return null

  return (
    <div className="fixed inset-0 z-[90] flex items-start justify-center bg-slate-900/50 p-4 pt-[12vh] backdrop-blur-sm">
      <div
        role="dialog"
        aria-modal="true"
        aria-label="Command palette"
        className="w-full max-w-lg overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-2xl dark:border-slate-700 dark:bg-slate-950"
        onMouseDown={(e) => e.stopPropagation()}
      >
        <label htmlFor="command-palette-input" className="sr-only">
          Search admin pages
        </label>
        <div className="border-b border-slate-100 px-3 py-3 dark:border-slate-800">
          <input
            ref={inputRef}
            id="command-palette-input"
            value={q}
            onChange={(e) => {
              setQ(e.target.value)
              setActiveIndex(0)
            }}
            placeholder="Search admin… (arrows navigate, Enter opens, Esc closes)"
            className="w-full rounded-xl border border-slate-200 bg-slate-50 px-4 py-2.5 text-sm text-slate-900 outline-none ring-blue-600/40 placeholder:text-slate-400 focus:border-blue-400 focus:bg-white focus:ring-4 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100 dark:focus:border-blue-600"
            autoComplete="off"
            aria-controls="command-palette-results"
            aria-activedescendant={
              filtered[activeIndex] ? `palette-cmd-${filtered[activeIndex].id}` : undefined
            }
            onKeyDown={(e) => {
              if (e.key === 'ArrowDown') {
                e.preventDefault()
                setActiveIndex((i) => (filtered.length === 0 ? 0 : (i + 1) % filtered.length))
              } else if (e.key === 'ArrowUp') {
                e.preventDefault()
                setActiveIndex((i) =>
                  filtered.length === 0 ? 0 : (i - 1 + filtered.length) % filtered.length,
                )
              } else if (e.key === 'Enter') {
                e.preventDefault()
                runSelect(activeIndex)
              } else if (e.key === 'Escape') {
                e.preventDefault()
                setOpen(false)
              }
            }}
          />
        </div>
        <div
          ref={listRef}
          id="command-palette-results"
          role="listbox"
          aria-label="Results"
          className="max-h-72 overflow-y-auto py-1"
        >
          {filtered.map((cmd, idx) => {
            const sel = idx === activeIndex
            return (
              <button
                key={cmd.id}
                type="button"
                id={`palette-cmd-${cmd.id}`}
                role="option"
                aria-selected={sel}
                className={`flex w-full items-center justify-between gap-3 px-4 py-2.5 text-left text-sm outline-none transition ${
                  sel
                    ? 'bg-blue-50 text-blue-950 dark:bg-blue-950/80 dark:text-blue-50'
                    : 'text-slate-800 hover:bg-slate-50 dark:text-slate-200 dark:hover:bg-slate-900'
                }`}
                onMouseEnter={() => setActiveIndex(idx)}
                onClick={() => runSelect(idx)}
              >
                <span className="font-medium">{cmd.label}</span>
                <span className="text-xs text-slate-400">{cmd.path}</span>
              </button>
            )
          })}
          {filtered.length === 0 ? (
            <p className="px-4 py-6 text-center text-sm text-slate-500">No matches.</p>
          ) : null}
        </div>
      </div>
      <button
        type="button"
        className="absolute inset-0 -z-10 cursor-default bg-transparent"
        aria-label="Close command palette"
        onClick={() => setOpen(false)}
      />
    </div>
  )
}

const DEFAULT_ADMIN_COMMANDS: readonly PaletteCommand[] = [
  { id: 'dash', label: 'Admin dashboard', path: '/admin', keywords: 'home stats' },
  { id: 'products', label: 'Products', path: '/admin/products' },
  { id: 'new-product', label: 'New product', path: '/admin/products/new' },
  { id: 'orders', label: 'Orders', path: '/admin/orders' },
  { id: 'users', label: 'Users', path: '/admin/users' },
  { id: 'categories', label: 'Categories', path: '/admin/categories' },
  { id: 'coupons', label: 'Coupons', path: '/admin/coupons' },
]
