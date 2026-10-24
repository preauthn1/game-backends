import service from '@/agentclub/service'
import router from '@/agentclub/router'
import Helpers from '@/agentclub/helpers'
import { xhrErrors, ERRORS_CODE } from '@/agentclub/service/xhr/config'
const md5 = require('md5')
const { authService } = service

const defaultState = {
  xhrQueue: [],
  isLogin: false,
  userpermission: null,
  userloginname: null,
  userAllowWithdraw: null,
  snackbar: false,
  snackbarColor: 'error',
  errorMsg: '',
  isAdmin: false,
  isGeneralAgent: false,
  isSecondAgent: false
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
  },
  USER_LOGINNAME (state, payload) {
    state.userloginname = payload
  },
  USER_AllowWithdraw (state, payload) {
    state.userAllowWithdraw = payload
  },
  USER_PERMISSION (state, payload) {
    state.userpermission = payload
  },
  SNACKBAR (state, payload) {
    state.snackbar = payload
  },
  SNACKBAR_COLOR (state, payload) {
    state.snackbarColor = payload
  },
  ERRORMSG (state, payload) {
    state.errorMsg = payload
  },
  IS_ADMIN (state, payload) {
    state.isAdmin = payload
  },
  Is_General_Agent (state, payload) {
    state.isGeneralAgent = payload
  },
  Is_Second_Agent(state, payload) {
    state.isSecondAgent = payload
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

  showSnackbar ({ commit }, { status, errorMsg }) {   
    const code = ERRORS_CODE[status]
    commit('SNACKBAR_COLOR', status > 20000 ? 'success' : 'error')
    if(errorMsg) {
      commit('ERRORMSG', errorMsg)
    } else {
      commit('ERRORMSG', xhrErrors(code))
    }
    if (status < 20000) console.warn(errorMsg ? errorMsg : xhrErrors(code))

    commit('SNACKBAR', true)
  },

  closeSnackbar({ commit }) {
    commit('SNACKBAR', false)
  },

  async logout ({ getters, commit, dispatch }) {
    try {
      Helpers.other.token = ''
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
      const token = [REDACTED], 'token')
      return Promise.resolve(token)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  async getInformation ({ getters, commit, dispatch }) {
    try {
      const res = await authService.GetInformation()
      const data = _.get(res, 'data')
      return Promise.resolve(data)
    } catch (err) {
      return Promise.reject(err)
    }
  },

  
  async sendVerificationCode ({ commit }) {
    await authService.SendVerificationCode()
  },

  async validateToken ({ dispatch }, secureToken) {
    try {
      const { IsAuthenticated, UserType, LoginName, AllowWithdraw } = await authService.validateToken(secureToken)
      await dispatch('setBase', {UserType, LoginName, AllowWithdraw})
      return Promise.resolve(IsAuthenticated)
    } catch (err) {
      dispatch('logout')
      return Promise.reject(`${err}`)
    }
  },

  async checkPermission ({ dispatch, getters }, toName) {
    try {
      let permissionSetting = getters.PermissionSetting
      const user_permission = getters.UserPermission
      const setting = _.filter(permissionSetting, function(o) {
        const userType = md5(o.userType)
        return userType == user_permission
      })
      
      if (setting.length === 0) return Promise.resolve(false)
      if(toName === 'home') return Promise.resolve(true)

      const permission = setting[0].Permission
      var filter =_.filter(permission, function(o) { return !o.name == toName })
      return Promise.resolve(setting)
    } catch (err) {
      dispatch('logout')
      return Promise.reject(`${err}`)
    }
  },

  async setBase ({ commit }, res) {
    const userType = _.get(res, 'UserType')
    commit('IS_ADMIN',  userType === 100)
    commit('Is_General_Agent', userType === 0)
    commit('Is_Second_Agent', userType === 2)
    commit('USER_PERMISSION', md5(userType))
    commit('USER_LOGINNAME', _.get(res, 'LoginName'))
    commit('USER_AllowWithdraw', _.get(res, 'AllowWithdraw'))
  }
}

const getters = {
  UserPermission: ({ userpermission }) => {
    return userpermission
  },
  UserLoginName: ({ userloginname }) => {
    return userloginname
  },
  IsAdmin: ({ isAdmin }) => {
    return isAdmin
  },
  AllowWithdraw: ({ userAllowWithdraw }) => {
    return userAllowWithdraw
  },
  IsGeneralAgent: ({ isGeneralAgent }) => {
    return isGeneralAgent
  },
  IsSecondAgent: ({ isSecondAgent }) => {
    return isSecondAgent
  }
}

export default { state, mutations, actions, getters }
