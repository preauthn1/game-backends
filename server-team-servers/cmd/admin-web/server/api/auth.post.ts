export default defineEventHandler(async (event) => {

    const conf = useRuntimeConfig()
    const { apiBase } = conf

    const uri = "/auth"

    const body = await readBody(event)

    return $fetch(uri, {
        method: "POST",
        baseURL: apiBase,
        body: body,
    })
})