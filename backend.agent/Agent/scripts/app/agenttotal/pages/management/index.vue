<template>
  <div>      
    <v-alert
      :value="isShow"
      :type="isSuccess ? 'success' : 'error'"
    >
      {{message}}
    </v-alert>
    <v-dialog v-model="dialog" max-width="500px">
      <v-card>
        <v-card-title>
          <span class="headline"></span>
        </v-card-title>
        <v-card-text>
          <v-container grid-list-md>
            <v-layout wrap>
              <v-flex xs12 sm6 md4>
                <v-text-field readonly v-model="editedItem.id" label="代理ID"></v-text-field>
              </v-flex>
              <v-flex xs12 sm6 md4>
                <v-text-field readonly v-model="editedItem.id" label="代理账户"></v-text-field>
              </v-flex>
              <v-flex xs12 sm6 md4>
                <v-text-field readonly v-model="editedItem.status" label="状态"></v-text-field>
              </v-flex>
              <v-flex xs12 sm6 md4>
                <v-text-field readonly v-model="editedItem.name" label="代理名称"></v-text-field>
              </v-flex>
              <v-flex xs12 sm6 md4>
                <v-text-field v-model="editedItem.amount" type="number" label="转账金额"></v-text-field>
              </v-flex>
            </v-layout>
          </v-container>
        </v-card-text>

        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="blue darken-1" flat @click.native="close">Cancel</v-btn>
          <v-btn color="blue darken-1" flat @click.native="save">Save</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
    <v-form ref="form">
      <v-container>
        <v-layout row wrap>
          <v-flex>
            <v-select
            v-model="inputForm.status"
            :items="status"
            item-text="text"
            item-value="value"
            label="状态"
            ></v-select>
          </v-flex>
          <v-flex>
            <v-select
            v-model="inputForm.transfer"
            :items="transfer"
            item-text="text"
            item-value="value"
            label="绑定手机登录"
            ></v-select>
          </v-flex>
          <v-flex>
            <v-text-field
              v-model="inputForm.agent_id"
              label="代理ID"
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
        <td class="text-xs-right">{{ props.item.status }}</td>
        <td class="text-xs-right">{{ props.item.telphone }}</td>
        <td class="text-xs-right">{{ props.item.transfer }}</td>
        <td class="text-xs-right">{{ props.item.name }}</td>
        <td class="text-xs-right">{{ props.item.money }}</td>
        <td class="text-xs-right">{{ props.item.total_recharge }}</td>
        <td class="text-xs-right">{{ props.item.total_turn_out }}</td>
        <td class="text-xs-right">{{ props.item.total_turn_in }}</td>
        <td class="text-xs-right">{{ props.item.weixin }}</td>
        <td class="text-xs-right">{{ props.item.qq }}</td>
        <td class="text-xs-right">{{ props.item.alipay }}</td>
        <td class="text-xs-right">{{ props.item.created_at }}</td>
        <td class="text-xs-right">{{ props.item.remark }}</td>
        <td class="justify-center layout px-0">
            <v-icon
              small
              class="mr-2"
              @click="editItem(props.item)"
            >
              edit
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
      isShow: false,
      isSuccess: '',
      message: '',
      dialog: false,
      editedIndex: -1,
      editedItem: {
        id: '',
        status: '',
        name: '',
        amount: 0
      },
      inputForm: {
        status: null,
        transfer: null,
        agent_id: ''
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
        { text: '状态', value: 'status' },
        { text: '手机号', value: 'telphone' },
        { text: '快速转账', value: 'transfer' },
        { text: '代理名称', value: 'name' },
        { text: '余额', value: 'money' },
        { text: '充值合计', value: 'total_recharge' },
        { text: '转出合计', value: 'total_turn_out' },
        { text: '转入合计', value: 'total_turn_in' },
        { text: 'weixin', value: 'weixin' },
        { text: 'QQ', value: 'qq' },
        { text: 'Alipay', value: 'alipay' },
        { text: '注册时间', value: 'created_at' },
        { text: '备注', value: 'remark' },
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
  mounted () {
    this.getDataFromApi().then(data => {
      this.desserts = data.items
      this.totalDesserts = data.total
    })
  },
  computed: {
    ...mapGetters(['status', 'transfer'])
  },
  methods: {
    submit () {
      this.getDataFromApi().then(data => {
        this.desserts = data.items
        this.totalDesserts = data.total
      })
    },
    clear () {
      this.$refs.form.reset()
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
      this.editedItem = Object.assign({}, item)
      this.dialog = true
    },
    close () {
      this.dialog = false
      setTimeout(() => {
        this.editedItem = Object.assign({}, this.defaultItem)
        this.editedIndex = -1
      }, 300)
    },
    save () {
      const amount = this.editedItem.amount
      const total_recharge = this.editedItem.total_recharge + parseInt(amount)
      const money = 0 + this.editedItem.money + parseInt(amount)

      if (this.editedItem.amount > 0) {
        const temp ={
          agentId: this.editedItem.id,
          turnInMoney: amount
        }
        this.transferMoney(temp).then(res => {
          this.isShow = true
          setTimeout(() => {
            this.isShow = false
          }, 2000);
          this.isSuccess = res.data
          this.message = res.msg

          if(res.data){
            this.editedItem.total_recharge = total_recharge
            this.editedItem.money = money
            Object.assign(this.desserts[this.editedIndex], this.editedItem)
            this.close()
          }
        })
      } else {
        return false
      }
    },
    ...mapActions(['getAgent', 'transferMoney'])
  }
}
</script>
<style lang="scss" scoped>
</style>
