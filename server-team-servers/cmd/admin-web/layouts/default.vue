<template>
    <div class="
            justify-center items-center
            transition-colors duration-500

            bg-gradient-to-br
            from-neutral-50 to-neutral-200
            dark:from-neutral-900 dark:to-neutral-800
            text-neutral-600 dark:text-neutral-300
            border-neutral-400 dark:border-neutral-700">
        
        <AppHeader />
        <div class="max-w-7xl mx-auto min-h-[calc(100vh-10rem)]">
            <Notifications class="absolute right-5" />
            <NuxtPage />
        </div>
        <AppFooter />
    </div>
</template>