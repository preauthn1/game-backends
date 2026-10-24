<template>
  <div>
     <v-form ref="form">
      <v-container>
        <v-layout row wrap>
          <v-flex>
            <v-text-field
              v-model="inputForm.order_id"
              label="转错订单ID"
              :rules="[rules.required]" 
            ></v-text-field>
          </v-flex>
           <v-btn @click="submit">开始搜索</v-btn>
            <v-btn @click="clear">重置</v-btn>
        </v-layout>
      </v-container>
    </v-form>
      <v-container v-if="title" fluid fill-height>
        <v-layout align-center justify-center>
          <v-flex xs12 sm8 md8>
            <v-card class="elevation-12">
              <v-toolbar dark color="primary">
                <v-toolbar-title>{{title}}</v-toolbar-title>
                <v-spacer></v-spacer>
              </v-toolbar>
              <v-card-text>
                <v-form ref="form2">
                  <v-text-field
                    v-model="editedItem.money"
                    label="转错订单金额"
                    readonly
                  ></v-text-field>
                   <v-text-field
                    v-model="editedItem.agent_id"
                    label="订单所属商人"
                    readonly
                  ></v-text-field>
                   <v-text-field
                    v-model="editedItem.user_id"
                    :label="OrderType ? '转错商人ID' : '转错玩家ID'"
                    readonly
                  ></v-text-field>
                   <v-text-field
                    v-model="editedItem.balance"
                    :label="OrderType ? '转错商人余额' : '转错玩家余额'"
                    readonly
                  ></v-text-field>
                  <v-text-field
                    v-model="editedItem.created_at"
                    label="订单时间"
                    readonly
                  ></v-text-field>
                  <v-text-field
                    v-model="editedItem.backMoney"
                    label="金额"
                    type="number"
                  ></v-text-field>
                  <v-text-field
                    v-model="editedItem.telphoneCode"
                    label="验证码"
                  ></v-text-field>
                </v-form>
              </v-card-text>
              <v-card-actions>
                <v-spacer></v-spacer>
                <v-btn @click="orderBackSubmit" color="primary">确认回调</v-btn>
              </v-card-actions>
            </v-card>
          </v-flex>
        </v-layout>
      </v-container>
  </div>
</template>

<script>
import { mapActions } from 'vuex'

export default {
  name: 'ErrorOrderOpt',
  data() {
    return {
      title: null,
      rules: {
        required: value => !!value || '请输入订单ID'
      },
      
      inputForm: {
        order_id: null
      },
      editedItem: {
        order_id: null,
        money: null,
        agent_id: null,
        user_id: null,
        balance: null,
        created_at: null,
        backMoney: null,
        telphoneCode: null
      }
    }
  },
  computed: {
    OrderType() {
      return this.editedItem.type == 2
    },
  },
  methods: {
    submit () {
      if (this.$refs.form.validate()) {
        this.getDataFromApi()
      }
    },
    orderBackSubmit () {
      console.log(123456)
      this.agentOrderBack(this.editedItem)
    },
    clear () {
      this.$refs.form.reset()
    },
    async getDataFromApi () {
      this.getAgentUserMoney(this.inputForm).then(res => {
        this.title = res.type == 2 ? "总代理转代理" : "代理转玩家",
        this.editedItem = Object.assign({}, res)
      })
    },
    ...mapActions(['getAgentUserMoney', 'agentOrderBack'])
  }
}
</script>
<style lang="scss" scoped>
</style>
