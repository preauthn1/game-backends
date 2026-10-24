import { createI18n } from 'vue-i18n'

export default defineNuxtPlugin(({ vueApp }) => {

    const i18n = createI18n({
        legacy: false,
        globalInjection: true,
        locale: 'en',
        messages: {
            en: {
                hello: 'Hello, {name}!'
            }
        },
        numberFormats: {
            'en': {
                currency: {
                    style: 'currency', currency: 'USD', notation: 'standard'
                },
                decimal: {
                    style: 'decimal', minimumFractionDigits: 2, maximumFractionDigits: 2
                },
                percent: {
                    style: 'percent', useGrouping: false
                },
            },
        },
    })

    vueApp.use(i18n)
})