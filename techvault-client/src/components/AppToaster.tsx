import { Toaster } from 'sonner'

/**
 * Global Sonner instance — Tailwind-friendly styling aligned with the app shell.
 */
export function AppToaster() {
  return (
    <Toaster
      position="top-center"
      richColors
      closeButton
      duration={3200}
      visibleToasts={5}
      toastOptions={{
        classNames: {
          toast:
            '!rounded-xl !border !border-slate-200/90 !bg-white/95 !text-slate-900 !shadow-lg !backdrop-blur-sm',
          title: '!text-sm !font-semibold !text-slate-900',
          description: '!text-sm !text-slate-600',
          success: '!border-emerald-200/80 !bg-emerald-50/95',
          error: '!border-rose-200/80 !bg-rose-50/95',
          warning: '!border-amber-200/80 !bg-amber-50/95',
          info: '!border-blue-200/80 !bg-blue-50/95',
          closeButton: '!text-slate-500 hover:!bg-slate-100',
        },
      }}
    />
  )
}
