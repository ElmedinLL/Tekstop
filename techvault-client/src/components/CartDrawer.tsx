import { Link } from 'react-router-dom'
import { resolveApiAssetUrl } from '../lib/assetUrl'
import { useCartQuery } from '../hooks/useCart'

function formatMoney(n: number) {
  return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD', maximumFractionDigits: 2 }).format(n)
}

type CartDrawerProps = {
  open: boolean
  onClose: () => void
}

export function CartDrawer({ open, onClose }: CartDrawerProps) {
  const { data: serverCart } = useCartQuery()
  const lines = serverCart?.lines ?? []
  const subTotal = serverCart?.subTotal ?? 0

  if (!open) {
    return null
  }

  return (
    <>
      <button
        type="button"
        className="fixed inset-0 z-40 bg-black/40"
        aria-label="Close cart"
        onClick={onClose}
      />
      <aside
        className="fixed right-0 top-0 z-50 flex h-full w-full max-w-md flex-col border-l border-slate-200 bg-white shadow-2xl animate-[slideIn_0.25s_ease-out]"
        role="dialog"
        aria-modal
        aria-labelledby="cart-drawer-title"
      >
        <div className="flex items-center justify-between border-b border-slate-200 px-4 py-3">
          <h2 id="cart-drawer-title" className="text-lg font-semibold text-slate-900">
            Your cart
          </h2>
          <button
            type="button"
            className="rounded p-1 text-slate-500 hover:bg-slate-100 hover:text-slate-800"
            onClick={onClose}
            aria-label="Close"
          >
            ✕
          </button>
        </div>

        <div className="min-h-0 flex-1 overflow-y-auto px-4 py-3">
          {lines.length === 0 ? (
            <p className="text-sm text-slate-600">Your cart is empty.</p>
          ) : (
            <ul className="space-y-3">
              {lines.map((line) => (
                <li key={`${line.productId}-${line.cartItemId}`} className="flex gap-3 border-b border-slate-100 pb-3">
                  <div className="h-16 w-16 shrink-0 overflow-hidden rounded bg-slate-100">
                    {line.imageUrl ? (
                      <img src={resolveApiAssetUrl(line.imageUrl)} alt="" className="h-full w-full object-cover" />
                    ) : (
                      <div className="flex h-full items-center justify-center text-[10px] text-slate-400">—</div>
                    )}
                  </div>
                  <div className="min-w-0 flex-1">
                    <p className="truncate text-sm font-medium text-slate-900">{line.name}</p>
                    <p className="text-xs text-slate-500">
                      {line.quantity} × {formatMoney(line.unitPrice)}
                    </p>
                  </div>
                  <p className="text-sm font-semibold text-slate-900">{formatMoney(line.lineTotal)}</p>
                </li>
              ))}
            </ul>
          )}
        </div>

        <div className="border-t border-slate-200 bg-slate-50 px-4 py-4">
          <div className="flex justify-between text-sm text-slate-600">
            <span>Subtotal</span>
            <span className="font-semibold text-slate-900">{formatMoney(subTotal)}</span>
          </div>
          <div className="mt-3 flex flex-col gap-2">
            <Link
              to="/cart"
              onClick={onClose}
              className="flex w-full justify-center rounded-lg border border-slate-300 bg-white py-2.5 text-sm font-semibold text-slate-800 hover:bg-slate-50"
            >
              View cart
            </Link>
            <Link
              to="/checkout"
              onClick={onClose}
              className="flex w-full justify-center rounded-lg bg-blue-600 py-2.5 text-sm font-semibold text-white hover:bg-blue-700"
            >
              Checkout
            </Link>
          </div>
        </div>
        <style>{`
          @keyframes slideIn {
            from { transform: translateX(100%); }
            to { transform: translateX(0); }
          }
        `}</style>
      </aside>
    </>
  )
}
