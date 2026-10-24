import service from '@/agenttotal/service'
const { managementService } = service

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
  async getAgent ({ getters, commit, dispatch }, inputForm) {
    try {
      let deep = _.cloneDeep(inputForm)
      if (inputForm.price) {
        _.set(deep, 'minPrice', inputForm.price[0])
        _.set(deep, 'maxPrice', inputForm.price[1])
      }
      const res = await managementService.GetAgent(deep)
      let data = _.get(res, 'data')
      return Promise.resolve(data)
    } catch (err) {
      return Promise.reject(err)
    }
  }
}

const getters = {
}

export default { state, mutations, actions, getters }
