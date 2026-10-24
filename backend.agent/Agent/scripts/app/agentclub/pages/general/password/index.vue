<template>
  <div>
    <v-toolbar flat>
      <v-toolbar-title>修改密码</v-toolbar-title>
      <v-divider
          class="mx-2"
          inset
          vertical
      ></v-divider>
      <v-spacer></v-spacer>
    </v-toolbar>
    <v-form ref="form">
      <v-container>
        <v-layout justify-center column  fill-height wrap >
          <v-text-field 
          v-model="inputForm.oldPassword"
          :rules="rules" 
          :append-icon="isShowPS1 ? 'visibility_off' : 'visibility'"
          :type="isShowPS1 ? 'text' : 'password'"
          @click:append="isShowPS1 = !isShowPS1"
          prepend-icon="lock" name="password" label="旧密码" id="password1">
          </v-text-field>
          <v-text-field 
          v-model="inputForm.newPassword"
          :rules="rules" 
          :append-icon="isShowPS2 ? 'visibility_off' : 'visibility'"
          :type="isShowPS2 ? 'text' : 'password'"
          @click:append="isShowPS2 = !isShowPS2"
          prepend-icon="lock" name="password" label="新密码" id="password2">
          </v-text-field>
          <v-text-field 
          v-on:keyup.enter="submit"
          v-model="inputForm.confirmPassword"
          :rules="confirmPasswordRules" 
          :append-icon="isShowPS3 ? 'visibility_off' : 'visibility'"
          :type="isShowPS3 ? 'text' : 'password'"
          @click:append="isShowPS3 = !isShowPS3"
          prepend-icon="lock" name="password" label="确认新密码" id="password3">
          </v-text-field>
          <v-btn @click="submit">确认修改</v-btn>
          <v-btn @click="clear">重置</v-btn>
        </v-layout>
      </v-container>
      
    </v-form>
  </div>
</template>
<script>
  import { mapActions } from 'vuex'
  export default {
    name: 'information',
    data() {
      return {
        isShowPS1: false,
        isShowPS2: false,
        isShowPS3: false,
        inputForm: {
          oldPassword: [REDACTED],
          newPassword: [REDACTED],
          confirmPassword: [REDACTED]
        },
        rules: [
          v => !!v || '请输入',
          v => v == null || v.length >= 6 || '至少六位'
        ],
        confirmPasswordRules: [
          v => !!v || '请输入',
          v => v === this.inputForm.newPassword || '新密码不一致'
        ],
        msg: ''
      }
    },
    computed: {
      errors () {
        try {
          const errors = []
          if(this.msg) errors.push(this.msg)
          return errors
        } catch (err) { return [] }
      }
    },

    methods: {
      check() {
        const newPassword = [REDACTED]
        const confirmPassword = [REDACTED]
        if(newPassword !== confirmPassword) this.msg = '新密码不一致'       
      },
      async submit () {
        if (this.$refs.form.validate()) {
          const res = await this.updatePassword(this.inputForm)
          if(res) this.$router.push({ path: '/home', replace: true })
        }
      },
      clear () {
        this.$refs.form.reset()
      },
      ...mapActions(['updatePassword'])
    }
  }
</script>

<style lang="scss" scoped>
</style>
