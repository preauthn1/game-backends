//常用方法 
(function (w) {
    w.DelConfirmUrl = function (url, msg) {
        var message = msg || "确定要删除此数据吗?"
        if (confirm(message)) {
            window.location.href = url;
        }
        return false;
    };
    //检查值是否为Null,空串等
    w.IsEmpty = function (val) {
        if (typeof (val) == "undefined") {
            return true;
        } else if (val == null || /^\s*$/i.test(val)) {
            return true;
        }
        return false;
    };
    //类似SQL中的IsNull,nullvalue 为空时的值，不为空时的值(默认值:参数d)
    w.IsNull = function (d, nullvalue, notnullvalue) {
        return d == undefined || /^\s*$/.test(d) ? nullvalue : notnullvalue == undefined ? d : notnullvalue;
    }
    w.ToCash = function (val) {
        return (val / webconfig.coinShowRate).toFixed(2);
    }

    w.swDelConfirmUrl = function (url, msg, option) {

        var message = msg || "确定要删除此数据吗?"
        var methodType = option ? option.method || "get" : "get"
        var isAjax = option ? option.isAjax : false
        var data = option ? option.data : {}
        var isReload = option ? option.isReload : true
        var callback = option ? option.callback : {}

        swal({
            title: "你确定吗?",
            text: message,
            type: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#DD6B55',
            confirmButtonText: '确定',
            cancelButtonText: '取消',
            allowOutsideClick: false,
            reverseButtons: true
        }).then((result) => {
            if (result.value) {
                if (isAjax) {
                    AjaxGetJson(url, methodType, data, isReload, callback)
                } else {
                    window.location.href = url;
                }
            }
        })
    }
})(window);


