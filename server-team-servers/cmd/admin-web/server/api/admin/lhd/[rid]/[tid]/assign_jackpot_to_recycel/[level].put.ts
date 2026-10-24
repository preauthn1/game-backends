export default defineEventHandler(async (event) => {

    const conf = useRuntimeConfig()
    const { apiBase } = conf

    const uri = "/api/admin/lhd/"+event.context.params.rid+"/"+event.context.params.tid+"/assign_jackpot_to_recycel/"+event.context.params.level

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