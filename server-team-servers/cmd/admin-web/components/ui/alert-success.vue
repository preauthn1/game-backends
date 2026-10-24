<template>
    <UiAlert class="
        bg-green-100 dark:bg-green-400
        text-green-600 dark:text-green-50
        border-green-400 dark:border-green-300

        hover:bg-green-200 hover:dark:bg-green-500
        hover:text-green-700 hover:dark:text-green-100
        hover:border-green-400 hover:dark:border-green-300

        disabled:bg-green-50 disabled:dark:bg-green-300
        disabled:text-green-300 disabled:dark:text-green-100
        disabled:border-green-200 disabled:dark:border-green-200
        ">
        <SvgSuccess class="w-5 h-5 mr-4" />
        <slot />
    </UiAlert>
</template>