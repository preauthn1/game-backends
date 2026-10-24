import service from '@/agentclub/service'
import moment from 'moment'

const { agentService } = service
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
  async getAgentWithdraw({ getters, commit, dispatch }, inputForm) {
    try {
      let deep = _.cloneDeep(inputForm)
      const res = await agentService.GetAgentWithdraw(deep)
      const result = _.get(res, 'data')
      return Promise.resolve(result)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async agentWithdraw({ getters, commit, dispatch }, remainAmount) {
    try {
      let deep = _.cloneDeep(remainAmount)
      const input = { remainAmount: deep * 100 }
      const res = await agentService.AgentWithdraw(input)
      const result = _.get(res, 'data')
      return Promise.resolve(result)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async updateWithdrawApply(
    { getters, commit, dispatch },
    { applyUID, status }
  ) {
    try {
      const input = {
        applyUID,
        status
      }
      const res = await agentService.UpdateWithdrawApply(input)
      const result = _.get(res, 'result')
      return Promise.resolve(result)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async getAgentApply({ getters, commit, dispatch }, inputForm) {
    try {
      let deep = _.cloneDeep(inputForm)
      if (deep.endDate) {
        _.set(
          deep,
          'endDate',
          moment(deep.endDate)
            .endOf('day')
            .format()
        )
      }

      const res = await agentService.GetAgentApply(deep)
      const data = _.get(res, 'data')
      return Promise.resolve(data)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async getAgentUser({ getters, commit, dispatch }, inputForm) {
    try {
      let deep = _.cloneDeep(inputForm)
      if (deep.endDate) {
        _.set(
          deep,
          'endDate',
          moment(deep.endDate)
            .endOf('day')
            .format()
        )
      }
      const res = await agentService.GetAgentUser(deep)
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async getAgentCustomer({ getters, commit, dispatch }, inputForm) {
    try {
      let deep = _.cloneDeep(inputForm)
      if (deep.endDate) {
        _.set(
          deep,
          'endDate',
          moment(deep.endDate)
            .endOf('day')
            .format()
        )
      }

      const res = await agentService.GetAgentCustomer(deep)
      const data = _.get(res, 'data')
      return Promise.resolve(data)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async getRateRange({ getters, commit, dispatch }) {
    try {
      const res = await agentService.GetRateRange()
      const result = _.get(res, 'result')
      return Promise.resolve(result)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async updateAgentUser({ getters, commit, dispatch }, editedItem) {
    try {
      const res = await agentService.UpdateAgentUser(editedItem)
      const result = _.get(res, 'result')
      return Promise.resolve(result)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async updateInformation({ getters, commit, dispatch }, editedItem) {
    try {
      const res = await agentService.UpdateInformation(editedItem)
      const result = _.get(res, 'result')
      return Promise.resolve(result)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async getGrandConfig({ getters, commit, dispatch }, inputForm) {
    try {
      const res = await agentService.GetGrandConfig(inputForm)
      const data = _.get(res, 'data')
      return Promise.resolve(data)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async getAgentMaxMinRate({ getters, commit, dispatch }, agentID) {
    try {
      const res = await agentService.GetAgentMaxMinRate({ agentID: agentID })
      const result = _.get(res, 'result')
      return Promise.resolve(result)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async updateGrandConfig({ getters, commit, dispatch }, editedItem) {
    try {
      const res = await agentService.UpdateGrandConfig(editedItem)
      const result = _.get(res, 'result')
      return Promise.resolve(result)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async deleteGrandConfig({ getters, commit, dispatch }, grandID) {
    try {
      const res = await agentService.DeleteGrandConfig({ grandID })
      const result = _.get(res, 'result')
      return Promise.resolve(result)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async agentPassed({ getters, commit, dispatch }, { applyUID, status }) {
    try {
      const input = {
        applyUID,
        status
      }
      const res = await agentService.AgentPassed(input)
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async updatePassword({ getters, commit, dispatch }, inputForm) {
    try {
      let deep = _.cloneDeep(inputForm)
      const res = await agentService.UpdatePassword(deep)
      const result = _.get(res, 'result')
      return Promise.resolve(result)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async createAgent({ getters, commit, dispatch }, inputForm) {
    try {
      let deep = _.cloneDeep(inputForm)
      const res = await agentService.CreateAgent(deep)
      const result = _.get(res, 'result')
      return Promise.resolve(result)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async customerUpgrade({ getters, commit, dispatch }, userID) {
    try {
      const res = await agentService.CustomerUpgrade({ userID })
      const result = _.get(res, 'result')
      return Promise.resolve(result)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async getAgentRecursiveDown({ getters, commit, dispatch }, id) {
    try {
      const res = await agentService.GetAgentRecursiveDown({ id: id })
      const result = _.get(res, 'result')
      return Promise.resolve(result)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async getAgentApplyCount({ getters, commit, dispatch }) {
    try {
      const res = await agentService.GetAgentApplyCount()
      const result = _.get(res, 'result')
      return Promise.resolve(result)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async getGrandConfigItems({ getters, commit, dispatch }, id) {
    try {
      const res = await agentService.GetGrandConfigItems({ id: id })
      const result = _.get(res, 'result')
      return Promise.resolve(result)
    } catch (err) {
      return Promise.reject(err)
    }
  }
}
const getters = {}

export default { state, mutations, actions, getters }
