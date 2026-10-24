import { xhr } from './xhr/'
import { xhrErrors } from './xhr/config'
const md5 = require('md5')

class AUTH {
  async login (data) {
    try {
      _.set(data, 'password', md5(data.password))
      const res = await xhr({ url: '/Admin/login', data })
      if (!_.isObject(res) || res.response === false) {
        throw xhrErrors('v-login-error')
      }

      if (res.status !== 200) return Promise.reject(res.msg)

      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async GetLoginLog () {
    try {
      const res = await xhr({ url: '/Admin/GetLoginLog'})

      if (!_.isObject(res) || res.response === false) {
        throw xhrErrors('v-login-error')
      }

      if (res.status !== 200) return Promise.reject(res.msg)

      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async GetAgentLoginLog (data) {
    try {
      const res = await xhr({ url: '/Admin/GetAgentLoginLog', data: data})

      if (!_.isObject(res) || res.response === false) {
        throw xhrErrors('v-login-error')
      }

      if (res.status !== 200) return Promise.reject(res.msg)

      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }
  
  validateToken = [REDACTED] (secureToken) => {
    try {
      const res = await xhr({ url: `/Admin/ValidateToken`, method: 'GET', token:  [REDACTED]
      const IsAuthenticated = _.get(res, 'IsAuthenticated')
      return Promise.resolve(IsAuthenticated)
    } catch (err) {
      return Promise.reject(`${err}`)
    }
  }
}

export default new AUTH()