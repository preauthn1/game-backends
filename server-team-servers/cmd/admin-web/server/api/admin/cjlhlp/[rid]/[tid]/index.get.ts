export default defineEventHandler(async (event) => {

    const conf = useRuntimeConfig()
    const { apiBase } = conf

    const uri = "/api/admin/cjlhlp/"+event.context.params.rid+"/"+event.context.params.tid

    return $fetch(uri, {
        method: "GET",
        baseURL: apiBase,
        onRequest({ options }) {
            options.headers = {
                "Authorization": event.node.req.headers.authorization,
                ...options.headers,
            } as HeadersInit
        },
    })
})