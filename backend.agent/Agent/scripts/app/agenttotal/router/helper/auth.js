
class AUTH_HELPER {
  isRouteNameIsOneOfAuth = ({ name }) => {
    return name === 'login' || name === 'logout'
  }
}

export default new AUTH_HELPER()
