<template>
  <div>
    <v-snackbar
    v-model="isShow"
    :timeout="3000"
    :color="color"
    top
  >
    {{ errorMsg }}
    <v-btn
      color="grey lighten-4"
      flat
      @click="snackbar = false"
    >
      Close
    </v-btn>
  </v-snackbar>
  <router-view></router-view>
  </div>
</template>

<script>
import { mapState, mapActions } from 'vuex'
  
export default {
  name: 'app',

  data () {
    return {
      isShow: false
    }
  },

  watch: {
    snackbar (val, oldVal) {
      try {
        if(val) this.$set(this, 'isShow', val)
      } catch (err) {}
    },
    isShow (val, oldVal) {
      try {
        if( !val) this.closeSnackbar()
       
      } catch (err) {}
    }
  },

  computed: {
    ...mapState({
      snackbar: state => state.auth.snackbar,
      color:  state => state.auth.snackbarColor,
      errorMsg: state => state.auth.errorMsg
    })
  },

  methods: {
    ...mapActions(['closeSnackbar'])
  }
}
</script>

<style lang="scss" scoped>
</style>
