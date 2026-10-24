// JavaScript Document
function hasTouch() {
    var touchObj = {};
    touchObj.isSupportTouch = "ontouchend" in document ? true : false;
    touchObj.isEvent = touchObj.isSupportTouch ? 'touchend' : 'click';
    return touchObj.isEvent;
}
//公共验证
var publicVlidator = {
    //长度（汉字为2个字节）
    strMaxLength: function (value, maxlength) {
        var length = value.length;
        for (var i = 0; i < value.length; i++) {
            if (value.charCodeAt(i) > 127) {
                length++;
            }
        }
        return length <= maxlength;
    },
    isMobile: function (value) {//手机号码
        var regExp = /^1[3|5|4|7|8]\d{9}$/;
        return (regExp.test(value));
    },
    isTelephone: function (value) {//固定电话和传真机
        var regExp = /^[+]{0,1}(\d){1,3}[ ]?([-]?((\d)|[ ]){1,12})+$/;
        return (regExp.test(value));
    },
    chrnum: function (value) {
        //只能输入数字和字母(字符A-Z, a-z, 0-9)
        var regExp = /^([a-zA-Z0-9]+)$/;
        return (regExp.test(value));
    },
    chinese: function (value) {
        //只能输入中文数字和字母(字符A-Z, a-z, 0-9)
        var regExp = /^([\u4e00-\u9fa5]|[a-z]|[A-Z]|[0-9])+$/;
        return (regExp.test(value));
    },
    QQ: function (value) {
        var regExp = /^[1-9]\d{4,9}$/;
        return (regExp.test(value));
    },
    isWechat: function (value) {
        var regExp = /^[a-zA-Z][-_a-zA-Z0-9]{5,19}$/;
        return (regExp.test(value));
    },
    number: function (value) {
        var regExp = /^[0-9]+$/;
        return (regExp.test(value));
    },
    decimal: function (value, len) {
        //len小数点精确位数
        //var testVal = Number(value);
        if (typeof (len) == undefined || len == 0) {
            var regExp = /^\d+$/;
        } else {
            var regxStr = "^(\\d){1,6}(\\.\\d{1," + len + "})?$";
            var regExp = new RegExp(regxStr);
        }
        return (regExp.test(value));
    },
    pattern: function (value) {
        var regExp = /"[\\pP‘’“”]", ""/;
        return (regExp.test(value));
    },
    chinaSingle: function (value) {
        //只能中文字符
        var regExp = /^[\u4e00-\u9fa5]+$/;
        return (regExp.test(value));
    }
};

///插件封装提示
function layeralert(cont) {
    layer.open({
        content: cont, skin: 'msg', time: 2000//2秒后自动关闭
    });
}
//body禁止滚动
function body_no() {
    $('body').css('overflow', 'hidden');
}
//body解除禁止滚动
function body_yes() {
    $('body').css('overflow', 'auto');
}
function statusRender(cellvalue, options, rowObject) {
    if(cellvalue==1){
        return '<span class="label label-lg label-success" data-value="'+cellvalue+'">开启('+cellvalue+')</span>';
    }else {
        return '<span class="label label-lg label-danger" data-value="'+cellvalue+'">关闭('+cellvalue+')</span>';
    }
}
function aceSwitch( cellvalue, options, cell ) {
    setTimeout(function(){
        $(cell) .find('input[type=checkbox]')
            // .wrap('<label class="inline" />')
            .addClass('ace ace-switch ace-switch-5')
            .after('<span class="lbl"></span>');
    }, 0);
    return $(cell).find('span').attr('data-value');
}
function pickDate( cellvalue, options, cell ) {
    setTimeout(function(){
        $(cell) .find('input[type=text]')
            .datepicker({format:'yyyy-mm-dd' , autoclose:true});
    }, 0);
}
function style_edit_form(form) {
    //dialog align center
    var dialog = form.closest('.ui-jqdialog');
    var parentDiv = dialog.parent();
    // dialog[0].example.invalid = Math.round((parentDiv.height()-dialog.height())/2) + "px";
    dialog[0].style.left = Math.round((parentDiv.width()-dialog.width())/2) + "px";
    //change checkbox style to switch
    form .find('input[type=checkbox]')
        .addClass('ace ace-switch ace-switch-5')
        .after('<span class="lbl"></span>');
    //enable datepicker on "sdate" field and switches for "stock" field
    // form.find('input[name=sdate]').datepicker({format:'yyyy-mm-dd' , autoclose:true}).end();

    //update buttons classes
    var buttons = form.next().find('.EditButton .fm-button');
    buttons.addClass('btn btn-sm').find('[class*="-icon"]').remove();//ui-icon, s-icon
    buttons.eq(0).find('.icon-ok').remove();
    buttons.eq(1).find('.icon-remove').remove();
    buttons.eq(0).addClass('btn-primary').prepend('<i class="icon-ok"></i>');
    buttons.eq(1).prepend('<i class="icon-remove"></i>');

    buttons = form.next().find('.navButton a');
    buttons.find('.ui-icon').remove();
    buttons.eq(0).append('<i class="icon-chevron-left"></i>');
    buttons.eq(1).append('<i class="icon-chevron-right"></i>');
}

