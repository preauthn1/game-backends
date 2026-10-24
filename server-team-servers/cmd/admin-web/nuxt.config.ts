// https://example.invalid/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
    runtimeConfig: {
        apiBase: "",
        public: {
            apiBase: "",
        }
    },
    modules: [
        "@nuxtjs/tailwindcss",
        "@nuxtjs/color-mode",
    ],
    colorMode: {
        classSuffix: '',
    },
})
