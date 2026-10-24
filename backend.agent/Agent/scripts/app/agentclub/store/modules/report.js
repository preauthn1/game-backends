import service from '@/agentclub/service'
import moment from 'moment'

const { reportService } = service
const defaultState = {}

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

const actions = {

  async getChangeLog({ getters, commit, dispatch }, inputForm) {
    try {
      let deep = _.cloneDeep(inputForm)
      if (deep.endDate) {
        _.set(deep, 'endDate', moment(deep.endDate).endOf('day').format())
      }

      const res = await reportService.GetChangeLog(deep)
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async getLogonLog({ getters, commit, dispatch }, inputForm) {
    try {
      let deep = _.cloneDeep(inputForm)
      if (deep.endDate) {
        _.set(deep, 'endDate', moment(deep.endDate).endOf('day').format())
      }

      const res = await reportService.GetLogonLog(deep)
      const data = _.get(res, 'data')
      return Promise.resolve(data)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async getAgentProfitLog({ getters, commit, dispatch }, inputForm) {
    try {
      let deep = _.cloneDeep(inputForm)
      if (deep.endDate) {
        _.set(deep, 'endDate', moment(deep.endDate).endOf('day').format())
      }
      
      const res = await reportService.GetAgentProfitLog(deep)
      const data = _.get(res, 'data')
      return Promise.resolve(data)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async getAgentProfitMonthlyLog({ getters, commit, dispatch }, inputForm) {
    try {
      let deep = _.cloneDeep(inputForm)
      if (deep.endDate) {
        _.set(deep, 'endDate', moment(deep.endDate).endOf('day').format())
      }
      
      const res = await reportService.GetAgentProfitMonthlyLog(deep)
      const data = _.get(res, 'data')
      return Promise.resolve(data)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async getAgentWithdrawLog({ getters, commit, dispatch }, inputForm) {
    try {
      let deep = _.cloneDeep(inputForm)
      if (deep.endDate) {
        _.set(deep, 'endDate', moment(deep.endDate).endOf('day').format())
      }
      
      const res = await reportService.GetAgentWithdrawLog(deep)
      const data = _.get(res, 'data')
      return Promise.resolve(data)
    } catch (err) {
      return Promise.reject(err)
    }
  }
}
const getters = {}

export default { state, mutations, actions, getters }
