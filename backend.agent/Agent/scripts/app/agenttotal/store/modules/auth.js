import service from '@/agenttotal/service'
import router from '@/agenttotal/router'
import { xhrErrors } from '@/agenttotal/service/xhr/config'

const { authService } = service

const defaultState = {
  xhrQueue: [],
  isLogin: false
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
  },
  RESET_XHR_QUEUE (state) {
    state.xhrQueue = []
  },
  SET_XHR_QUEUE (state, payload) {
    state.xhrQueue = payload
  }
}

const actions = {
  addXhrQueue ({ state, commit }, newXhr) {
    const oldQueue = _.cloneDeep(state.xhrQueue)
    const newQueue = _.concat(oldQueue, newXhr)
    commit('SET_XHR_QUEUE', newQueue)
  },
  removeXhrQueue ({ state, commit }, xhrIndex) {
    const oldQueue = _.cloneDeep(state.xhrQueue)
    const newQueue = oldQueue.filter(xhr => xhr.xhrIndex !== xhrIndex)
    commit('SET_XHR_QUEUE', newQueue)
  },

  async logout ({ getters, commit, dispatch }) {
    try {
    } catch (err) {
      console.error(`[logout] -> ${err}`)
    } finally {
      router.replace({ name: 'login' })
      
    }
  },

  async login ({ getters, commit, dispatch }, { username, password }) {
    try {
      if (username === '' || password === '') throw xhrErrors('v-no-data')
      const data = {
        username,
        password
      }
      const res = await authService.login(data)
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async validateToken ({ dispatch }, secureToken) {
    try {
      const isToken = [REDACTED] authService.validateToken(secureToken)
      if(!isToken)  dispatch('logout')
      return Promise.resolve(isToken)
    } catch (err) {
      dispatch('logout')
      return Promise.reject(`${err}`)
    }
  },

  async getLoginLog ({ getters, commit, dispatch }) {
    try {
      const res = await authService.GetLoginLog()
      const data = _.get(res, 'data')
      return Promise.resolve(data)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async getAgentLoginLog ({ getters, commit, dispatch }, inputForm) {
    try {
      let deep = _.cloneDeep(inputForm)
      const res = await authService.GetAgentLoginLog(deep)
      const data = _.get(res, 'data')
      return Promise.resolve(data)
    } catch (err) {
      return Promise.reject(err)
    }
  }
}

export default { state, mutations, actions }
