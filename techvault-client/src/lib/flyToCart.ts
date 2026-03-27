/** Animates a small dot from `fromEl` toward the navbar cart icon (`#nav-cart-icon`). */
export function flyToCart(fromEl: HTMLElement | null) {
  if (typeof document === 'undefined' || !fromEl) {
    return
  }
  const target = document.getElementById('nav-cart-icon')
  if (!target) {
    return
  }
  const a = fromEl.getBoundingClientRect()
  const b = target.getBoundingClientRect()
  const dot = document.createElement('div')
  dot.setAttribute('aria-hidden', 'true')
  dot.className = 'pointer-events-none fixed z-[100] h-3 w-3 rounded-full bg-blue-600 shadow-md'
  dot.style.left = `${a.left + a.width / 2 - 6}px`
  dot.style.top = `${a.top + a.height / 2 - 6}px`
  document.body.appendChild(dot)
  const dx = b.left + b.width / 2 - (a.left + a.width / 2)
  const dy = b.top + b.height / 2 - (a.top + a.height / 2)
  requestAnimationFrame(() => {
    dot.style.transition = 'transform 0.55s cubic-bezier(0.2, 0.85, 0.2, 1), opacity 0.55s ease'
    dot.style.transform = `translate(${dx}px, ${dy}px) scale(0.35)`
    dot.style.opacity = '0.85'
  })
  window.setTimeout(() => dot.remove(), 600)
}
