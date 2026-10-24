import { xhr } from './xhr/'
import { xhrErrors } from './xhr/config'

class MANAGEMENT {
  async GetAgent (data) {
    try {
      const res = await xhr({ url: '/Admin/GetAgent', data: data })

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

export default new MANAGEMENT()