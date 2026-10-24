const defaultState = {
  deviceType: [
    { text: '全部', value: 'null' },
    { text: 'PC', value: '1' },
    { text: '手机', value: '2' }
  ],
  transferType: [
    { text: '全部', value: 'null' },
    { text: '转入', value: '0' },
    { text: '转出', value: '1' }
  ],
  agentMoneyType: [
    { text: '全部', value: 'null' },
    { text: '转入', value: '1' },
    { text: '转出', value: '2' }
  ],
  status: [
    { text: '全部', value: 'null' },
    { text: '禁用', value: '0' },
    { text: '启用', value: '1' }
  ],
  transfer: [
    { text: '全部', value: 'null' },
    { text: '否', value: '0' },
    { text: '是', value: '1' }
  ],
  adLevel: [
    { text: '1级', value: '1' },
    { text: '2级', value: '2' },
    { text: '3级', value: '3' },
    { text: '4级', value: '4' },
    { text: '5级无广告', value: '5' }
  ]
}

const state = () => {
  return _.cloneDeep(defaultState)
}

const mutations = {
  RESET(s) {
    const initial = state()
    Object.keys(initial).forEach(key => {
      s[key] = initial[key]
    })
  }
}

const actions = {}

const getters = {
  deviceType: ({ deviceType }) => {
    return deviceType
  },
  transferType: ({ transferType }) => {
    return transferType
  },
  status: ({ status }) => {
    return status
  },
  transfer: ({ transfer }) => {
    return transfer
  },
  agentMoneyType: ({ agentMoneyType }) => {
    return agentMoneyType
  },
  adLevel: ({ adLevel }) => {
    return adLevel
  }
}

export default { state, mutations, actions, getters }
