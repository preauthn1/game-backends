<template>
    <UiAlert class="
        bg-orange-100 dark:bg-orange-400
        text-orange-600 dark:text-orange-50
        border-orange-400 dark:border-orange-300

        hover:bg-orange-200 hover:dark:bg-orange-500
        hover:text-orange-700 hover:dark:text-orange-100
        hover:border-orange-400 hover:dark:border-orange-300

        disabled:bg-orange-50 disabled:dark:bg-orange-300
        disabled:text-orange-300 disabled:dark:text-orange-100
        disabled:border-orange-200 disabled:dark:border-orange-200
        ">
        <SvgWarn class="w-5 h-5 mr-4" />
        <slot />
    </UiAlert>
</template>