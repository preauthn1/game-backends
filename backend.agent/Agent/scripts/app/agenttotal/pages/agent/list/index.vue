<template>
  <div>      
    <v-alert
      :value="isShow"
      :type="isSuccess ? 'success' : 'error'"
    >
      {{message}}
    </v-alert>
    <v-toolbar flat color="white">
      <v-toolbar-title>代理商查询</v-toolbar-title>
      <v-divider
          class="mx-2"
          inset
          vertical
      ></v-divider>
      <v-spacer></v-spacer>
      <v-dialog v-model="dialog" max-width="600px">
        <v-btn slot="activator" color="primary" dark class="mb-2">添加代理</v-btn>
        <v-card>
          <v-card-title>
            <span class="headline"></span>
          </v-card-title>
          <v-card-text>
            <v-form ref="form2">
            <v-container grid-list-md>
              <v-layout wrap>
                <v-flex xs12 sm6 md4>
                  <v-text-field 
                  v-model="editedItem.name" 
                  label="代理名称"
                  :rules="[accountRules.required]"></v-text-field>
                </v-flex>
                <v-flex xs12 sm6 md4>
                  <v-text-field 
                  v-model="editedItem.player_id" 
                  label="绑定游戏ID"
                  :rules="[accountRules.required]"></v-text-field>
                </v-flex>
                <v-flex xs12 sm6 md4>
                  <v-text-field 
                  v-model="editedItem.min_money" 
                  label="最低代充金额"
                  :rules="[accountRules.required]"></v-text-field>
                </v-flex>
                <v-flex xs12 sm6 md4>
                  <v-text-field v-model="editedItem.qq" label="QQ"></v-text-field>
                </v-flex>
                <v-flex xs12 sm6 md4>
                  <v-text-field v-model="editedItem.weixin" label="微信"></v-text-field>
                </v-flex>
                <v-flex xs12 sm6 md4>
                  <v-text-field v-model="editedItem.alipay" label="支付宝"></v-text-field>
                </v-flex>
                <v-flex xs12 sm6 md4>
                  <v-text-field 
                  v-model="editedItem.telphone" 
                  label="手机号"
                  :rules="[accountRules.required]"></v-text-field>
                </v-flex>
               
                <v-flex xs12 sm6 md4>
                  <v-text-field v-model="editedItem.remark" label="备注"></v-text-field>
                </v-flex>
                <v-flex xs12 sm6 md4>
                  <v-text-field v-model="editedItem.auto_reply" label="自动回复"></v-text-field>
                </v-flex>
                
                <v-flex xs12 sm6 md4>
                  <v-switch label="快速转账" v-model="editedItem.transfer"></v-switch>
                </v-flex>
                <v-flex xs12 sm6 md4>
                  <v-select
                    v-model="editedItem.adLevel"
                    :items="adLevel"
                    item-text="text"
                    item-value="value"
                    label="代理广告"
                    ></v-select>
                </v-flex>
              </v-layout>
              <v-layout wrap>
                <v-flex v-if="isNew" >
                  <v-text-field v-model="editedItem.mailContent" label="聊天说明"></v-text-field>
                </v-flex>
              </v-layout>
            </v-container>
            </v-form>
          </v-card-text>

          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="blue darken-1" flat @click.native="close">Cancel</v-btn>
            <v-btn color="blue darken-1" flat @click.native="save">Save</v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>
    </v-toolbar>
    <v-form ref="form">
      <v-container>
        <v-layout row wrap>
          <v-flex>
            <v-text-field
              v-model="inputForm.agent_id"
              label="代理ID" 
            ></v-text-field>
          </v-flex>
          <v-flex>
            <v-text-field
              v-model="inputForm.agent_name"
              label="代理名称" 
            ></v-text-field>
          </v-flex>

          <v-flex
            shrink
            style="width: 128px"
            >
            <v-text-field
              v-model="inputForm.price[0]"
              label="账号金额（小）"
              type="number"
            ></v-text-field>
          </v-flex>
    
          <v-flex>
            <v-range-slider
              v-model="inputForm.price"
              :max="100000"
              :min="0"
            ></v-range-slider>
          </v-flex>
    
          <v-flex
            shrink
            style="width: 128px"
          >
            <v-text-field
              v-model="inputForm.price[1]"
              label="账号金额（大）"
              type="number"
            ></v-text-field>
          </v-flex>
          <v-btn @click="submit">开始搜索</v-btn>
          <v-btn @click="clear">重置</v-btn>
        </v-layout>
      </v-container>
    </v-form>
    <v-data-table
      :headers="headers"
      :items="desserts"
      :pagination.sync="pagination"
      :total-items="totalDesserts"
      :loading="loading"
      :rows-per-page-items="pageItems"
    >
      <template slot="items" slot-scope="props">
        <td>{{ props.item.id }}</td>
        <td class="text-xs-right">{{ props.item.name }}</td>
        <td class="text-xs-right">{{ props.item.channel }}</td>
        <td class="text-xs-right">{{ props.item.player_id }}</td>
        <td class="text-xs-right">{{ props.item.level }}</td>
        <td class="text-xs-right">{{ props.item.min_money }}</td>
        <td class="text-xs-right">{{ props.item.money }}</td>
        <td class="text-xs-right">{{ props.item.transferName }}</td>
        <td class="text-xs-right">{{ props.item.clock }}</td>
        <td class="text-xs-right">{{ props.item.remark }}</td>
        <td class="text-xs-right">{{ props.item.created_at }}</td>
        <td class="justify-center layout px-0">
          <v-icon
            small
            class="mr-2"
            @click="editItem(props.item)"
          >
          edit
          </v-icon>
          <v-icon
            small
            @click="deleteItem(props.item)"
          >
          delete
          </v-icon>
        </td>
      </template>
    </v-data-table>
  </div>
