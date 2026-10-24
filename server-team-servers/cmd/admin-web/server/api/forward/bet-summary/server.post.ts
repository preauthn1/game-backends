export default defineEventHandler(async (event) => {

    const conf = useRuntimeConfig()
    const { apiBase } = conf

    const uri = "/api/forward/bet-summary/server"

    const body = await readBody(event)

    return $fetch(uri, {
        method: "POST",
        baseURL: apiBase,
        body: body,
        onRequest({ options }) {
            options.headers = {
                "Authorization": event.node.req.headers.authorization,
                ...options.headers,
            } as HeadersInit
        },
    })
})