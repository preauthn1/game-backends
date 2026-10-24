<template>
  <div>
    <v-dialog
      v-model="dialog"
      dark
      hide-overlay
      persistent
      width="300"
    >
      <v-card
        color="primary"
        dark
      >
        <v-card-text>
          处理中
          <v-progress-linear
            indeterminate
            color="white"
            class="mb-0"
          ></v-progress-linear>
        </v-card-text>
      </v-card>
    </v-dialog>
  </div>
</template>

<script>
export default {
  name: 'x-processing',
  data() {
    return {
      dialog: false
    }
  },

  props: {
    isProcessing: {
      type: Boolean,
      default: () => false
    } 
  },

  watch: {
    isProcessing (val) {
      if (val) {
        this.$set(this, 'dialog', true)
      } else {
        setTimeout(() => (this.$set(this, 'dialog', false)), 500)
      }
    }
  },

  methods: {
  }
}
</script>

<style lang="scss">
</style>
