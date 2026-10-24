<template>
  <div class="login__content">
    <v-content>
      <v-container fluid fill-height>
        <v-layout align-center justify-center>
          <v-flex xs12 sm8 md4>
            <v-card class="elevation-12">
              <v-toolbar dark color="primary">
                <v-toolbar-title>FZ</v-toolbar-title>
                <v-spacer></v-spacer>
              </v-toolbar>
              <v-card-text>
                <v-form ref="form">
                  <v-alert
                  :value="isAlert"
                  type="error"
                  transition="scale-transition"
                  >{{errorMsg}}
                  </v-alert>
                  <v-text-field 
                  v-model="loginForm.username"
                  :rules="[accountRules.required]" 
                  prepend-icon="person" name="login" label="登录账号" type="text"></v-text-field>
                  <v-text-field 
                  v-model="loginForm.password"
                  :append-icon="isShowPS ? 'visibility_off' : 'visibility'"
                  :rules="[passwordRules.required]"
                  :type="isShowPS ? 'text' : 'password'"
                  @click:append="isShowPS = !isShowPS"
                  prepend-icon="lock" name="password" label="登录密码" id="password"></v-text-field>
                </v-form>
              </v-card-text>
              <v-card-actions>
                <v-spacer></v-spacer>
                <v-btn @click="submit" color="primary">登 录</v-btn>
                <v-btn @click="clear"> 重 置 </v-btn>
              </v-card-actions>
            </v-card>
          </v-flex>
        </v-layout>
      </v-container>
    </v-content>
  </div>
</template>

<script>
import { mapActions } from 'vuex'
import Helpers from '@/agenttotal/helpers'

export default {
  name: 'login',
  data () {
    return {
      loginForm: {
        username: '',
        password: ''
      },
      isShowPS: false,
      isAlert:false,
      errorMsg: '',
      accountRules: {
        required: value => !!value || '请输入帐号'
      },
      passwordRules: {
        required: value => !!value || '请输入密码'
      }
    }
  },
  methods: {
    submit () {
      if (this.$refs.form.validate()) {
        this.login(this.loginForm).then(data => {
          Helpers.other.token = [REDACTED]
          this.$router.push({ path: '/home', replace: true })
        }).catch((error) => {
          this.$set(this, 'errorMsg', error)
          this.$set(this, 'isAlert', true)
        })
      }
    },
    clear () {
      this.$refs.form.reset()
       this.$set(this, 'isAlert', false)
    },
    ...mapActions(['login'])
  }
}
</script>

<style lang="scss" scoped>
  .login__content {
    display: flex;
    align-items: center;
    justify-content: center;
    height: 100%;
    width: 100%;
  }
</style>


