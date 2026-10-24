export default defineEventHandler((event) => {

    const conf = useRuntimeConfig()
    const { apiBase } = conf

    const uri = "/api/admin/lhd/"+event.context.params.rid+"/"+event.context.params.tid+"/stream"

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