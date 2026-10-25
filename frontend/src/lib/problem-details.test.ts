import { isProblemDetails } from './problem-details'

describe('isProblemDetails', () => {
  it.each([{ status: 400 }, { title: 'Bad' }, { code: 'x' }])('accepts %o', (value) => {
    expect(isProblemDetails(value)).toBe(true)
  })

  it.each([null, undefined, 'text', 42, {}, { id: 1 }])('rejects %o', (value) => {
    expect(isProblemDetails(value)).toBe(false)
  })
})
