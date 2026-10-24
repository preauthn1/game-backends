import { xhr } from './xhr/'
import { xhrErrors } from './xhr/config'
const md5 = require('md5')

class AUTH {
  async login (data) {
    try {
      _.set(data, 'password', md5(data.password))
      const res = await xhr({ url: '/Admin/login', data })
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  async GetInformation () {
    try {
      const res = await xhr({ url: '/Agent/GetInformation' })
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
      const res = await xhr({ url: '/Admin/ValidateToken', method: 'GET', token:  [REDACTED]
      const data ={
        IsAuthenticated: _.get(res, 'IsAuthenticated'),
        UserType: _.get(res, 'Class'),
        LoginName: _.get(res, 'LoginName'),
        AllowWithdraw: _.get(res, 'AllowWithdraw')
      }
      return Promise.resolve(data)
    } catch (err) {
      return Promise.reject(`${err}`)
    }
  }

  SendVerificationCode = async () => {
    try {
      const res = await xhr({ url: '/SMS/SendVerificationCode'})
      return Promise.resolve(res)
    } catch (err) {
      return Promise.reject(`${err}`)
    }
  }
}

export default new AUTH()