/*数组扩展方法*/
(function () {
    ArrayEx = function (array) {
        if (typeof array != "undefined") {
            for (var i = 0, l = array.length; i < l; i++) {
                this.push(array[i]);
            }
        }
    };
    ArrayEx.prototype = new Array();
    //添加数组元素,和push的区别是有自动排序功能,val是添加的对象,fun当是数值类型时可以忽略,如果是一个对象,可以指定一个排序规则,如:list.Add({a:1,b:"张三"},function(a,b){ return a.a-b.a;}),这样会按对象的a属性进行排序
    //支持:排序功能必须是数据的所有元素是相同对象结构
    ArrayEx.prototype.Add = function (val, fun) {
        this.push(val);
        this.sort(function (a, b) {
            if (typeof (fun) == "function") {
                return fun(a) < fun(b) ? -1 : fun(a) == fun(b) ? 0 : 1;
            } else {
                return a - b;
            }
        });

    };

    //添加多个元素
    ArrayEx.prototype.AddRang = function (rangdata) {
        if (rangdata || rangdata.constructor == Array) {
            for (var i = 0, l = rangdata.length; i < l; i++) {
                this.push(rangdata[i]);
            }
        }
    };
    //排序
    ArrayEx.prototype.Sort = function (fieldname, isasc) {
        this.sort(function (a, b) {
            if (isasc) {
                return a[fieldname] > b[fieldname];
            } else {
                return a[fieldname] < b[fieldname];
            }
        });
    }

    //判断数组中是否存在指定对象
    ArrayEx.prototype.IsExists = function (val) {
        for (var i = 0; i < this.length; i++) {
            if (typeof (val) == "function" && val(this[i])) {
                return true;
            }
            else {
                if (this[i] == val) {
                    return true;
                }
            }
        }
        return false;
    };

    //添加或更新指定条件的元素
    ArrayEx.prototype.AddOrUpdate = function (data, conditionFun) {
        if (typeof (conditionFun) == "function") {
            var isExists = false;
            for (var i = 0, l = this.length; i < l; i++) {
                if (conditionFun(this[i])) {
                    isExists = true;
                    this[i] = data;
                    break;
                }
            }
            if (!isExists) {
                this.push(data);
            }
        }
    };

    //移除数组中指定对象
    ArrayEx.prototype.Remove = function (val) {
        for (var i = 0; i < this.length; i++) {
            var flag = false;
            switch (typeof (val)) {
                case "function":
                    if (val(this[i])) {
                        this.splice(i, 1);
                        i--;    //因为删除一个元素后数组的长度减少了,防止漏掉余下元素的判断所以i需要减1
                    }
                    break;
                default:
                    if (val == this[i]) { this.splice(i, 1); i-- };
                    break;
            }
        }
    }

    //根据数组元素的其中一个属性作为条件获取元素,如:var list=[{id:1,name:"张三"},{id:2,name:"李四"}]; list.FindAll(function(l){return l.id=2;}); 这样就可以返回id=2的数组元素.
    ArrayEx.prototype.FindAll = function (val) {
        var list = new ArrayEx();

        for (var i = 0; i < this.length; i++) {
            if (typeof (val) != "function" && val == this[i]) {
                list.push(this[i]);
            }
            else if (typeof val == "function" && val(this[i])) {
                list.push(this[i]);
            }
        } return list;
    };
    ArrayEx.prototype.Select = function (fun) {
        var list = new Array();
        for (var i = 0, length = this.length; i < length; i++) {
            list.push(fun(this[i]));
        }
        return list;
    };
    //根据数组元素的其中一个属性作为条件获取第一个符合条件的元素
    ArrayEx.prototype.First = function (val) {
        var list = this.FindAll(val);
        if (list.length > 0) {
            return list[0];
        }
        else {
            return null;
        }
    };

    //返回 排除list数据以外的数据数组
    ArrayEx.prototype.ExcludeArray = function (list) {
        var val = this.concat();
        for (var i = 0; i < list.length; i++) {
            if (this.IsExists(list[i])) {
                val.Remove(list[i]);
            }
        }
        return this.length > 0 && typeof (this[0]) == "number" ? val.sort(function (a, b) { return a - b; }) : val;
    };

    //对某属性汇总 name属性名
    ArrayEx.prototype.Sum = function (val) {
        var sum = 0;
        for (var i = 0; i < this.length; i++) {
            if (val && typeof (val) == "function") {
                sum = sum ? sum + val(this[i]) : val(this[i]);
            }
            if (!val) { sum = sum | 0 + this[i]; }
        }
        return sum;
    };

    //取数组中某属性最大值,如list.Max(),list.Max(function(a){return a.id;})
    ArrayEx.prototype.Max = function (val) {
        var max;
        for (var i = 0; i < this.length; i++) {
            if (val && typeof (val) == "function") {
                max = max ? (max < val(this[i]) ? val(this[i]) : max) : val(this[i]);
            }
            else {
                max = max ? max < this[i] ? this[i] : max : this[i];
            }
        }
        return max || null;
    };
    ArrayEx.prototype.Min = function (val) {
        var min;
        for (var i = 0; i < this.length; i++) {
            if (val && typeof (val) == "function") {
                min = min ? (min > val(this[i]) ? val(this[i]) : min) : val(this[i]);
            }
            else {
                min = min ? (min > this[i] ? this[i] : min) : this[i];
            }
        }
        return min || null;
    };

    //遍历数组元素,只有当元素是对象才有意义,示例:对所有年龄大于小于2的对象加1年. var a = [{age:1},{age:2}];a.Each(function(o){if(o.age<2){o.age=o.age+1;}});
    ArrayEx.prototype.Each = function (fun) {
        for (var i = 0; i < this.length; i++) {
            var result = fun(this[i], i);
            /*为了提高性能，如果在执行次遍历后不需要再循环时可以返回一个break字符*/
            if (typeof result != "undefined" && !result) { break; }
        }
    }

    //获取多个不重复随机数,start开始数值， end结束数值，count 随机数个数，isrepeat 是否重复(默认false)，issort 是否自动排序(默认为true) 注意：当加上间隔数大于结束数值是不会加上end数值元素
    ArrayEx.prototype.GetRandom = function (start, end, count, isrepeat, issort) {
        if (issort == undefined) { issort = true; }
        if (isrepeat == undefined) { isrepeat = false; }
        var array = [];
        if (count > end - start + 1 && !isrepeat) {
            count = end - start + 1;
        }

        for (var i = 0; i < count; i++) {
            var r = new Number().Random(start, end); //生成了一个随机数            
            if (isrepeat) {
                if (issort) {
                    array.Add(r);
                } else { array.push(r); }
            }
            else {
                if (!array.IsExists(r)) {
                    if (issort) {
                        array.Add(r);
                    } else { array.push(r); }
                }
                else { i--; }
            }
        }
        return array;
    };

    /*copy一个新的数组,fun自定义方法*/
    ArrayEx.prototype.ToList = function (fun) {
        var list = new Array();
        for (var i = 0, length = this.length; i < length; i++) {
            list.push(fun(this[i]));
        }
        return list;
    };
    /*获取元素的索引值*/
    ArrayEx.prototype.Index = function (paras) {
        for (var i = 0, length = this.length; i < length; i++) {
            if (typeof (paras) == "function" && paras(this[i])) { return i; }
            else if (this[i] == paras) {
                return i;
            }
        }
    };
    /*返回一个数组的组合结果集,以二维数组方式返回,num几位数的组合,addfun(ary,list)添加组合的函数,将替换掉原来的添加操作,ary添加的组合数组,list返回的数组集合*/
    ArrayEx.prototype.CToList = function (num, addfun) {
        var add = function (ary) { if (typeof (addfun) == "function") { addfun(ary, list); } else { list.push(ary); } };
        var list = new Array();
        var thislist = this.concat();
        var CResult = function (ary1, ary2) {
            var listl = ary1.slice(0);
            var listr = ary2.slice(0);
            var needl = num - listl.length;     //还需要多少个元素构成组合

            switch (needl) {
                case 0:
                    add(listl.concat());
                    return list.push(listl);
                    break;
                case 1:
                    listr.Each(function (o) {
                        var clist = listl.slice(0);
                        clist.push(o);
                        add(clist.concat());
                    });
                    break;
                default:
                    listl.push(listr.shift());
                    CResult(listl, listr);
                    if (listl.length + listr.length > num) {
                        CResult(ary1, listr);
                    }
                    break;
            }
        };
        var listL = [thislist.shift()];
        CResult(listL, thislist);
        while (thislist.length >= num) {
            listL = [thislist.shift()];
            CResult(listL, thislist);
        }

        return list;
    };
    /*返回一个复式数组的组合结果集,一个组合为一个元素,如list = [[1,2,3],[1]] 将返回:[[1,1],[2,1],[3,1]] 这个组合数组,addfun(ary,list)添加组合的函数,将替换掉原来的添加操作,ary添加的组合数组,list返回的数组集合*/
    ArrayEx.prototype.PluralCToList = function (addfun) {
        var list = this.slice(0);
        var resultlist = new ArrayEx();
        var add = function (ary) { if (typeof (addfun) == "function") { addfun(ary, resultlist); } else { resultlist.push(ary); } };
        var combin = function (left, right) {
            left.Each(function (lval) {
                var newright = right.slice(0);
                var newleft = new ArrayEx();
                newright.shift().Each(function (o) {
                    if (newright.length > 0) {
                        combin(new ArrayEx([new Array().concat(lval, [o])]), newright);
                    }
                    else {
                        add(new ArrayEx(new Array().concat(lval, o)));
                    }
                });
            });
        };
        combin(list.shift(), list);
        return resultlist;
    };

})();

