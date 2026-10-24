export default defineEventHandler(async (event) => {

    const conf = useRuntimeConfig()
    const { apiBase } = conf

    const uri = "/api/admin/lhd/"+event.context.params.rid+"/"+event.context.params.tid+"/assign_next_award/"+event.context.params.award

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