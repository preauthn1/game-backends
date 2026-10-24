<template>
  <div>
     <v-form ref="form">
      <v-container>
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
      class="elevation-1"
    >

      <template slot="items" slot-scope="props">
        <td>{{ props.item.created_at }}</td>
        <td>{{ props.item.agent_id }}</td>
        <td>{{ props.item.name }}</td>
        <td>{{ props.item.recharge }}</td>
      </template>
    </v-data-table>
  </div>
</template>

<script>
import { mapActions } from 'vuex'

export default {
  name: 'transferlog',
  data() {
    return {
      menu1: false,
      menu2: false,
      inputForm: {
        agent_id: '',
        startDate: '',
        endDate: ''
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
        { text: '时间', value: 'created_at' },
        { text: '代理', value: 'agent_id' },
        { text: '代理名', value: 'name' },
        { text: '充值金额', value: 'recharge' }
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
        this.getAgentRechargeLog(this.inputForm).then(data => {
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
    ...mapActions(['getAgentRechargeLog'])
  }
}
</script>
<style lang="scss" scoped>
</style>
