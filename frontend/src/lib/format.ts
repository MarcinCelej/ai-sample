/** Formatting helpers. Never format numbers, money or dates by string concatenation. */

const LOCALE = 'pl-PL'

const moneyFormat = new Intl.NumberFormat(LOCALE, { style: 'currency', currency: 'PLN' })
const numberFormat = new Intl.NumberFormat(LOCALE)

/** An amount in PLN, e.g. `150,50 zł`. */
export function formatMoney(amount: number): string {
  return moneyFormat.format(amount)
}

export function formatNumber(value: number): string {
  return numberFormat.format(value)
}
