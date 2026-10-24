<template>
    <UiAlert class="
        bg-blue-100 dark:bg-blue-400
        text-blue-600 dark:text-blue-50
        border-blue-400 dark:border-blue-300

        hover:bg-blue-200 hover:dark:bg-blue-500
        hover:text-blue-700 hover:dark:text-blue-100
        hover:border-blue-400 hover:dark:border-blue-300

        disabled:bg-blue-50 disabled:dark:bg-blue-300
        disabled:text-blue-300 disabled:dark:text-blue-100
        disabled:border-blue-200 disabled:border:text-blue-200
        ">
        <SvgInfo class="w-5 h-5 mr-4" />
        <slot />
    </UiAlert>
</template>