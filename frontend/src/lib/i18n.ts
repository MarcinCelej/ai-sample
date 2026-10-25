import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'
import common from '@/locales/pl/common.json'
import courses from '@/locales/pl/courses.json'

export const defaultNS = 'common'
export const resources = { pl: { common, courses } } as const

declare module 'i18next' {
  interface CustomTypeOptions {
    defaultNS: typeof defaultNS
    resources: (typeof resources)['pl']
  }
}

void i18n.use(initReactI18next).init({
  lng: 'pl',
  fallbackLng: 'pl',
  supportedLngs: ['pl'],
  defaultNS,
  ns: Object.keys(resources.pl),
  resources,
  interpolation: { escapeValue: false }, // React escapes already.
})

export default i18n