function style_delete_form(form) {
    var dialog = form.closest('.ui-jqdialog');
    var parentDiv = dialog.parent();
    // dialog[0].example.invalid = Math.round((parentDiv.height()-dialog.height())/2) + "px";
    dialog[0].style.left = Math.round((parentDiv.width()-dialog.width())/2) + "px";

    var buttons = form.next().find('.EditButton .fm-button');
    buttons.addClass('btn btn-sm').find('[class*="-icon"]').remove();//ui-icon, s-icon
    buttons.eq(0).addClass('btn-danger').prepend('<i class="icon-trash"></i>');
    buttons.eq(1).prepend('<i class="icon-remove"></i>')
}

function style_search_filters(form) {
    form.find('.delete-rule').val('X');
    form.find('.add-rule').addClass('btn btn-xs btn-primary');
    form.find('.add-group').addClass('btn btn-xs btn-success');
    form.find('.delete-group').addClass('btn btn-xs btn-danger');
}
function style_search_form(form) {
    var dialog = form.closest('.ui-jqdialog');
    var buttons = dialog.find('.EditTable');
    buttons.find('.EditButton a[id*="_reset"]').addClass('btn btn-sm btn-info').find('.ui-icon').attr('class', 'icon-retweet');
    buttons.find('.EditButton a[id*="_query"]').addClass('btn btn-sm btn-inverse').find('.ui-icon').attr('class', 'icon-comment-alt');
    buttons.find('.EditButton a[id*="_search"]').addClass('btn btn-sm btn-purple').find('.ui-icon').attr('class', 'icon-search');

    var parentDiv = dialog.parent();
    // dialog[0].example.invalid = Math.round((parentDiv.height()-dialog.height())/2) + "px";
    dialog[0].style.left = Math.round((parentDiv.width()-dialog.width())/2) + "px";
}

function beforeDeleteCallback(e) {
    var form = $(e[0]);
    if(form.data('styled')) return false;

    form.closest('.ui-jqdialog').find('.ui-jqdialog-titlebar').wrapInner('<div class="widget-header" />')
    style_delete_form(form);

    form.data('styled', true);
}

function beforeEditCallback(e) {
    var form = $(e[0]);
    form.closest('.ui-jqdialog').find('.ui-jqdialog-titlebar').wrapInner('<div class="widget-header" />')
    style_edit_form(form);
}



//it causes some flicker when reloading or navigating grid
//it may be possible to have some custom formatter to do this as the grid is being created to prevent this
//or go back to default browser checkbox styles for the grid
function styleCheckbox(table) {
    $(table).find('input:checkbox').addClass('ace')
        .wrap('<label />')
        .after('<span class="lbl align-top" />')


    $('.ui-jqgrid-labels th[id*="_cb"]:first-child')
        .find('input.cbox[type=checkbox]').addClass('ace')
        .wrap('<label />').after('<span class="lbl align-top" />');
}


