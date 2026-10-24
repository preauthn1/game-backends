
class OTHER {
  get token () {
    const token = [REDACTED]'token')
    return token
  }

  set token (result) {
    localStorage.setItem('token', result)
  }
}
export default new OTHER()
