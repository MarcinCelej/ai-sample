import { formatMoney, formatNumber } from './format'

// Intl uses non-breaking spaces as separators in pl-PL.
const normalize = (text: string) => text.replace(/\s/g, ' ')

describe('format', () => {
  it('formats money as PLN in Polish notation', () => {
    expect(normalize(formatMoney(1234.5))).toBe('1234,50 zł')
    expect(normalize(formatMoney(0))).toBe('0,00 zł')
  })

  it('formats numbers in Polish notation', () => {
    expect(normalize(formatNumber(12345.6))).toBe('12 345,6')
  })
})
