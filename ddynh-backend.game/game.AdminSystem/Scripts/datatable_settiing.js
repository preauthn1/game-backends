const datatable_opt = {
    TodayRanking: function () {
        const head = [
       {
           "sTitle": "玩家ID", "mData": "UserID",
           "render": function (data, type, row) {
               return `<a class="link" target="_blank" href="/UserInfo/Search?userId=${data}&beginTime=@(DateTime.Now.AddMinutes(-10).ToString("yyyy-MM-dd HH:mm:ss"))&endTime=@(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))">${data}</a>`
           }
       },
       {
           "sTitle": "今日输赢", "mData": "TodayUserWinLose",
           "render": function (data, type, row) {
               var value = data * -1;
               if (value < 0) {
                   return `<span class="update_Red">${ToCash(value)}</span>`
               }
               return `<span class="update_Green">${ToCash(value)}</span>`
           }
       },
       { "sTitle": "总胜率(%)", "mData": "TotalWinRate", },
       { "sTitle": "今日局数", "mData": "TodayAll" },
       { "sTitle": "今日胜局", "mData": "TodayWin" },
       { "sTitle": "今日胜率(%)", "mData": "TodayWinRate" },
       { "sTitle": "是否在线", "mData": "StrOnline" },
       { "sTitle": "上线时间", "mData": "StrLoginTime" },
       { "sTitle": "下线时间", "mData": "StrLogoutTime" },
       {
           "sTitle": "增删监控", "mData": "VIPUserID",
           "render": function (data, type, row) {
               if (data == 0) {
                   return `<a class="btn_link" href="#" onclick="vApp.addVIPUser(${row.UserID})" >增加监控</a>`
               }
               return `<a class="btn_link_Red" href="#" onclick="vApp.delVIPUser(${data})" >删除监控</a>`
           }
       }]

        const opt = {
            "ordering": false,
            "searching": false,
            "aoColumns": head,
            "processing": true,
            "serverSide": true,
            "bLengthChange": false,
            "bPaginate": false,
            "ajax": {
                "type": "POST",
                "url": "/GameCommon/GetTodayRanking",
                data: function (d) {
                    Object.assign(d, {
                        'gameID': vApp.gameID,
                        'online': vApp.selectedStatus
                    });
                    return d;
                }
            },
            "destroy": true
        }

        return opt
    },

    TodayUser: function () {
        const head = [
       {
           "sTitle": "玩家ID", "mData": "UserID",
           "render": function (data, type, row) {
               return `<a class="link" target="_blank" href="/UserInfo/Search?userId=${data}&beginTime=@(DateTime.Now.AddMinutes(-10).ToString("yyyy-MM-dd HH:mm:ss"))&endTime=@(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))">${data}</a>`
           }
       },
       {
           "sTitle": "总充值", "mData": "TotalSumCharge",
           "render": function (data, type, row) {
               return ToCash(data);
           }
       },
       {
           "sTitle": "总兑换", "mData": "TotalSumConvert",
           "render": function (data, type, row) {
               return ToCash(data);
           }
       },
       {
           "sTitle": "用户金币", "mData": "UserCash",
           "render": function (data, type, row) {
               return ToCash(data);
           }
       },
       {
           "sTitle": "总输赢", "mData": "TotalUserWinLose",
           "render": function (data, type, row) {
               var value = data * -1;
               if (value < 0) {
                   return `<span class="update_Red">${ToCash(value)}</span>`
               }
               return `<span class="update_Green">${ToCash(value)}</span>`
           }
       },
       {
           "sTitle": "今日充值", "mData": "TodaySumCharge",
           "render": function (data, type, row) {
               return ToCash(data);
           }
       },
       {
           "sTitle": "今日兑换", "mData": "TodaySumConvert",
           "render": function (data, type, row) {
               return ToCash(data);
           }
       },
       {
           "sTitle": "今日输赢", "mData": "TodayUserWinLose",
           "render": function (data, type, row) {
               var value = data * -1;
               if (value < 0) {
                   return `<span class="update_Red">${ToCash(value)}</span>`
               }
               return `<span class="update_Green">${ToCash(value)}</span>`
           }
       },
       { "sTitle": "局数", "mData": "TotalAll" },
       { "sTitle": "总胜局", "mData": "TotalWinCount" },
       { "sTitle": "总负局", "mData": "TotalLoseCount" },
       { "sTitle": "总胜率(%)", "mData": "TotalWinRate" },
       { "sTitle": "今日局数", "mData": "TodayAll" },
       { "sTitle": "今日胜局", "mData": "TodayWin" },
       { "sTitle": "今日胜率(%)", "mData": "TodayWinRate" },
       { "sTitle": "是否在线", "mData": "StrOnline" },
       { "sTitle": "上线时间", "mData": "StrLoginTime", "class": "width6" },
       { "sTitle": "下线时间", "mData": "StrLogoutTime", "class": "width6" },
       {
           "sTitle": "增删监控", "mData": "VIPUserID",
           "render": function (data, type, row) {
               if (data == 0) {
                   return `<div class="btn_link" onclick="vApp.addVIPUser(${row.UserID})" >增加监控</div>`
               }
               return `<div class="btn_link_Red" onclick="vApp.delVIPUser(${data})" >删除监控</div>`
           }
       }]

        const opt = {
            "ordering": false,
            "searching": false,
            "aoColumns": head,
            "processing": true,
            "serverSide": true,
            "bLengthChange": false,
            "bPaginate": false,
            "ajax": {
                "type": "POST",
                "url": "/GameCommon/GetTodayUserList",
                "data": {
                    'gameID': vApp.gameID,
                }
            },
            "destroy": true
        }
        return opt
    },

    VIPUser: function () {
        const head = [
            {
                "sTitle": "玩家ID", "mData": "UserID",
                "render": function (data, type, row) {
                    return `<a class="link" target="_blank" href="/UserInfo/Search?userId=${data}&beginTime=@(DateTime.Now.AddMinutes(-10).ToString("yyyy-MM-dd HH:mm:ss"))&endTime=@(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))">${data}</a>`
                }
            },
            {
                "sTitle": "总输赢", "mData": "TotalUserWinLose",
                "render": function (data, type, row) {
                    var value = data * -1;
                    if (value < 0) {
                        return `<span class="update_Red">${ToCash(value)}</span>`
                    }
                    return `<span class="update_Green">${ToCash(value)}</span>`
                }
            },
            {
                "sTitle": "今日输赢", "mData": "TodayUserWinLose",
                "render": function (data, type, row) {
                    var value = data * -1;
                    if (value < 0) {
                        return `<span class="update_Red">${ToCash(value)}</span>`
                    }
                    return `<span class="update_Green">${ToCash(value)}</span>`
                }
            },
            { "sTitle": "总局数", "mData": "TotalAll" },
            {
                "sTitle": "指定放分金额", "mData": "AssignAmount",
                "render": function (data, type, row) {
                    return `<input type="text" id="AssignAmount_${row.UserID}" name="AssignAmount_${row.UserID}" class ="form-control input-medium" placeholder="" value="${ToCash(data)}" />`
                }
            },
            {
                "sTitle": "放分胜率(%)", "mData": "AssignRate",
                "render": function (data, type, row) {
                    const val = data > 6 ? 6 : data
                    return `
                    <select id="AssignRate_${row.UserID}" name="AssignRate_${row.UserID}" class ="form-control input-small" value="${val}">
                        <option value="1" ${val === 1 ? 'selected' : ''}>20%</option>
                        <option value="2" ${val === 2 ? 'selected' : ''}>30%</option>
                        <option value="3" ${val === 3 ? 'selected' : ''}>40%</option>
                        <option value="4" ${val === 4 ? 'selected' : ''}>70%</option>
                        <option value="5" ${val === 5 ? 'selected' : ''}>80%</option>
                        <option value="6" ${val === 6 ? 'selected' : ''}>90%</option>
                    </select>`
                }
            },
            {
                "sTitle": "完成金额", "mData": "CompleteAmount",
                "render": function (data, type, row) {
                    return ToCash(data);
                }
            },
            { "sTitle": "是否完成", "mData": "StrComplete" },
            {
                "sTitle": "开始/暂停", "mData": "Status",
                "render": function (data, type, rowData, meta) {
                    if (data == 0) {
                        return `<div class="btn_link" onclick="vApp.editStatus(${meta.row},1)" >开始</div>`
                    }
                    return `<div class="btn_link" onclick="vApp.editStatus(${meta.row},0)" >暂停</div>`
                }
            },
            {
                "sTitle": "删除监控", "mData": "UserID",
                "render": function (data, type, row) {
                    return `<div class="btn_link_Red" onclick="vApp.delVIPUser(${data})" >删除</div>`
                }
            },
            { "sTitle": "放分时间", "mData": "StrUpdateTime" },
            { "sTitle": "操作人", "mData": "Operator" },
            {
                "sTitle": "操作", "mData": "UserID",
                "render": function (data, type, row, meta) {
                    vApp.datas[meta.row] = row;
                    return `<button class="btn gray" href="#" onclick="vApp.saveVIPUser(${meta.row})">保存</a>`
                }
            }
        ]

        const opt = {
            "ordering": false,
            "searching": false,
            "aoColumns": head,
            "processing": true,
            "serverSide": true,
            "bLengthChange": false,
            "bPaginate": false,
            "ajax": {
                "type": "POST",
                "url": "/GameCommon/GetVIPUser",
                data: function (d) {
                    Object.assign(d, {
                        'gameID': vApp.gameID,
                    });
                    return d;
                }
            },
            "destroy": true
        }
        return opt
    },

    PoolLog: function () {
        const head = [
            { "sTitle": "修改记录", "mData": "Result" },
            { "sTitle": "修改人", "mData": "Operator" },
            { "sTitle": "修改时间", "mData": "StrCreateTime" }]

        const opt = {
            "ordering": false,
            "searching": false,
            "aoColumns": head,
            "processing": true,
            "serverSide": true,
            "bLengthChange": false,
            "bPaginate": false,
            "ajax": {
                "type": "POST",
                "url": "/GameCommon/GetChangeLog",
                "data": {
                    'type': 1,
                    'gameID': vApp.gameID,
                }
            },
            "destroy": true
        }
        return opt
    },

    ChangeLog: function () {
        const head = [
            { "sTitle": "修改记录", "mData": "Result" },
            { "sTitle": "修改人", "mData": "Operator" },
            { "sTitle": "修改时间", "mData": "StrCreateTime" }]

        const opt = {
            "ordering": false,
            "searching": false,
            "aoColumns": head,
            "processing": true,
            "serverSide": true,
            "bLengthChange": false,
            "bPaginate": false,
            "ajax": {
                "type": "POST",
                "url": "/GameCommon/GetChangeLog",
                "data": {
                    'type': 2,
                    'gameID': vApp.gameID,
                }
            },
            "destroy": true
        }
        return opt
    },

    FreeMoneyLog: function () {
        const head = [
            { "sTitle": "修改记录", "mData": "Result" },
            { "sTitle": "修改人", "mData": "Operator" },
            { "sTitle": "修改时间", "mData": "StrCreateTime" }]

        const opt = {
            "ordering": false,
            "searching": false,
            "aoColumns": head,
            "processing": true,
            "serverSide": true,
            "bLengthChange": false,
            "bPaginate": false,
            "ajax": {
                "type": "POST",
                "url": "/GameCommon/GetChangeLog",
                "data": {
                    'type': 2,
                    'gameID': vApp.gameID,
                }
            },
            "destroy": true
        }
        return opt
    },

    WinPrizeLogTable: function () {
        const head = [
                               { "sTitle": "牌局ID", "mData": "LogID" },
                               { "sTitle": "修改人", "mData": "AdminUser" },
                               {
                                   "sTitle": "开奖类型", "mData": "PrizeType",
                                   "render": function (data, type, row) {
                                       if (data.indexOf(",") != -1) {
                                           var _array = data.split(',');
                                           for (var i = 0; i < _array.length; i++) {
                                               _array[i] = vApp.prizeType[_array[i]];
                                           }
                                           return _array.join(',');
                                       }

                                       return vApp.prizeType[data];
                                   }
                               },
                               { "sTitle": "创建时间", "mData": "StrCreateTime" },
                               { "sTitle": "修改时间", "mData": "StrUpdateTime" }]

        const beginTime = $("#beginTimeLog").val();
        const endTime = $("#endTimeLog").val();

        const opt = {
            "ordering": false,
            "searching": false,
            "aoColumns": head,
            "processing": true,
            "serverSide": true,
            "lengthMenu": [20, 50, 100, 150],
            "pageLength": 50,
            "ajax": {
                "type": "POST",
                "url": "/GameCommon/GetWinPrizeLog",
                "data": {
                    '_r': Math.random(),
                    'ServerID': vApp.serverID,
                    'GameID': vApp.gameID,
                    'TableNo': vApp.tableNO,
                    'BeginTime': beginTime,
                    'EndTime': endTime
                }
            },
            "destroy": true
        }
        return opt
    }
}