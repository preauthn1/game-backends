<template>
    <div class="w-72 mx-auto pt-12 sm:pt-40 grid gap-5">
        <span class="font-bold text-lg">Username</span>
        <UiInput type="text" placeholder="Username" v-model="form.data.name" />
        <span class="font-bold text-lg">Password</span>
        <UiInput type="password" placeholder="Password" v-model="form.data.password" @keyup.enter="onLogin" />
        <UiButtonPrimary class="mt-5 mx-auto" @click="onLogin">Login</UiButtonPrimary>
        <UiAlertDanger v-if="form.error">{{ form.error }}</UiAlertDanger>
    </div>
</template> 

<script lang="ts" setup>

const { auth, login } = useAuth()

if (auth.value) {
    navigateTo("/")
}

const form = reactive({
    data: {
        name: ref(""),
        password: [REDACTED]""),
    },
    error: "",
    pending: false,
})

const onLogin = async () => {

    try {
        form.error = ""
        form.pending = true

        await login(form.data.name, form.data.password)

        navigateTo("/")

    } catch (err: any) {

        form.error = err.data.message

        if (err.data.data) {
            form.error = err.data.data.message
        }
    } finally {
        form.pending = false
    }

}

</script>