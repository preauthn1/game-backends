<template>
  <div class="xxxx">
    <v-form ref="form">
      <v-container>
        <v-layout row wrap>
          <v-flex>
            <v-select
            v-model="inputForm.deviceType"
            :items="deviceType"
            item-text="text"
            item-value="value"
            label="操作终端"
            ></v-select>
          </v-flex>
          <v-flex>
            <v-select
            v-model="inputForm.transferType"
            :items="transferType"
            item-text="text"
            item-value="value"
            label="转账类别"
            ></v-select>
          </v-flex>
          <v-flex>
            <v-text-field
              v-model="inputForm.userID"
              label="代理ID"
            ></v-text-field>
          </v-flex>

        </v-layout>
          <v-layout row wrap>
            <v-flex>
              <v-menu
                ref="menu1"
                :close-on-content-click="false"
                v-model="menu1"
                :nudge-right="40"
                lazy
                transition="scale-transition"
                offset-y
                full-width
                max-width="290px"
                min-width="290px"
              >
                <v-text-field
                  slot="activator"
                  v-model="inputForm.startDate"
                  label="开始时间"
                  persistent-hint
                  prepend-icon="event"
                ></v-text-field>
                <v-date-picker v-model="inputForm.startDate" no-title @input="menu1 = false"></v-date-picker>
              </v-menu>
            </v-flex>
            <v-flex>
              <v-menu
                ref="menu2"
                :close-on-content-click="false"
                v-model="menu2"
                :nudge-right="40"
                lazy
                transition="scale-transition"
                offset-y
                full-width
                max-width="290px"
                min-width="290px"
              >
                <v-text-field
                  slot="activator"
                  v-model="inputForm.endDate"
                  label="结束时间"
                  persistent-hint
                  prepend-icon="event"
                ></v-text-field>
                <v-date-picker v-model="inputForm.endDate" no-title @input="menu2 = false"></v-date-picker>
              </v-menu>
            </v-flex>
          <v-flex
            shrink
            style="width: 128px"
            >
            <v-text-field
              v-model="inputForm.price[0]"
              label="金额范围（小）"
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
              label="金额范围（大）"
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
      class="elevation-1"
    >
      <template slot="items" slot-scope="props">
        <td>{{ props.item.id }}</td>
        <td class="text-xs-right">{{ props.item.total_agent_id }}</td>
        <td class="text-xs-right">{{ props.item.type }}</td>
        <td class="text-xs-right">{{ props.item.user_agent_id }}</td>
        <td class="text-xs-right">{{ props.item.money }}</td>
        <td class="text-xs-right">{{ props.item.old_money }}</td>
        <td class="text-xs-right">{{ props.item.new_money }}</td>
        <td class="text-xs-right">{{ props.item.created_at }}</td>
        <td class="text-xs-right">{{ props.item.device }}</td>
        <td class="text-xs-right">{{ props.item.remark }}</td>
      </template>
    </v-data-table>
  </div>
</template>

<script>
import { mapActions, mapGetters } from 'vuex'

export default {
  name: 'transferlog',
  data() {
    return {
      inputForm: {
        deviceType: null,
        transferType: null,
        userID: '',
        price: [0, 0],
        startDate: '',
        endDate: ''
      },
      menu1: false,
      menu2: false,
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
          text: '订单ID',
          align: 'left',
          value: 'id'
        },
        { text: '代理（id）', value: 'total_agent_id' },
        { text: '转账类型', value: 'type' },
        { text: '目标代理', value: 'user_agent_id' },
        { text: '转账金额', value: 'money' },
        { text: '转账前余额', value: 'old_money' },
        { text: '转账后余额', value: 'new_money' },
        { text: '转账时间', value: 'created_at' },
        { text: '操作终端', value: 'device' },
        { text: '转账备注', value: 'remark' }
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
    ...mapGetters(['deviceType', 'transferType'])
  },
  methods: {
    submit () {
      this.getDataFromApi().then(data => {
        this.desserts = data.items
        this.totalDesserts = data.total
      })
    },
    clear () {
      this.inputForm.deviceType = null
      this.inputForm.transferType = 0
      this.inputForm.userID = ''
      this.inputForm.price = [0, 0]
      this.inputForm.startDate = ''
      this.inputForm.endDate = ''
    },
    async getDataFromApi () {
      this.loading = true
      return new Promise((resolve, reject) => {
        const { sortBy, descending, page, rowsPerPage } = this.pagination
        this.getTransferLog(this.inputForm).then(data => {
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
    ...mapActions(['getTransferLog'])
  }
}
</script>
<style lang="scss" scoped>
</style>
