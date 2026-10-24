import { useAuth } from "~~/composables"

export default defineNuxtRouteMiddleware((to, from) => {
    const { auth } = useAuth()

    if (!auth.value) {
        return navigateTo("/login")
    }

})