//unlike navButtons icons, action icons in rows seem to be hard-coded
//you can change them like this in here if you want
function updateActionIcons(table) {
    /**
     var replacement =
     {
         'ui-icon-pencil' : 'icon-pencil blue',
         'ui-icon-trash' : 'icon-trash red',
         'ui-icon-disk' : 'icon-ok green',
         'ui-icon-cancel' : 'icon-remove red'
     };
     $(table).find('.ui-pg-div span.ui-icon').each(function(){
			var icon = $(this);
			var $class = $.trim(icon.attr('class').replace('ui-icon', ''));
			if($class in replacement) icon.attr('class', 'ui-icon '+replacement[$class]);
		})
     */
}

//replace icons with FontAwesome icons like above
function updatePagerIcons(table) {
    var replacement =
        {
            'ui-icon-seek-first' : 'icon-double-angle-left bigger-140',
            'ui-icon-seek-prev' : 'icon-angle-left bigger-140',
            'ui-icon-seek-next' : 'icon-angle-right bigger-140',
            'ui-icon-seek-end' : 'icon-double-angle-right bigger-140'
        };
    $('.ui-pg-table:not(.navtable) > tbody > tr > .ui-pg-button > .ui-icon').each(function(){
        var icon = $(this);
        var $class = $.trim(icon.attr('class').replace('ui-icon', ''));

        if($class in replacement) icon.attr('class', 'ui-icon '+replacement[$class]);
    })
}

function enableTooltips(table) {
    $('.navtable .ui-pg-button').tooltip({container:'body'});
    $(table).find('.ui-pg-div').tooltip({container:'body'});
}

$(document).ajaxError(function(e,xhr,opt){
    layeralert(xhr.statusText + " requesting " + opt.url);
});
function  hideScroll(grid_selector) {
    var w2 = parseInt($('.ui-jqgrid-labels>th:eq(2)').css('width'))-0.01;
    $('.ui-jqgrid-lables>th:eq(2)').css('width',w2);
    $(grid_selector+' tr').find("td:eq(2)").each(function(){
        $(this).css('width',w2);
    });
}
function add_time_search() {
    if($("#search").length==0){
        $(".ui-jqgrid-titlebar").append('<div class="row" style="margin-top: 2.5px;"><div class="col-xs-8 col-sm-2"><div class="input-group"><span class="input-group-addon"><i class="icon-calendar bigger-110"></i></span><input class="form-control" type="text" name="date-range-picker" id="id-date-range-picker" placeholder="时间范围"></div></div><input id="search" class="btn btn-purple btn-sm" type="button" value="查  询" /></div>');
        $('input[name=date-range-picker]').daterangepicker().prev().on("click", function(){
            $(this).next().focus();
        });
    }
}
function search_time_onclick(timeField) {
    $("body").on("click","#search",function () {
        var date=$("#id-date-range-picker").val();
        var dataArr=date.split(" - ");
        var grid = $("#grid-table");
        if (dataArr.length === 0||dataArr[0]=="") {
            grid[0].p.search = false;
            $.extend(grid[0].p.postData,{filters:""});
            return ;
        }
        var f = {groupOp:"AND",rules:[]};
        f.rules.push({field:timeField,op:"ge",data:dataArr[0]});
        f.rules.push({field:timeField,op:"le",data:dataArr[1]});
        grid[0].p.search = true;
        $.extend(grid[0].p.postData,{filters:JSON.stringify(f)});
        grid.trigger("reloadGrid",[{page:1,current:true}]);
    });
}
$(document).on("click","#search",function () {
});

