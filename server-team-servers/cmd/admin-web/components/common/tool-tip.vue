<template>
    <div class="leading-8 p-2
            transition-opacity duration-500 
            text-neutral-100 dark:text-neutral-700
            bg-neutral-700 dark:bg-neutral-100
            absolute top-[calc(100%+0.5rem)]
            z-20 w-32 text-center rounded-3xl
            left-1/2 -tranneutral-x-1/2
            opacity-0 group-hover:opacity-100
            before:w-2 before:h-2 before:absolute
            before:-top-1 before:left-1/2
            before:-tranneutral-x-1/2 before:rotate-45
            before:z-50
            before:bg-neutral-700 before:dark:bg-neutral-100">
        <slot />
    </div>
</template>