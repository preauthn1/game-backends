import service from '@/agenttotal/service'
import moment from 'moment'

const { reportService } = service

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
  async getReportDay ({ getters, commit, dispatch }, inputForm) {
    try {
      let deep = _.cloneDeep(inputForm)
      if (deep.endDate) {
        _.set(deep, 'endDate', moment(deep.endDate).endOf('day').format())
      }
      _.set(deep, 'minPrice', inputForm.price[0])
      _.set(deep, 'maxPrice', inputForm.price[1])
      
      const res = await reportService.GetReportDay(deep)
      const data = _.get(res, 'data')
      return Promise.resolve(data)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async getAgentDetails ({ getters, commit, dispatch }, inputForm) {
    try {
      let deep = _.cloneDeep(inputForm)
      if (deep.endDate) {
        _.set(deep, 'endDate', moment(deep.endDate).endOf('day').format())
      }
      _.set(deep, 'minPrice', inputForm.price[0])
      _.set(deep, 'maxPrice', inputForm.price[1])
      
      const res = await reportService.GetAgentDetails(deep)
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