function addJqGridFormSearch(){
    var dataArr =$("#search").closest("form").serializeArray();
    var grid = $("#grid-table");
    if (dataArr.length === 0) {
        grid[0].p.search = false;
        $.extend(grid[0].p.postData,{filters:""});
    }
    var f = {groupOp:"AND",rules:[]};
    for (var v in dataArr) {
        if(dataArr[v]['value']){
            var dateArr = dataArr[v]['value'].split(" - ");
            if(dateArr.length>=2){
                f.rules.push({field:dataArr[v]['name'],op:"ge",data:dateArr[0]});
                f.rules.push({field:dataArr[v]['name'],op:"le",data:dateArr[1]});
            }else{
                f.rules.push({field:dataArr[v]['name'],op:"eq",data:dataArr[v]['value']});
            }
        }
    }
    grid[0].p.search = true;
    $.extend(grid[0].p.postData,{filters:JSON.stringify(f)});
    grid.jqGrid('setGridParam',{datatype:'json'}).trigger("reloadGrid",[{page:1,current:true}]);
}

function dateRangePicker(){
    $('.date-daterangepicker').daterangepicker();
}

function dateTimeRangePicker(){
    $('.datetimestamp-daterangepicker').daterangepicker({
            showDropdowns: false,
            showWeekNumbers: false,
            timePicker: true,
            timePickerIncrement: 1,
            singleDatePicker: true,
            timePicker12Hour: false,
            locale: {
            format: 'YYYY-MM-DD HH:mm:ss',
            daysOfWeek: ['日', '一', '二', '三', '四', '五', '六'],
                  monthNames: ['一月', '二月', '三月', '四月', '五月', '六月', '七月', '八月', '九月', '十月', '十一月', '十二月'],
            },
            opens: 'left',
            buttonClasses: ['btn btn-default'],
            applyClass: 'btn-small btn-primary',
            cancelClass: 'btn-small',
            format: 'YYYY-MM-DD HH:mm:ss',
    });
}

