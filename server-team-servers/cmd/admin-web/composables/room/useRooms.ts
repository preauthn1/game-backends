import type { RoomInfo, APIResponse } from '~~/types'

export const useRooms = () => {

    const { auth } = useAuth()

    const fetchAll = () => useFetch<RoomInfo[]>("/api/room", {
        method: "GET",
        onRequest({ options }) {
            options.headers = {
                "Authorization": auth.value?.access_token,
                ...options.headers
            } as HeadersInit
        },
        onResponse({ response }) {
            response._data = response._data.data
        },
    })

    return {
        fetchAll,
    }
}