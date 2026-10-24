<template>
    <div class="notifications">
        <transition-group name="list">
            <div class="notification" v-for="item in state" :key="item.key" :class="{
                'notification-primary': item.color == 'primary',
                'notification-info': item.color == 'info',
                'notification-success': item.color == 'success',
                'notification-warn': item.color == 'warning',
                'notification-danger': item.color == 'error',
            }">
                <div>
                    <svg xmlns="http://example.invalid/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-10 h-10">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v3.75m9-.75a9 9 0 11-18 0 9 9 0 0118 0zm-9 3.75h.008v.008H12v-.008z" />
                    </svg>
                </div>
                <div class="pr-8">
                    <div v-if="item.title" class="text-lg font-bold">{{ item.title }}</div>
                    <div v-if="item.text" class="text-sm">{{ item.text }}</div>
                </div>
                
                <span class="absolute right-1 top-1 cursor-pointer" @click="remove(item.key)">
                    <svg xmlns="http://example.invalid/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-6 h-6">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M9.75 9.75l4.5 4.5m0-4.5l-4.5 4.5M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                </span>

            </div>
        </transition-group>
    </div>
</template>

<script lang="ts" setup>
    const { state, remove } = useNotification()
</script>

<style lang="postcss" scoped>

    .notifications {
        .notification {
            @apply
                max-w-xl p-3 mb-3
                sticky top-0

                rounded-lg

                flex items-center justify-start gap-3

                bg-gradient-to-br
                from-cyan-50 to-cyan-100
                dark:from-neutral-600 dark:to-neutral-700
            ;

            button {
                @apply
                    hover:rotate-90
                    ml-auto
                ;
            }

            &.notification-primary {
                @apply
                    from-fuchsia-100 to-fuchsia-200
                    dark:from-fuchsia-400 dark:to-fuchsia-500
                ;
            }

            &.notification-info {
                @apply
                    from-sky-100 to-sky-200
                    dark:from-sky-400 dark:to-sky-500
                ;
            }

            &.notification-success {
                @apply
                    from-emerald-100 to-emerald-200
                    dark:from-emerald-400 dark:to-emerald-500
                ;
            }

            &.notification-warn {
                @apply
                    from-orange-100 to-orange-200
                    dark:from-orange-400 dark:to-orange-500
                ;
            }

            &.notification-danger {
                @apply
                    from-rose-100 to-rose-200
                    dark:from-rose-400 dark:to-rose-500
                ;
            }

        }

        .list-enter-active,
        .list-leave-active {
            transition: all 0.5s ease;
        }
        .list-enter-from,
        .list-leave-to {
            opacity: 0;
            transform: translateX(30px);
        }
    }

</style>