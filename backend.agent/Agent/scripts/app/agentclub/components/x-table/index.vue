<template>
  <div>
    <v-form ref="form">
      <v-container>
        <v-layout row wrap>
          <v-flex>
            <v-text-field
              v-model="inputForm.user_id"
              label="玩家ID"
            ></v-text-field>
          </v-flex>
          <v-flex>
            <v-text-field
              v-model="inputForm.loginName"
              label="帐号名称"
            ></v-text-field>
          </v-flex>
          <v-flex v-if="classType !== null">
            <v-select
            v-model="inputForm.agentClass"
            :items="classItems"
            item-text="text"
            item-value="value"
            label="身份"
            ></v-select>
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
      item-key="ID"
      rows-per-page-text="每页行数"
      no-data-text="无资料"
    >
      <template slot="items" slot-scope="props">
        <tr @click="expanded(props)">
          <td>{{ props.item.UserID }}</td>
          <td>{{ props.item.LoginName }}</td>
          <td>{{ props.item.ClassName }}</td>
          <td>{{ props.item.Tel }}</td>
          <td>{{ props.item.Weixin }}</td>
          <td>{{ props.item.QQ }}</td>
          <td>{{ props.item.Email }}</td>
          <td class="text-xs-right">{{ props.item.TotalFlows }}</td>
          <td class="text-xs-right">{{ props.item.TotalIncome }}</td>
          <td class="text-xs-right">{{ props.item.RemainAmount }}</td>
          <td>{{ props.item.InviteCode }}</td>
          <td>{{ props.item.StatusText }}</td>
          <td  class="justify-center ">
            <div v-if="props.item.Class != 99">
              <v-icon
                small
                class="mr-4"
              >
                details
              </v-icon>
              
            </div>
          </td>
        </tr>
      </template>
      <template slot="expand" slot-scope="props" v-if="props.item.Class != 99">
        <v-card flat>
          <v-card-text>
            <v-layout align-center row fill-height>
              <x-icon icon="edit" v-if="IsAdmin" @click="editItem(props.item)">修改</x-icon>
              <x-icon icon="power" v-if="(props.item.Status === 0 || props.item.Status === 2) && IsAdmin" @click="setAgentStatus(props.item.ID, 1)">帐号启用</x-icon> 
              <x-icon icon="power_off" v-else-if="props.item.Status === 1 && IsAdmin" @click="setAgentStatus(props.item.ID, 2)">帐号停权</x-icon>

              <x-icon icon="attach_money" v-if="props.item.AllowWithdraw === 0 && IsAdmin" @click="setAllowWithdraw(props.item.ID, 1)">启用转帐</x-icon> 
              <x-icon icon="money_off" v-else-if="props.item.AllowWithdraw === 1 && IsAdmin" @click="setAllowWithdraw(props.item.ID, 0)">禁用转帐</x-icon>

              <x-icon icon="restore" v-if="IsAdmin" @click="newPassword(props.item.ID)">密码重置</x-icon>      
              <x-icon icon="input" :badge="badge" @click="nextStep(props.item)">查看下级</x-icon>
              <x-icon v-if="props.item.QRUrl && IsAdmin" icon="file_copy" @click="copy(props.item.QRUrl)">复制推广</x-icon>
              <x-icon v-if="props.item.QRUrl && IsAdmin" icon="domain" @click="downloadQRCode()">下载二维码</x-icon>
              <img v-if="props.item.QRUrl && IsAdmin" :src="ShareLiveQRCodeURL" class="agent_QRCode_img">
            </v-layout>
          </v-card-text>
        </v-card>
      </template>
      <template slot="footer">
        <td colspan="100%">
          <strong>总收益 : {{sumTotal}}</strong>
        </td>
      </template>
    </v-data-table>
    <v-dialog v-model="dialog" max-width="500px" >
      <v-card dark>
        <v-card-title>
          <span class="headline"></span>
        </v-card-title>
        <v-card-text>
          <v-form ref="form2">
            <v-container>
              <v-layout>
                <v-flex>
                  <v-flex v-if="IsAdmin">
                    <v-text-field v-model="editedItem.UserID" :rules="rules" label="玩家ID"></v-text-field>
                  </v-flex>
                  <v-flex>
                    <v-text-field v-model="editedItem.Tel" :rules="tel_rules" label="电话"></v-text-field>
                  </v-flex>
                  <v-flex>
                    <v-text-field v-model="editedItem.Weixin" :rules="weixin_rules" label="微信"></v-text-field>
                  </v-flex>
                  <v-flex v-if="isShowLevel && (IsAdmin || IsGeneralAgent)">
                    <v-select
                    v-model="editedItem.GrandConfigSelect"
                    :items="grandConfigItems"
                    item-text="text"
                    item-value="value"
                    label="身份"
                    ></v-select>
                  </v-flex>
                </v-flex>
                <v-flex>
                  <v-flex>
                    <v-text-field v-model="editedItem.QQ" :rules="qq_rules" label="QQ"></v-text-field>
                  </v-flex>
                  <v-flex>
                    <v-text-field v-model="editedItem.Email" :rules="email_rules" label="信箱"></v-text-field>
                  </v-flex>
                  <v-flex v-if="IsAdmin">
                    <v-text-field v-model="editedItem.QRUrl" 
                    :rules="url_rules" label="活码位置"></v-text-field>
                  </v-flex>
                 </v-flex>
                </v-layout>
                <v-card flat color="transparent" v-if="editedItem.Class === 0"   >
                  <v-subheader>税收比例</v-subheader>
                  <v-slider
                    v-model="editedItem.DistributeRate"
                    label="渠道代理"
                    step="1"
                    thumb-label="always"
                    ticks
                    :max="rateMax"
                  ></v-slider>
                </v-card>
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
    <x-processing :is-processing="isProcessing"></x-processing>
  </div>
