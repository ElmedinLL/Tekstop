export type PasswordStrengthResult = {
  /** Number of criteria satisfied (0–5). */
  score: number
  label: string
  barClass: string
  /** Which of the five criteria are met (for segment UI). */
  criteria: boolean[]
}

const CRITERIA_LABELS = [
  'At least 8 characters',
  'Lowercase letter',
  'Uppercase letter',
  'Number',
  'Symbol',
] as const

function computeCriteria(password: string): boolean[] {
  return [
    password.length >= 8,
    /[a-z]/.test(password),
    /[A-Z]/.test(password),
    /[0-9]/.test(password),
    /[^A-Za-z0-9]/.test(password),
  ]
}

const LABEL_BY_SCORE = ['', 'Very weak', 'Weak', 'Fair', 'Good', 'Strong'] as const

const BAR_CLASS_BY_SCORE = [
  'bg-slate-200',
  'bg-red-500',
  'bg-orange-500',
  'bg-amber-500',
  'bg-lime-500',
  'bg-emerald-600',
] as const

export function evaluatePasswordStrength(password: string): PasswordStrengthResult {
  if (!password) {
    return {
      score: 0,
      label: '',
      barClass: BAR_CLASS_BY_SCORE[0],
      criteria: computeCriteria(''),
    }
  }

  const criteria = computeCriteria(password)
  const score = criteria.filter(Boolean).length
  return {
    score,
    label: LABEL_BY_SCORE[score],
    barClass: BAR_CLASS_BY_SCORE[score],
    criteria,
  }
}

export function passwordStrengthCriteriaLabels(): readonly string[] {
  return CRITERIA_LABELS
}
