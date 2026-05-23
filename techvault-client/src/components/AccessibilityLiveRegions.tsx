/**
 * P229: Targets for asserting dynamic updates (`polite`) vs critical failures (`assertive`).
 * Wire app code to `#a11y-polite-root` via `document.getElementById(...).textContent = ...`
 * sparingly — most flows already use toast (Sonner announces).
 */
export function AccessibilityLiveRegions() {
  return (
    <>
      <div id="a11y-polite-root" className="sr-only" aria-live="polite" aria-relevant="additions text" />
      <div id="a11y-assertive-root" className="sr-only" aria-live="assertive" aria-relevant="additions text" />
    </>
  )
}