//字符串扩展
(function () {
    String.prototype.format = function () {
        var str = this;
        for (var i = 0, l = arguments.length; i < l; i++) {
            var reg = new RegExp("\\{" + i + "\\}", "g");
            str = str.replace(reg, arguments[i]);
        };
        return str.replace(/\{\d+\}/g, "");
    };
    String.prototype.BindData = function (obj) {
        var str = this;
        for (var key in obj) {
            var reg = new RegExp("\\{" + key + "\\}", "g");
            str = str.replace(reg, obj[key]);
        };
        return str.replace(/\{[^\}]+\}/g, "");
    };
})();

/**
* 填充表单数据
*/
function fillFormData(formId) {
    var url = location.search;
    var ps = {};
    if (url.indexOf("?") != -1) {
        var str = url.substr(1);
        strArr = str.split("&");
        for (var i = 0; i < strArr.length; i++) {
            var pArr = strArr[i].split("=");
            var pname = pArr[0];
            var pvalue = pArr[1];

            pvalue = decodeURIComponent(pvalue);
            pvalue = ShengUtils.decode(pvalue);
            ps[pname] = pvalue;
        }
    }

    for (var p in ps) {
        var selector = "#" + p;
        $(selector).val(ps[p].toLowerCase());
    }

}

function getQueryString(name) {
    var reg = new RegExp('(^|&)' + name + '=([^&]*)(&|$)', 'i');
    var r = window.location.search.substr(1).match(reg);
    if (r != null) {
        return unescape(r[2]);
    }
    return null;
}

