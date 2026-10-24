

var strategies = {
    isNonEmpty: function (value, errorMsg) {
        if (value === '') {
            return errorMsg
        }
    },

    isSomeNonEmpty: function (values, errorMsg) {
        var value = values.some(function (item) {
            return item
        });
        if (!value) {
            return errorMsg
        }
    },
    minLength: function (value, length, errorMsg) {
        if (value.length < length) {
            return errorMsg
        }
    },
    isMobile: function (value, errorMsg) {
        if (!/(^1[3|5|8][0-9]{9}$)/.test(value)) {
            return errorMsg
        }
    }
}

var Validator = function () {
    this.cache = [];
}

Validator.prototype.add = function (dom, rules) {

    var self = this
    for (var i = 0, rule; rule = rules[i++];) {
        (function () {
            var strategyAry = rule.strategy.split(':')
            var errorMsg = rule.errorMsg

            self.cache.push(function () {
                var strategy = strategyAry.shift()

                if (Object.prototype.toString.call(dom) === '[object Array]') {
                    var values = dom.map((val) => {
                        return val ? val.value : ''
                    })
                    strategyAry.unshift(values)
                } else {
                    strategyAry.unshift(dom.value)
                }
                strategyAry.push(errorMsg)
                return strategies[strategy].apply(dom, strategyAry)
            })
        })(rule)
    }
}

Validator.prototype.start = function () {
    for (var i = 0, validatorFunc; validatorFunc = this.cache[i++];) {
        var msg = validatorFunc();
        if (msg) {
            return msg
        }
    }
}