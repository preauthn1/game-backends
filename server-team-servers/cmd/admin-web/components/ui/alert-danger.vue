<template>
    <UiAlert class="
        bg-red-100 dark:bg-red-400
        text-red-600 dark:text-red-50
        border-red-400 dark:border-red-300

        hover:bg-red-200 hover:dark:bg-red-500
        hover:text-red-700 hover:dark:text-red-100
        hover:border-red-400 hover:dark:border-red-300

        disabled:bg-red-50 disabled:dark:bg-red-300
        disabled:text-red-300 disabled:dark:text-red-100
        disabled:border-red-200 disabled:dark:border-red-200
        ">
        <SvgError class="w-5 h-5 mr-4" />
        <slot />
    </UiAlert>
</template>