function AjaxGetJson(url, method, data, isReload, callback) {
    $.ajax({
        cache: false,
        dataType: 'json',
        url: url,
        data: data,
        contentType: "application/x-www-form-urlencoded",
        type: method,
        beforeSend: function () {

        },
        success: function (val) {
            var status = val.status
            swal({
                type: status ? 'success' : 'error',
                title: val.msg,
            }).then((result) => {
                if (isReload) {
                    window.location.reload()
                } else if ({}.toString.call(callback) === '[object Function]') {
                    callback()
                }
            })
        },
        error: function (err) {
            swal({
                type: 'error',
                title: '操作失败',
                text: err
            })
        }
    });
}

function groupBy(list, keyGetter) {
    const map = new Map();
    list.forEach((item) => {
        const key = keyGetter(item);
        const collection = map.get(key);
        if (!collection) {
            map.set(key, [item]);
        } else {
            collection.push(item);
        }
    });
    return map;
}


$(function () {
    $(".tooglebtns .btn").click(function () {
        $(this).siblings().removeClass("blue").removeClass("active");
        $(this).addClass("blue").addClass("active");
    });
})

function initToogleButton(selector, index) {
    $(selector).removeClass("blue").removeClass("active");
    $(selector).eq(index).addClass("blue").addClass("active").children("input").attr("checked", "checked");
}

function urlDecode(zipStr) {
    var uzipStr = "";
    for (var i = 0; i < zipStr.length; i++) {
        var chr = zipStr.charAt(i);
        if (chr == "+") {
            uzipStr += " ";
        } else if (chr == "%") {
            var asc = zipStr.substring(i + 1, i + 3);
            if (parseInt("0x" + asc) > 0x7f) {
                uzipStr += decodeURI("%" + asc.toString() + zipStr.substring(i + 3, i + 9).toString());;
                i += 8;
            } else {
                uzipStr += ShengUtils.AsciiToString(parseInt("0x" + asc));
                i += 2;
            }
        } else {
            uzipStr += chr;
        }
    }
    return uzipStr;
}

/** 
* Url编码 
**/
var ShengUtils = {};
ShengUtils.encode = function (unzipStr) {
    var zipstr = "";
    var strSpecial = "!\"#$%&'()*+,/:;<=>?[]^`{|}~%";
    var tt = "";
    for (var i = 0; i < unzipStr.length; i++) {
        var chr = unzipStr.charAt(i);
        var c = ShengUtils.StringToAscii(chr);
        tt += chr + ":" + c + "n";
        if (parseInt("0x" + c) > 0x7f) {
            zipstr += encodeURI(unzipStr.substr(i, 1));
        } else {
            if (chr == " ")
                zipstr += "+";
            else if (strSpecial.indexOf(chr) != -1)
                zipstr += "%" + c.toString(16);
            else
                zipstr += chr;
        }
    }
    return zipstr;
}

/** 
* Url解码 
**/
ShengUtils.decode = function (zipStr) {
    var uzipStr = "";
    for (var i = 0; i < zipStr.length; i++) {
        var chr = zipStr.charAt(i);
        if (chr == "+") {
            uzipStr += " ";
        } else if (chr == "%") {
            var asc = zipStr.substring(i + 1, i + 3);
            if (parseInt("0x" + asc) > 0x7f) {
                uzipStr += decodeURI("%" + asc.toString() + zipStr.substring(i + 3, i + 9).toString());;
                i += 8;
            } else {
                uzipStr += ShengUtils.AsciiToString(parseInt("0x" + asc));
                i += 2;
            }
        } else {
            uzipStr += chr;
        }
    }
    return uzipStr;
}

ShengUtils.StringToAscii = function (str) {
    return str.charCodeAt(0).toString(16);
}

ShengUtils.AsciiToString = function (asccode) {
    return String.fromCharCode(asccode);
}


