
class ENCODE {
  pipe (dataObject) {
    try {
      if (this.check(dataObject)) throw new Error('no data')

      const result = this.dataPipe({dataObject})
      return result
    } catch (err) { return '' }
  }

  dataPipe ({dataKey = null, dataObject = null}) {
    try {
      if (this.check(dataObject)) throw new Error('no data')

      const valuedKeys = this.dataObjectFilter(dataObject)
      if (_.isEmpty(valuedKeys)) throw new Error('no data')

      const pipe = valuedKeys.map(key => {
        const value = dataObject[key]
        if (_.isObject(value)) return this.dataPipe({dataKey: key, dataObject: value})

        const resultKey = encodeURIComponent(key)
        const resultValue = encodeURIComponent(value)
        const normalPart = `${resultKey}=${resultValue}`
        const objectPart = `${dataKey}[${resultKey}]=${resultValue}`
        return _.isNull(dataKey) ? normalPart : objectPart
      })
      const result = pipe.join('&')
      return result
    } catch (err) { return '' }
  }

  check (dataObject) {
    try {
      if (_.isNull(dataObject) || _.isUndefined(dataObject)) throw new Error('no data')
      return false
    } catch (err) { return true }
  }

  dataObjectFilter (dataObject) {
    try {
      const valuedKeys = Object.keys(dataObject).filter(key => {
        return _.has(dataObject, key) && !_.isNull(dataObject[key]) && !_.isUndefined(dataObject[key])
      })
      return valuedKeys
    } catch (err) { return [] }
  }

  randomString (length) {
    try {
      length = length || 30
      let text = ''
      const possible = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789'
      for (var i = 0; i < length; i++) {
        text += possible.charAt(_.random(possible.length))
      }
      return text
    } catch (err) { return 'index' }
  }
}

export default new ENCODE()
