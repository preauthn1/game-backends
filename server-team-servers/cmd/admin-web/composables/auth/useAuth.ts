import MD5 from "crypto-js/md5"

import { APIResponse, User } from "~~/types"

export const useAuth = () => {

    const cookie = useCookie<User|null>("auth")
    const auth = useState<User|null>("auth", () => cookie.value )

    const login = async (name: string, passwd: [REDACTED] => {

        const promise = await $fetch<APIResponse>("/api/auth", {
            method: "POST",
            body: {
                username: name,
                password: [REDACTED],
            },
        })

        if (promise.success && promise.data) {
            
            const u = {
                name: promise.data.auth.Username,
                role: promise.data.auth.Role,
                access_token: [REDACTED] ,
            } as User

            auth.value = u
            cookie.value = u
        }

        return promise
    }

    const logout = () => {
        auth.value = null
        cookie.value = null

        navigateTo("/login")
    }

    return {
        auth,
        login,
        logout,
    }
}