//table:jquery Table 对象
//tdProgressClass:数字列进度条的Class
//width:宽度默认100
//type:类型 1.按最大值显示百分比     2.按合计显示百分比   默认为1
//文档加载完成之后调用
function ShowProgress(table, tdProgressClass, width, type) {
    return;
    width = width || 100;
    type = type || 1;

    var num = 0;
    var nums = [];
    var maxNum1 = 0, maxNum2 = 0, sumNum1 = 0, sumNum2 = 0;

    table.find("tbody td." + tdProgressClass).each(function () {
        num = Number($(this).text().replace(/,/g, "").replace("¥", "").replace("￥", "")) || 0;
        nums.push(num);
        if (num > 0) {
            if (num > maxNum1) maxNum1 = num;
            sumNum1 += num;
        }
        else if (num < 0) {
            if (num < maxNum2) maxNum2 = num;
            sumNum2 += num;
        }
    });

    var i = 0;
    var html = '<div class="progress" style="width:' + width + 'px;margin-bottom: 0px;"><div class="progress-bar progress-bar-{color}" role="progressbar" style="width: {progress}%;color:#d9534f;text-align:left;">{amount}</div></div>';

    var color, progress;
    table.find("tbody td." + tdProgressClass).each(function () {
        $(this).width();
        if (nums[i] > 0) {
            color = "success";
            progress = type == 1 ? (nums[i] / maxNum1) * 100 : (nums[i] / sumNum1) * 100;
        }
        else {
            color = "info";
            progress = type == 1 ? (Math.abs(nums[i]) / Math.abs(maxNum2)) * 100 : (Math.abs(nums[i]) / Math.abs(sumNum2)) * 100;
        }

        $(this).html(html.replace("{color}", color).replace("{progress}", progress).replace("{amount}", $(this).text()));
        i++;
    });

}


//报警
$(document).ready(function () {
    setTimeout("ajaxWarning2();", 1000 * 15);
});

function ajaxWarning2() {
    ajaxWarning();
    setInterval("ajaxWarning();", 1000 * 30);
}

function ajaxWarning() {
    $.get('/Warning/ExistWarning', {}, function (result) {
        $(".nav-warning").remove();
        if (result.toLowerCase() == "true") {
            $(".cus_topnav").after('<div class="nav-warning" style="background-color: rgb(237, 78, 42);display: inline-block;width:100px;height:62px;"><span class="label label-danger"></span></div>');
            $(".nav-warning").css({ "-webkit-animation": "twinkling 2s infinite 0.9s ease-in-out alternate", "animation": "twinkling 2s infinite 0.9s ease-in-out alternate" });
        }
    });
}

var dataTable = {};

function createDataTable(tag, columns, url, data, callback, option) {
    const ajax_data = Object.assign({ '_r': Math.random() }, data)
    const pageSize = option ? option.pageSize || 100 : 100
    const LengthMenu = option ? option.LengthMenu || [100, 200, 500, 1000] : [100, 200, 500, 1000]
    const aaSorting = option ? option.aaSorting || [[0, "asc"]] : [[0, "asc"]]

    var isOrder = columns.map((item, index, array) => {
        item.bSortable = item.bSortable || false
        return item
    })
    const opt = {
        "searching": false,
        "aoColumns": columns,
        "aaSorting": aaSorting,
        "processing": true,
        "serverSide": true,
        "aLengthMenu": LengthMenu,
        "iDisplayLength": pageSize,
        "ajax": {
            "type": "POST",
            "url": url,
            "data": ajax_data
        },
        "oLanguage": {
            "sLengthMenu": "显示 _MENU_ 记录",
            "sZeroRecords": "对不起，查询不到任何相关数据",
            "sEmptyTable": "未有相关数据",
            "sLoadingRecords": "正在加载数据-请等待...",
            "sInfo": "当前显示 _START_ 到 _END_ 条，共 _TOTAL_ 条记录。",
            "sInfoEmpty": "当前显示0到0条，共0条记录",
            "sInfoFiltered": "（数据库中共为 _MAX_ 条记录）",
            "sProcessing": "正在加载数据...",
            "sSearch": "模糊查询：",
            "sUrl": "",
            "oPaginate": {
                "sFirst": "首页",
                "sPrevious": " 上一页 ",
                "sNext": " 下一页 ",
                "sLast": " 尾页 "
            }
        },
        "destroy": true,
        "initComplete": function (settings, json) {
            if (callback) callback(json)
        }
    }
    dataTable = $(`#${tag}_table`).DataTable(opt)
}