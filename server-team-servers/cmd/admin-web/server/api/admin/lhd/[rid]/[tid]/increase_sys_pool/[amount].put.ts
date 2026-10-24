export default defineEventHandler(async (event) => {

    const conf = useRuntimeConfig()
    const { apiBase } = conf

    const uri = "/api/admin/lhd/"+event.context.params.rid+"/"+event.context.params.tid+"/increase_sys_pool/"+event.context.params.amount

    const body = await readBody(event)

    return $fetch(uri, {
        method: "PUT",
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