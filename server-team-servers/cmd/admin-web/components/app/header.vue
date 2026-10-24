<template>
    <header class="sticky top-0 z-40 h-20 w-full backdrop-blur flex-none">
        <div class="max-w-7xl mx-auto">
            <div class="py-4 border-b border-neutral-400 dark:border-neutral-700 lg:px-8 mx-4">
                <div class="relative flex items-center">
                    <NuxtLink to="/">
                        <span class="text-xl mr-1 font-bold">target</span>
                        <span class="text-xl mr-1 font-bold">Admin</span>
                        <span class="hidden sm:inline text-xl mr-1 font-bold">Dashboard</span>
                    </NuxtLink>
                    <div class="relative items-center ml-auto">
                        <nav class="text-sm leading-6 font-semibold text-neutral-700 dark:text-neutral-200"></nav>
                        <div class="flex items-center border-l border-neutral-400 dark:border-neutral-700 ml-5 pl-5">
                            <ClientOnly>
                                <Switch class="relative inline-flex rounded-full w-12 h-7 p-1
                                bg-neutral-200 dark:bg-neutral-700" v-model="enabled">
                                    <span class="sr-only">Enable notifications</span>
                                    <span :class="enabled ? 'translate-x-5' : 'translate-x-0.5'"
                                        class="h-4 w-4 transform transition duration-400 ">
                                        <SvgSun v-if="!enabled"
                                            class="rounded-full w-5 h-5 absolute p-1 bg-neutral-50 dark:bg-neutral-800" />
                                        <SvgMoon v-else
                                            class="rounded-full w-5 h-5 absolute p-1 bg-neutral-50 dark:bg-neutral-800" />
                                    </span>
                                </Switch>
                            </ClientOnly>

                            <div v-if="route.path != '/login'">
                                <NuxtLink v-if="auth == null" to="/login">
                                    <UiButton class="ml-5 px-3 py-0.5">Login</UiButton>
                                </NuxtLink>
                                <UiButtonDanger v-else class="ml-5 px-3 py-0.5" @click="onLogout">
                                    Logout
                                </UiButtonDanger>
                            </div>
                        </div>
                    </div>
                </div>

            </div>
        </div>
    </header>
</template>

<script lang="ts" setup>

import { Switch } from "@headlessui/vue"

const enabled = ref(useColorMode().preference == "dark")

watch(enabled, (v) => {
    const color = v ? "dark" : "light"
    useColorMode().preference = color
})

const { auth, logout } = useAuth()

const route = useRoute()

const onLogout = () => {
    logout()
}

</script>