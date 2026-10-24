<template>
  <div>
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
            <v-select
            v-model="inputForm.agentMoneyType"
            :items="agentMoneyType"
            item-text="text"
            item-value="value"
            label="转账类别"
            ></v-select>
          </v-flex>
          <v-flex>
            <v-text-field
              v-model="inputForm.user_id"
              label="用户ID"
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
          <v-flex shrink style="width: 128px">
            <v-text-field
              v-model="inputForm.price[0]"
              label="交易金额（小）"
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
          <v-flex shrink style="width: 128px">
            <v-text-field
              v-model="inputForm.price[1]"
              label="交易金额（大）"
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
        <td>{{ props.item.agent_id }}</td>
        <td>{{ props.item.remark }}</td>
        <td>{{ props.item.type }}</td>
        <td>{{ props.item.user_id }}</td>
        <td>{{ props.item.money }}</td>
        <td>{{ props.item.created_at }}</td>
        <td>{{ props.item.ip }}</td>
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
      menu1: false,
      menu2: false,
      inputForm: {
        agent_id: '',
        agentMoneyType: null,
        price: [0, 0],
        user_id: '',
        startDate: '',
        endDate: '',
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
        { text: '代理ID', value: 'agent_id' },
        { text: '备注', value: 'remark' },
        { text: '类型', value: 'type' },
        
        { text: '交易用户ID', value: 'user_id' },
        { text: '交易金额', value: 'money' },
        { text: '创建时间', value: 'created_at' },
        { text: '操作IP区域', value: 'ip' }
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
    ...mapGetters(['agentMoneyType'])
  },
  methods: {
    submit () {
      this.getDataFromApi().then(data => {
        this.desserts = data.items
        this.totalDesserts = data.total
      })
    },
    clear () {
      this.inputForm.agent_id = ''
      this.inputForm.price = [0, 0]
      this.inputForm.user_id = ''
      this.inputForm.transferType = null
      this.inputForm.startDate = ''
      this.inputForm.endDate = ''
    },
    async getDataFromApi () {
      this.loading = true
      return new Promise((resolve, reject) => {
        const { sortBy, descending, page, rowsPerPage } = this.pagination
        this.getAgentDetails(this.inputForm).then(data => {
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
    ...mapActions(['getAgentDetails'])
  }
}
</script>
<style lang="scss" scoped>
</style>
