import { xhr } from './xhr/'
import { xhrErrors } from './xhr/config'

class ADMIN {
  async SaveAgent (deep) {
    try {
      const res = await xhr({ url: '/Admin/SaveAgent', data: deep })

      if (!_.isObject(res) || res.response === false) {
        throw xhrErrors('v-login-error')
      }
      if (res.status !== 200) return Promise.reject(res.msg)

      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async CreateAgent (deep) {
    try {
      const res = await xhr({ url: '/Admin/CreateAgent', data: deep })

      if (!_.isObject(res) || res.response === false) {
        throw xhrErrors('v-login-error')
      }
      if (res.status !== 200) return Promise.reject(res.msg)

      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async GetAgentUserMoney (data) {
    try {
      const res = await xhr({ url: '/Admin/GetAgentUserMoney', data: data })

      if (!_.isObject(res) || res.response === false) {
        throw xhrErrors('v-login-error')
      }
      if (res.status !== 200) return Promise.reject(res.msg)


      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }
}

export default new ADMIN()