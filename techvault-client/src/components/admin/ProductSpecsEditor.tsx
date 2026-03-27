import { useCallback, useId, useState } from 'react'

export type SpecRow = { key: string; value: string }

type ProductSpecsEditorProps = {
  rows: SpecRow[]
  onChange: (rows: SpecRow[]) => void
  error?: string
}

function rowsFromJson(text: string): SpecRow[] | null {
  try {
    const parsed = JSON.parse(text) as unknown
    if (parsed === null || typeof parsed !== 'object' || Array.isArray(parsed)) {
      return null
    }
    return Object.entries(parsed as Record<string, unknown>).map(([key, value]) => ({
      key,
      value: value == null ? '' : String(value),
    }))
  } catch {
    return null
  }
}

export function ProductSpecsEditor({ rows, onChange, error }: ProductSpecsEditorProps) {
  const baseId = useId()
  const [jsonOpen, setJsonOpen] = useState(false)
  const [jsonText, setJsonText] = useState('')

  const updateRow = useCallback(
    (index: number, patch: Partial<SpecRow>) => {
      const next = rows.map((r, i) => (i === index ? { ...r, ...patch } : r))
      onChange(next)
    },
    [rows, onChange],
  )

  const addRow = useCallback(() => {
    onChange([...rows, { key: '', value: '' }])
  }, [rows, onChange])

  const removeRow = useCallback(
    (index: number) => {
      onChange(rows.filter((_, i) => i !== index))
    },
    [rows, onChange],
  )

  const applyJson = useCallback(() => {
    const parsed = rowsFromJson(jsonText)
    if (parsed === null) {
      return false
    }
    onChange(parsed.length > 0 ? parsed : [{ key: '', value: '' }])
    setJsonOpen(false)
    setJsonText('')
    return true
  }, [jsonText, onChange])

  return (
    <div className="space-y-3">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <p className="text-xs font-medium text-slate-600">Technical specs (key / value pairs)</p>
        <button
          type="button"
          onClick={() => setJsonOpen((o) => !o)}
          className="text-xs font-medium text-blue-600 hover:underline"
        >
          {jsonOpen ? 'Hide JSON import' : 'Import from JSON'}
        </button>
      </div>

      {jsonOpen && (
        <div className="rounded-lg border border-slate-200 bg-slate-50 p-3">
          <label className="text-xs font-medium text-slate-600" htmlFor={`${baseId}-json`}>
            Paste a JSON object, e.g. {'{"RAM":"16GB","Storage":"512GB SSD"}'}
          </label>
          <textarea
            id={`${baseId}-json`}
            value={jsonText}
            onChange={(e) => setJsonText(e.target.value)}
            rows={4}
            className="mt-1 w-full rounded-md border border-slate-300 bg-white px-3 py-2 font-mono text-xs text-slate-900 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            placeholder='{"Key": "Value"}'
          />
          <button
            type="button"
            onClick={() => {
              if (!applyJson()) {
                window.alert('Invalid JSON object.')
              }
            }}
            className="mt-2 rounded-lg bg-slate-800 px-3 py-1.5 text-xs font-semibold text-white hover:bg-slate-900"
          >
            Replace table from JSON
          </button>
        </div>
      )}

      <div className="space-y-2 rounded-lg border border-slate-200 bg-white p-3">
        {rows.length === 0 && (
          <p className="text-sm text-slate-500">No spec rows. Add one or import JSON.</p>
        )}
        {rows.map((row, index) => (
          <div key={`${baseId}-row-${index}`} className="flex flex-wrap items-start gap-2">
            <input
              aria-label={`Spec name ${index + 1}`}
              value={row.key}
              onChange={(e) => updateRow(index, { key: e.target.value })}
              placeholder="Name (e.g. RAM)"
              className="min-w-[120px] flex-1 rounded-md border border-slate-300 px-2 py-1.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
            <input
              aria-label={`Spec value ${index + 1}`}
              value={row.value}
              onChange={(e) => updateRow(index, { value: e.target.value })}
              placeholder="Value"
              className="min-w-[120px] flex-[2] rounded-md border border-slate-300 px-2 py-1.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
            <button
              type="button"
              onClick={() => removeRow(index)}
              className="shrink-0 rounded-md border border-rose-200 px-2 py-1.5 text-xs font-medium text-rose-700 hover:bg-rose-50"
            >
              Remove
            </button>
          </div>
        ))}
        <button
          type="button"
          onClick={addRow}
          className="mt-1 text-sm font-medium text-blue-600 hover:underline"
        >
          + Add spec row
        </button>
      </div>
      {error ? <p className="text-xs text-rose-600">{error}</p> : null}
    </div>
  )
}
