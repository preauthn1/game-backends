import { xhr } from './xhr/'
import { xhrErrors } from './xhr/config'

class TRANSFER {
  async GetTransferLog (data) {
    try {
      const res = await xhr({ url: '/Transfer/GetTransferLog', data: data })

      if (!_.isObject(res) || res.response === false) {
        throw xhrErrors('v-login-error')
      }

      if (res.status !== 200) return Promise.reject(res.msg)

      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async GetRechargeLog () {
    try {
      const res = await xhr({ url: '/Transfer/GetRechargeLog'})

      if (!_.isObject(res) || res.response === false) {
        throw xhrErrors('v-login-error')
      }

      if (res.status !== 200) return Promise.reject(res.msg)

      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async GetAgentRechargeLog (data) {
    try {
      const res = await xhr({ url: '/Transfer/GetAgentRechargeLog', data: data})

      if (!_.isObject(res) || res.response === false) {
        throw xhrErrors('v-login-error')
      }

      if (res.status !== 200) return Promise.reject(res.msg)

      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async GetAgentBackLog (data) {
    try {
      const res = await xhr({ url: '/Transfer/GetAgentBackLog', data: data})

      if (!_.isObject(res) || res.response === false) {
        throw xhrErrors('v-login-error')
      }

      if (res.status !== 200) return Promise.reject(res.msg)

      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async transferMoney (data) {
    try {
      const res = await xhr({ url: '/Transfer/transferMoney', data: data })

      if (!_.isObject(res) || res.response === false) {
        throw xhrErrors('v-login-error')
      }
      if (res.status !== 200) return Promise.reject(res.msg)
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async AgentOrderBack (data) {
    try {
      const res = await xhr({ url: '/Transfer/AgentOrderBack', data: data })

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

export default new TRANSFER()