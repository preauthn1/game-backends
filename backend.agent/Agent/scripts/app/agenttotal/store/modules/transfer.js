import service from '@/agenttotal/service'
import moment from 'moment'

const { transferService } = service

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
  async getTransferLog ({ getters, commit, dispatch }, inputForm) {
    try {
      let deep = _.cloneDeep(inputForm)
      if (deep.endDate) {
        _.set(deep, 'endDate', moment(deep.endDate).endOf('day').format())
      }
      _.set(deep, 'minPrice', inputForm.price[0])
      _.set(deep, 'maxPrice', inputForm.price[1])

      const res = await transferService.GetTransferLog(deep)
      const data = _.get(res, 'data')
      return Promise.resolve(data)
    } catch (err) {
      return Promise.reject(err)
    }
  },
  async transferMoney ({ getters, commit, dispatch }, temp) {
    try {
      const res = await transferService.transferMoney(temp)
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async agentOrderBack ({ getters, commit, dispatch }, editedItem) {
    try {
      const res = await transferService.AgentOrderBack(editedItem)
      const data = _.get(res, 'data')
      return Promise.resolve(data)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async getRechargelog ({ getters, commit, dispatch }) {
    try {
      const res = await transferService.GetRechargeLog()
      const data = _.get(res, 'data')
      return Promise.resolve(data)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async getAgentRechargeLog ({ getters, commit, dispatch }, inputForm) {
    try {
      let deep = _.cloneDeep(inputForm)
      if (deep.endDate) {
        _.set(deep, 'endDate', moment(deep.endDate).endOf('day').format())
      }
      const res = await transferService.GetAgentRechargeLog(deep)
      const data = _.get(res, 'data')
      return Promise.resolve(data)
    } catch (err) {
      return Promise.reject(err)
    }
  },
  
  async getAgentBackLog ({ getters, commit, dispatch }, inputForm) {
    try {
      let deep = _.cloneDeep(inputForm)
      if (deep.endDate) {
        _.set(deep, 'endDate', moment(deep.endDate).endOf('day').format())
      }
      const res = await transferService.GetAgentBackLog(deep)
      const data = _.get(res, 'data')
      return Promise.resolve(data)
    } catch (err) {
      return Promise.reject(err)
    }
  }
}

export default { state, mutations, actions }
