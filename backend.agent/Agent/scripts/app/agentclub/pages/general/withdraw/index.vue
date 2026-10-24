<template>
  <div>
    <v-toolbar flat>
      <v-toolbar-title>提取收益</v-toolbar-title>
      <v-divider
          class="mx-2"
          inset
          vertical
      ></v-divider>
      <v-spacer></v-spacer>
    </v-toolbar>
    <v-form ref="form">
      <v-container>
        <v-layout align-center justify-center row fill-height wrap >
          <v-flex >
            <v-text-field
              v-model="inputForm.UserID"
              label="玩家ID"
              readonly
            ></v-text-field>
          </v-flex>
          <v-flex>
            <v-text-field
              v-model="inputForm.LoginName"
              label="帐号名称"
              readonly
            ></v-text-field>
          </v-flex>
        </v-layout>
        <v-layout align-center justify-center row fill-height wrap >
          <v-flex>
            <v-text-field
              v-model="inputForm.PlatformName"
              label="平台"
              readonly
            ></v-text-field>
          </v-flex>
          <v-flex>
            <v-text-field
              v-model="inputForm.AreaName"
              label="渠道"
              readonly
            ></v-text-field>
          </v-flex>
        </v-layout>
        <v-layout align-center justify-center row fill-height wrap v-if="inputForm.Class === 1 || inputForm.Class === 2">
          <v-flex>
            <v-text-field
              v-model="inputForm.GradeName"
              label="阶级"
              readonly
            ></v-text-field>
          </v-flex>
          <v-flex>
            <v-text-field
              v-model="inputForm.DistributeRate"
              label="分成"
              readonly
            ></v-text-field>
          </v-flex>
        </v-layout>
        <v-layout justify-center column  fill-height wrap >
          <v-text-field 
          v-model="remainAmount"
          :rules="withdraw_rules" 
          type="number"
          prepend-icon="local_atm" name="money" :label="`提取余额 ( 可提取余额 : ${inputForm.RemainAmount} )`" id="money">
          </v-text-field>

          <v-flex>
            <v-alert
            :value="true"
            color="warning"
            icon="priority_high"
            outline
            >
              提取金额将移往游戏帐号
            </v-alert>
          </v-flex>
          <v-btn @click="submit">确认提取</v-btn>
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
        remaining: 0,
        remainAmount: 1,
        inputForm: {
          UserID: null,
          Class: null,
          ClassName: null,
          LoginName: null,
          PlatformName: null,
          AreaName: null,
          GradeName: null,
          DistributeRate: null,
          RemainAmount: null
        },
        withdraw_rules: [
          v =>  !!v || v === 0 || '请输入',
          v => v > 0 || '不可小于1',
          v => {
            return  v * 100 <= this.remaining || '超过提取上限'
          }
        ],
        msg: ''
      }
    },

    async mounted () {
      const item = await this.getInformation()
      this.inputForm = Object.assign({}, item)
      this.$set(this, 'remaining', _.get(item, 'CheckAmount', 0))
    },

    methods: {
      async submit () {
        if (this.$refs.form.validate()) {
          const res = await this.agentWithdraw(this.remainAmount)
          const item = await this.getInformation()
          this.inputForm = Object.assign({}, item)
          this.$set(this, 'remaining', _.get(item, 'CheckAmount', 0))
          this.$set(this, 'remainAmount', 1)
        }
      },
      clear () {
        this.$set(this.inputForm, 'RemainAmount', this.remaining)
      },
      ...mapActions(['getInformation', 'agentWithdraw'])
    }
  }
</script>

<style lang="scss" scoped>
</style>