</template>

<script>
import { mapActions, mapGetters } from 'vuex'
import xIcon from '@/agentclub/components/x-icon'
import xProcessing from '@/agentclub/components/x-processing'

export default {
  name: 'AgentList',
  data() {
    return {
      rateMax: 100,
      isProcessing: false,
      classItems: [],
      grandConfigItems: [],
      ShareLiveQRCodeURL: null,
      dialog: false,
      remaining: 0,
      sumTotal: 0,
      menu1: false,
      menu2: false,
      local_id: null,
      isShowLevel: false,
      inputForm: {
        agent_id: null,
        user_id: null,
        agentClass: null,
        loginName: null
      },
      editedItem: {
        agent_id: null,
        Class: null,
        Level: null,
        RemainAmount: null,
        DistributeRate: null,
        Class1RateRange: null,
        Class2RateRange: null,
        Class3RateRange: null,
        Class4RateRange: null,
        Email: null,
        Tel: null,
        Weixin: null,
        QQ: null,
        GrandConfigSelect: null,
        QRUrl: null
      },
      rules: [
        v => !!v || v === 0 || '请输入'
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
      ],
      url_rules: [
        v => {
           if (v === null || v === void 0 || v.length === 0) return true
          const urlPattern = /(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&:/~\+#]*[\w\-\@?^=%&/~\+#])?/;
          return urlPattern.test(v)|| '请输入正确活码位置'
        }
      ],
      Class1Rules: [
        v => (v + this.editedItem.DistributeRate + this.editedItem.Class2RateRange + this.editedItem.Class3RateRange + this.editedItem.Class4RateRange) <= this.rateMax || `比例不能超過${this.rateMax}`
      ],
      Class2Rules: [
        v => (v + this.editedItem.DistributeRate + this.editedItem.Class1RateRange + this.editedItem.Class3RateRange + this.editedItem.Class4RateRange) <= this.rateMax || `比例不能超過${this.rateMax}`
      ],
      Class3Rules: [
        v => (v + this.editedItem.DistributeRate + this.editedItem.Class1RateRange + this.editedItem.Class2RateRange + this.editedItem.Class4RateRange) <= this.rateMax || `比例不能超過${this.rateMax}`
      ],
      Class4Rules: [
        v => (v + this.editedItem.DistributeRate + this.editedItem.Class1RateRange + this.editedItem.Class2RateRange + this.editedItem.Class3RateRange) <= this.rateMax || `比例不能超過${this.rateMax}`
      ],
      totalDesserts: 0,
      desserts: [],
      loading: true,
      pageItems: [
        50,
        100,
        150,
        { text: '$vuetify.dataIterator.rowsPerPageAll', value: -1 }
      ],
      pagination: {'sortBy': 'CreateTime', 'descending': true},
      headers: [
        {
          text: '玩家ID',
          align: 'left',
          value: 'UserID'
        },
        { text: '帐号名称', value: 'LoginName' },
        { text: '身份', value: 'ClassName' },
        { text: '电话', value: 'Tel' },
        { text: '微信', value: 'Weixin' },
        { text: 'QQ', value: 'QQ' },
        { text: '信箱', value: 'Email' },
        { text: '总税收', value: 'TotalFlows' },
        { text: '总收益', value: 'TotalIncome' },
        { text: '总剩余金额', value: 'RemainAmount' },
        { text: '邀请码', value: 'InviteCode' },
        { text: '状态', value: 'StatusText' },
        { text: '操作', value: 'name', sortable: false }
      ],
      badge: 0
    }
  },

  props: {
    agentID: [String],
    classType: [Number, String],
    index: [Number, String],
    pageIndex: [Number, String],
    id: [Number, String],
    title: [Array]
  },

  watch: {
    id (val) {
      if (!val) return
      if (this.index == this.pageIndex) {
        if (this.local_id === val) return

        this.$set(this, 'local_id', val)
        this.$set(this, 'desserts', [])
        this.getDataFromApi().then(data => {
          this.desserts = data.items
          this.totalDesserts = data.total
        })
      }
    },
    steps (val) {
      if (this.e1 > val) {
        this.e1 = val
      }
    },
    pagination: {
      handler () {
        if(this.index == this.pageIndex) {
          this.getDataFromApi().then(data => {
            this.desserts = data.items
            this.totalDesserts = data.total
          })
        }
      },
      deep: true
    }
  },

  computed: {
 
    ...mapGetters(['IsAdmin', 'IsGeneralAgent'])
  },

  async mounted () {
    this.$set(this, 'classItems', await this.getAgentClassItems())
  },

  methods: {
    async expanded (props) {
      if(!props.expanded) {
        const id = props.item.ID
        const QRUrl = props.item.QRUrl

        const _count = await this.getAgentRecursiveDown(id)
        const url = await this.getLiveQRCode(QRUrl)

        _.set(this, 'ShareLiveQRCodeURL', url)
        _.set(this, 'badge', _count)
      }
      props.expanded = !props.expanded
    },
    onInput (val) {
      this.steps = parseInt(val)
    },
    nextStep (item) {
      this.clear()
      const _title = _.cloneDeep(this.title)
      _title[this.pageIndex] = { Class: item.ClassName, UserID: item.LoginName }
      this.$emit('update:title', _title)
      this.$emit('update:agentID', item.ID)
      this.$emit('update:classType', item.Class)
      this.$emit('click', this.pageIndex, this.id)
    },
    submit () {
      this.pagination.page = 1
      this.getDataFromApi().then(data => {
        this.desserts = data.items
        this.totalDesserts = data.total
      })
    },
    clear () {
      this.$refs.form.reset()
    },

    copy(s) {
      let clip_area = document.createElement('textarea');
      clip_area.textContent = s;
      document.body.appendChild(clip_area);
      clip_area.select();
      document.execCommand('copy');
      clip_area.remove();
    },

     downloadQRCode () {
      let link = document.createElement('a');
      link.href = this.ShareLiveQRCodeURL;
      link.download = 'code.jpg';
      document.body.appendChild(link);
      link.click();
    },

    getLiveQRCode (url) {
      if(url) return `/agentclub/admin/GetLiveQRCode?url=${url}`
      return url
    },

    async editItem (item) {
      const grandConfigItems = await this.getGrandConfigItems(item.ID)
      item.Class === 0 ? this.$set(this, 'isShowLevel', false) : this.$set(this, 'isShowLevel', true)
      this.$set(this, 'grandConfigItems', grandConfigItems)
      const level = item.Level
      const grandConfig = grandConfigItems.filter((d) => {
        return d.level === level
      })
      this.editedItem = Object.assign({}, item)
      if (grandConfig.length >= 1) this.$set(this.editedItem, 'GrandConfigSelect', { value: grandConfig[0].value, text: grandConfig[0].text })
      this.dialog = true
    },
    async newPassword (id) {
      this.$set(this, 'isProcessing', true)
      const res = await this.updateAgentUser({id: id, isPassword: [REDACTED]
      this.$set(this, 'isProcessing', false)
      this.restart(res)
    },
    async newInviteCode (id) {
      this.$set(this, 'isProcessing', true)
      const res = await this.updateAgentUser({id: id, isInviteCode: true})
      this.$set(this, 'isProcessing', false)
      this.restart(res)
    },
    async setAgentStatus (id, status) {
      this.$set(this, 'isProcessing', true)
      const res = await this.updateAgentUser({id: id, agentStatus: status})
      this.$set(this, 'isProcessing', false)
      this.restart(res)
    },
    async setAllowWithdraw (id,isWithdraw) {
      this.$set(this, 'isProcessing', true)
      const res = await this.updateAgentUser({id: id, allowWithdraw: isWithdraw})
      this.$set(this, 'isProcessing', false)
      this.restart(res)
    },
    async save () {
      if (this.$refs.form2.validate()) {
        this.$set(this, 'isProcessing', true)
        const res = await this.updateAgentUser(this.editedItem)
        this.$set(this, 'isProcessing', false)
        this.restart(res)
        this.$set(this, 'dialog', false)
      }
    },
    close () {
      this.$set(this, 'dialog', false)
    },
    async getDataFromApi () {
      this.loading = true
      return new Promise((resolve, reject) => {
        const { sortBy, descending, page, rowsPerPage } = this.pagination
        _.set(this.inputForm, 'agent_id', this.id)
        this.getAgentUser(this.inputForm).then(res => {
          const sumTotal =  _.get(res, 'sumTotal')
          const data = _.get(res, 'data')
          this.$set(this, 'sumTotal', sumTotal)

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
    restart (isSuccess) {
      if(isSuccess) {
        this.getDataFromApi().then(data => {
          this.desserts = data.items
          this.totalDesserts = data.total
        })
      }
    },
    ...mapActions(['getAgentClassItems', 'getGrandConfigItems', 'getAgentUser', 'updateAgentUser', 'getAgentRecursiveDown'])
  },
  components: {
    xIcon,
    xProcessing
  }
}
</script>
<style lang="scss">

.agent_QRCode_img {
  width: 5%;
}
</style>
