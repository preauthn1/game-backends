
import store from '@/agentclub/store'
import axios from 'axios'
import encode from './encode'
import { API_URL, xhrErrors, XHR_TIMEOUT } from './config'

const httpClient = axios.create()
const CancelToken = [REDACTED]

const xhr = async ({url = '/', method = 'post', data = null, params = {}, token = [REDACTED], isCustomBaseURL = false, isLoadingIcon = true, timeout = XHR_TIMEOUT}) => {
  const xhrIndex = encode.randomString(8)
  _.set(params, 'cachePrefix', xhrIndex)
  this.isTimeout = false
  try {
    let cancel = () => {}
    if (!isCustomBaseURL) httpClient.defaults.baseURL = API_URL
    httpClient.defaults.headers.common['Content-Type'] = 'application/x-www-form-urlencoded'
    httpClient.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest'
    httpClient.crossDomain = true
    httpClient.withCredentials = true
    httpClient.defaults.cancelToken = [REDACTED] CancelToken(originCancel => { cancel = originCancel })
    httpClient.defaults.paramsSerializer = params => {
      const paramsKeys = _.keys(params)
      let paramsKeyValues = paramsKeys.map(key => key + '=' + params[key]).join('&')
      return paramsKeyValues
    }
    if (!_.isNull(token)) httpClient.defaults.headers.common['authorization'] = token
    if (data) httpClient.defaults.transformRequest = data => encode.pipe(data)
    store.dispatch('addXhrQueue', { url, cancel, xhrIndex, loadingIcon: isLoadingIcon })

    window.setTimeout(() => {
      this.isTimeout = true
      cancel()
    }, timeout)
    
    const res = await httpClient({ method, url, data, params })
    const result = _.get(res, 'data')
    const status = _.get(result, 'status')
    const message = _.get(result, 'msg')
    if(status !== 200) { store.dispatch('showSnackbar', {status: status, errorMsg: message })}

    return Promise.resolve(result)
  } catch (err) {
    const errorMsg = onError({err, isTimeout: this.isTimeout, isCancel: axios.isCancel(err)})
    store.dispatch('showSnackbar', { errorMsg: errorMsg }) 
    return Promise.reject(errorMsg)
  } finally {
    store.dispatch('removeXhrQueue', xhrIndex)
  }

  function onError ({err, isTimeout, isCancel}) {
    try {
      const status = _.get(err, 'response.status') || 500
      const message = _.get(err, 'response.data.message')
      if (!_.isUndefined(message) && _.isString(message)) return message
      if (isTimeout) return `${xhrErrors('v-timeout')}`
      if (isCancel) return `${xhrErrors('v-cancel')}`
      const statusToNumber = _.toNumber(status)
      if (statusToNumber === 403) { return store.dispatch('checkTokenAliveFromRemoteAndSetPermissions') }
      const errorMsg = subErrorMessagePipe(statusToNumber)
      return errorMsg
    } catch (err) { return err }
  }

  function subErrorMessagePipe (status) {
    try {
      const errMsg = xhrErrors(`http-${status}`)
      return errMsg
    } catch (err) {
      return xhrErrors('v-error-without-http-response')
    }
  }
}

export default xhr
