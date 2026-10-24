import service from '@/agenttotal/service'

const { adminService } = service
const defaultState = {
}

const state = () => {
  return _.cloneDeep(defaultState)
}

const mutations = {
  RESET (s) {
    const initial = state()
    Object.keys(initial).forEach(key => {
      s[key] = initial[key]
    })
  }
}

const actions = {
  async createAgent ({ getters, commit, dispatch }, deep) {
    try {
      const res = await adminService.CreateAgent(deep)
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  },
  async saveAgent ({ getters, commit, dispatch }, deep) {
    try {
      const res = await adminService.SaveAgent(deep)
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  },
  async getAgentUserMoney ({ getters, commit, dispatch }, inputForm) {
    try {
      const res = await adminService.GetAgentUserMoney(inputForm)
      const data = _.get(res, 'data')
      return Promise.resolve(data)
    } catch (err) {
      return Promise.reject(err)
    }
  }
}

const getters = {
}

export default { state, mutations, actions, getters }