</template>

<script>
import { mapActions, mapGetters } from 'vuex'

export default {
  name: 'management',
  data() {
    return {
      switch1: true,
      isShow: false,
      isSuccess: '',
      message: '',
      dialog: false,
      defaultItem: {
        name: null,
        player_id: null,
        min_money: null,
        qq: null,
        alipay: null,
        weixin: null,
        telphone: null,
        auto_reply: null,
        remark: null,
        transfer: false,
        level: 1,
        adLevel: { text: '1级', value: '1' },
        mailContent: '你已通过代理审核，恭喜你的账号正式成为代理账号！'
      },
      editedIndex: -1,
      editedItem: {
        name: null,
        player_id: null,
        min_money: null,
        qq: null,
        alipay: null,
        weixin: null,
        telphone: null,
        auto_reply: null,
        remark: null,
        transfer: false,
        level: 1,
        adLevel: { text: '1级', value: '1' },
        mailContent: '你已通过代理审核，恭喜你的账号正式成为代理账号！'
      },
      inputForm: {
        agent_id: null,
        agent_name: null,
        price: [0, 0]
      },
      accountRules: {
        required: value => !!value || '请输入帐号'
      },
      passwordRules: {
        required: value => !!value || '请输入密码'
      },
      totalDesserts: 0,
      desserts: [],
      loading: true,
      pageItems: [
        10,
        20,
        30,
        { text: '$vuetify.dataIterator.rowsPerPageAll', value: -1 }
      ],
      pagination: {'sortBy': 'created_at', 'descending': true},
      headers: [
        {
          text: '代理商ID',
          align: 'left',
          value: 'id'
        },
        { text: '代理名称', value: 'name' },
        { text: '渠道', value: 'channel' },
        { text: '玩家ID', value: 'player_id' },
        { text: '等级', value: 'level' },
        { text: '充值下限', value: 'min_money' },
        { text: '账号金额', value: 'money' },
        { text: '快速转账', value: 'transferName' },
        { text: '打卡状态', value: 'clock' },
        { text: '备注', value: 'remark' },
        { text: '创建时间', value: 'created_at' },
        { text: '操作', value: '' }
      ]
    }
  },
  watch: {
    pagination: {
      handler () {
        this.getDataFromApi().then(data => {
          this.desserts = data.items
          this.totalDesserts = data.total
        })
      },
      deep: true
    }
  },
  computed: {
    isNew() {
      return this.editedIndex == -1
    },
    ...mapGetters(['adLevel'])
  },
  methods: {
    submit () {
      this.getDataFromApi().then(data => {
        this.desserts = data.items
        this.totalDesserts = data.total
      })
    },
    clear () {
      this.inputForm.agent_id = null
      this.inputForm.agent_name = null
      this.inputForm.price = [0, 0]
    },
    async getDataFromApi () {
      this.loading = true
      return new Promise((resolve, reject) => {
        const { sortBy, descending, page, rowsPerPage } = this.pagination
        this.getAgent(this.inputForm).then(data => {
          let items = _.cloneDeep(data)
          const total = items.length

          if (this.pagination.sortBy) {
            items = items.sort((a, b) => {
              const sortA = a[sortBy]
              const sortB = b[sortBy]

              if (descending) {
                if (sortA < sortB) return 1
                if (sortA > sortB) return -1
                return 0
              } else {
                if (sortA < sortB) return -1
                if (sortA > sortB) return 1
                return 0
              }
            })
          }

          if (rowsPerPage > 0) {
            items = items.slice((page - 1) * rowsPerPage, page * rowsPerPage)
          }
          setTimeout(() => {
            this.loading = false
            resolve({
              items,
              total
            })
          }, 1000)
        })
      })
    },
    editItem (item) {
      this.editedIndex = this.desserts.indexOf(item)
      const filter = this.adLevel.filter(x => x.value == item.level)
      const adLevel= _.get(filter, '[0]', { text: '5级无广告', value: '5' })

      _.set(item, 'adLevel', adLevel)
      this.editedItem = Object.assign({}, item)
      this.dialog = true
    },
    deleteItem (item) {
      confirm('确定要删除该行吗？')
    },
    close () {
      this.dialog = false
      setTimeout(() => {
        this.editedItem = Object.assign({}, this.defaultItem)
        this.editedIndex = -1
      }, 300)
    },
    save () {
      if (this.$refs.form2.validate()) {
        let deep = _.cloneDeep(this.editedItem)
        const isEdit =_.isObject(deep.adLevel)
        if(!isEdit)  _.set(deep, 'level', deep.adLevel)
        _.set(deep, 'transfer', + deep.transfer)

        if (this.editedIndex > -1) {
          this.saveAgent(deep).then(res => {
            this.isShow = true
            setTimeout(() => {
              this.isShow = false
            }, 2000);
            this.isSuccess = res.data
            this.message = res.msg

            if(res.data){
              Object.assign(this.desserts[this.editedIndex], deep)
              this.close()
            }
          })
        } else {
          this.createAgent(deep).then(res => {
            this.isShow = true
            setTimeout(() => {
              this.isShow = false
            }, 2000);
            this.isSuccess = res.data
            this.message = res.msg
            this.close()
            if(res.data){
              this.getDataFromApi().then(data => {
                this.desserts = data.items
                this.totalDesserts = data.total
              })
            }
          })
        }
      }
    },
    getAdLevel (level) {
      const filter = this.adLevel.filter(x => x.value == level)
      return _.get(filter, '[0]', { text: '5级无广告', value: '5' })
    },
    ...mapActions(['getAgent', 'saveAgent', 'createAgent'])
  }
}
</script>
<style lang="scss" scoped>
</style>
