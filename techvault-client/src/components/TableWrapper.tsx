import type { HTMLAttributes } from 'react'

type TableWrapperProps = HTMLAttributes<HTMLDivElement>

/**
 * P219: Responsive data tables — stacks as label/value cards under `md`.
 * Requires each &lt;td&gt; to set `data-label` matching the column (see admin pages).
 */
export function TableWrapper({ className = '', children, ...rest }: TableWrapperProps) {
  return (
    <div
      className={`techvault-table-wrapper overflow-x-auto transition-[box-shadow] duration-300 md:shadow-none ${className}`}
      {...rest}
    >
      {children}
    </div>
  )
}