//时间戳格式化
function dateFormat(format, timestamp){ 
     var a, jsdate=((timestamp) ? new Date(timestamp*1000) : new Date()); 
     var pad = function(n, c){ 
      if((n = n + "").length < c){ 
       return new Array(++c - n.length).join("0") + n; 
      } else { 
       return n; 
      } 
     }; 
     var txt_weekdays = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"]; 
     var txt_ordin = {1:"st", 2:"nd", 3:"rd", 21:"st", 22:"nd", 23:"rd", 31:"st"}; 
     var txt_months = ["", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"]; 
     var f = { 
      // Day 
      d: function(){return pad(f.j(), 2)}, 
      D: function(){return f.l().substr(0,3)}, 
      j: function(){return jsdate.getDate()}, 
      l: function(){return txt_weekdays[f.w()]}, 
      N: function(){return f.w() + 1}, 
      S: function(){return txt_ordin[f.j()] ? txt_ordin[f.j()] : 'th'}, 
      w: function(){return jsdate.getDay()}, 
      z: function(){return (jsdate - new Date(jsdate.getFullYear() + "/1/1")) / 864e5 >> 0}, 
       
      // Week 
      W: function(){ 
       var a = f.z(), b = 364 + f.L() - a; 
       var nd2, nd = (new Date(jsdate.getFullYear() + "/1/1").getDay() || 7) - 1; 
       if(b <= 2 && ((jsdate.getDay() || 7) - 1) <= 2 - b){ 
        return 1; 
       } else{ 
        if(a <= 2 && nd >= 4 && a >= (6 - nd)){ 
         nd2 = new Date(jsdate.getFullYear() - 1 + "/12/31"); 
         return date("W", Math.round(nd2.getTime()/1000)); 
        } else{ 
         return (1 + (nd <= 3 ? ((a + nd) / 7) : (a - (7 - nd)) / 7) >> 0); 
        } 
       } 
      }, 
       
      // Month 
      F: function(){return txt_months[f.n()]}, 
      m: function(){return pad(f.n(), 2)}, 
      M: function(){return f.F().substr(0,3)}, 
      n: function(){return jsdate.getMonth() + 1}, 
      t: function(){ 
       var n; 
       if( (n = jsdate.getMonth() + 1) == 2 ){ 
        return 28 + f.L(); 
       } else{ 
        if( n & 1 && n < 8 || !(n & 1) && n > 7 ){ 
         return 31; 
        } else{ 
         return 30; 
        } 
       } 
      }, 
       
      // Year 
      L: function(){var y = f.Y();return (!(y & 3) && (y % 1e2 || !(y % 4e2))) ? 1 : 0}, 
      //o not supported yet 
      Y: function(){return jsdate.getFullYear()}, 
      y: function(){return (jsdate.getFullYear() + "").slice(2)}, 
       
      // Time 
      a: function(){return jsdate.getHours() > 11 ? "pm" : "am"}, 
      A: function(){return f.a().toUpperCase()}, 
      B: function(){ 
       // peter paul koch: 
       var off = (jsdate.getTimezoneOffset() + 60)*60; 
       var theSeconds = (jsdate.getHours() * 3600) + (jsdate.getMinutes() * 60) + jsdate.getSeconds() + off; 
       var beat = Math.floor(theSeconds/86.4); 
       if (beat > 1000) beat -= 1000; 
       if (beat < 0) beat += 1000; 
       if ((String(beat)).length == 1) beat = "00"+beat; 
       if ((String(beat)).length == 2) beat = "0"+beat; 
       return beat; 
      }, 
      g: function(){return jsdate.getHours() % 12 || 12}, 
      G: function(){return jsdate.getHours()}, 
      h: function(){return pad(f.g(), 2)}, 
      H: function(){return pad(jsdate.getHours(), 2)}, 
      i: function(){return pad(jsdate.getMinutes(), 2)}, 
      s: function(){return pad(jsdate.getSeconds(), 2)}, 
      //u not supported yet 
       
      // Timezone 
      //e not supported yet 
      //I not supported yet 
      O: function(){ 
       var t = pad(Math.abs(jsdate.getTimezoneOffset()/60*100), 4); 
       if (jsdate.getTimezoneOffset() > 0) t = "-" + t; else t = "+" + t; 
       return t; 
      }, 
      P: function(){var O = f.O();return (O.substr(0, 3) + ":" + O.substr(3, 2))}, 
      //T not supported yet 
      //Z not supported yet 
       
      // Full Date/Time 
      c: function(){return f.Y() + "-" + f.m() + "-" + f.d() + "T" + f.h() + ":" + f.i() + ":" + f.s() + f.P()}, 
      //r not supported yet 
      U: function(){return Math.round(jsdate.getTime()/1000)} 
     }; 
       
     return format.replace(/[\\]?([a-zA-Z])/g, function(t, s){ 
          if( t!=s ){ 
           // escaped 
           ret = s; 
          } else if( f[s] ){ 
           // a date function exists 
           ret = f[s](); 
          } else{ 
           // nothing special 
           ret = s; 
          } 
          return ret; 
     }); 
}

//合并内容相同单元格
function mergeTableCell(table_id, colnum) {
    var firsttd = "";
    var currenttd = "";
    var spanNum = 0;
    var obj = $( table_id + " tr td:nth-child(" + colnum + ")");
    obj.each(function (i) {
        if(i==0){
            firsttd = $(this);
            spanNum = 1;
        }else{
            currenttd = $(this);
            if (firsttd.text() == currenttd.text()) {
                spanNum++;
                currenttd.hide();
                firsttd.attr("rowSpan", spanNum);
                firsttd.css('vertical-align','middle');
            } else {
                spanNum = 1;
                firsttd = $(this);
            }
        }
    });
}
//隐藏指定列重复的行
function hideTableRepeatRow(table_id, colnum) {
    var firsttd = "";
    var currenttd = "";
    var arr=[];
    var obj = $( table_id + " tr td:nth-child(" + colnum + ")");
    obj.each(function (i) {
        arr.push(i);
        firsttd = $(this);
        obj.each(function (j) {
            if(j in arr){
            }else{
                currenttd = $(this);
                if (firsttd.text() == currenttd.text()) {
                    currenttd.parent('tr').hide();
                }
            }
        });
    });
}
function centerInfoDialog()
{
    var $infoDlg = $("#info_dialog");
    var $parentDiv = $infoDlg.parent();
    var dlgWidth = $infoDlg.width();
    var parentWidth = $parentDiv.width();

    $infoDlg[0].style.left = Math.round((parentWidth - dlgWidth) / 2) + "px";
}