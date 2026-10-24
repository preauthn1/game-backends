<template>
  <div>
    <v-toolbar flat>
      <v-toolbar-title>添加代理</v-toolbar-title>
      <v-divider
          class="mx-2"
          inset
          vertical
      ></v-divider>
      <v-spacer></v-spacer>
    </v-toolbar>
    <v-form ref="form">
      <v-container>
        <v-layout justify-center column  fill-height >    
          <v-flex v-if="!IsAdmin">
            <v-text-field
              v-model="inputForm.UserID"
              label="游戏ID"
              :rules="rules" 
            ></v-text-field>
          </v-flex>
          <v-flex>
            <v-select
            v-model="inputForm.PlatformID"
            :items="platformItems"
            item-text="text"
            item-value="value"
            label="平台"
            auro
            dark
            ></v-select>
          </v-flex>     
          <v-flex>
            <v-text-field
              v-model="inputForm.LoginName"
              label="帐号名称"
              :rules="rules" 
            ></v-text-field>
          </v-flex>
          <v-flex>
            <v-text-field v-model="inputForm.Tel" :rules="tel_rules" label="电话"></v-text-field>
          </v-flex>
          <v-flex>
            <v-text-field v-model="inputForm.Weixin" :rules="weixin_rules" label="微信"></v-text-field>
          </v-flex>
          <v-flex>
            <v-text-field v-model="inputForm.Email" :rules="email_rules" label="信箱"></v-text-field>
          </v-flex>
          <v-flex>
            <v-text-field v-model="inputForm.QQ" :rules="qq_rules" label="QQ"></v-text-field>
          </v-flex>
          <v-flex>
            <v-text-field v-if="!IsAdmin" v-model="inputForm.vCode" :rules="rules"  label="验证码"></v-text-field>
          </v-flex>
          
          <v-btn v-if="!IsAdmin"
          :disabled="isDisabled" 
          @click="VerificationCode">
            {{buttonName}}
          </v-btn>
          <v-btn @click="submit">新增代理</v-btn>
          <v-btn @click="clear">重置</v-btn>
        </v-layout>
      </v-container>
    </v-form>
  </div>
</template>
<script>
  import { mapActions, mapGetters } from 'vuex'
  export default {
    name: 'information',
    data() {
      return {
        platformItems: [],
        readonly: true,
        buttonName: "发送验证码",
				isDisabled: false,
				time: 60,
        inputForm: {
          UserID: null,
          PlatformID: null,
          LoginName: null,
          Email: null,
          Tel: null,
          Weixin: null,
          QQ: null,
          DownloadUrl: null,
          vCode: null
        },
        rules: [
          v =>  !!v || '请输入'
        ],
        tel_rules: [
          v =>  !!v || '请输入',
          v => {
            const mPattern = /^1[345789]\d{9}$/;
            return mPattern.test(v)|| '请输入正确的手机号'
          }
        ],
        email_rules: [
          v => {
            if (v === null || v.length === 0) return true
            const ePattern = /^([A-Za-z0-9_\-\.])+\@([A-Za-z0-9_\-\.])+\.([A-Za-z]{2,4})$/
            return ePattern.test(v)|| '请输入正确的信箱'
          }
        ],
        weixin_rules: [
          v =>  !!v || '请输入'
        ],
        qq_rules: [
          v => {
            if (v === null || v.length === 0) return true
            const qqPattern = /^[1-9][0-9]{4,10}$/;
            return qqPattern.test(v)|| '请输入正确的QQ'
          }
        ]
      }
    },

    computed: {
      ...mapGetters(['IsAdmin'])
    },

    async mounted () {
      const platform = await this.getPlatformItems()
      this.$set(this, 'platformItems', platform)
    },

    methods: {
      async VerificationCode () {
        let me = this;
        me.isDisabled = true;
        let interval = window.setInterval(function() {
          me.buttonName = '（' + me.time + '秒）后重新发送';
          --me.time;
          if(me.time < 0) {
            me.buttonName = "重新发送";
            me.time = 10;
            me.isDisabled = false;
            window.clearInterval(interval);
          }
        }, 1000);
        
        const res = await this.sendVerificationCode()
      },

      async submit () {
        if (this.$refs.form.validate()) {
          const res = await this.createAgent(this.inputForm)
          if (res) this.$refs.form.reset()
        }
      },
      clear () {
        this.$refs.form.reset()
      },

      ...mapActions(['getPlatformItems', 'createAgent', 'sendVerificationCode'])
    }
  }
</script>

<style lang="scss" scoped>
  .progress_content {
    font-size: 18px;
    text-align: center;
  }
  .progress_title{
    padding: 8%;
  }
</style>
