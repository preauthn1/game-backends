<?php
//客户端接口
$app->group('', function () {
    $this->get('/serverlist', 'App\Controllers\AppController:serverList');//获取服务器列表
    $this->get('/pay/{player:[0-9]+}', 'App\Controllers\AppController:payConfig');//获取配置
    $this->get('/pay/{player:[0-9]+}/{channel:[0-9]{9}}', 'App\Controllers\AppController:payConfig');//获取配置
    $this->get('/exchange/{player:[0-9]+}/{type:[0-1]{1}}', 'App\Controllers\AppController:getUserExchange');//获取用户兑换列表
})->add(function ($request, $response, $next) {
    if ($_SERVER['HTTP_HOST'] != "example.invalid") {
        if ($_SERVER['HTTP_HOST'] != "example.invalid") {
            return $response = $response->withStatus(404);
        }
        return $response = $next($request, $response);
    } else {
        return $response = $next($request, $response);
    }
});

//支付下单
$app->group('', function () {
    $this->get('/order/{channel:[0-9]+}/{userid:[0-9]+}/{price:[0-9]+}', 'App\Controllers\PayController:order');//下单
    $this->any('/front', 'App\Controllers\PayController:front');//前台回显
})->add(function ($request, $response, $next) {
    if ($_SERVER['HTTP_HOST'] != "example.invalid") {
        if ($_SERVER['HTTP_HOST'] != "example.invalid") {
            return $response = $response->withStatus(404);
        }
        return $response = $next($request, $response);
    } else {
        return $response = $next($request, $response);
    }
});
//支付后端回调
$app->group('', function () {
    $this->any('/back/{channel:[0-9]+}', 'App\Controllers\PayController:back');//回调
})->add(function ($request, $response, $next) {
    if ($_SERVER['HTTP_HOST'] != "example.invalid") {
        if ($_SERVER['HTTP_HOST'] != "example.invalid") {
            return $response = $response->withStatus(404);
        }
        return $response = $next($request, $response);
    } else {
        return $response = $next($request, $response);
    }
});

//支付宝自动打款工具
$app->group('', function () {
    $this->get('/exchangelist', 'App\Controllers\ServerController:exchangeList');
    $this->get('/exchangeEditStatus', 'App\Controllers\ServerController:exchangeEditStatus');
    $this->get('/exchangelistcheckidstatus', 'App\Controllers\ServerController:exchangelistcheckidstatus');
})->add(function ($request, $response, $next) {
    if ($_SERVER['HTTP_HOST'] != "example.invalid") {
        if ($_SERVER['HTTP_HOST'] != "example.invalid") {
            return $response = $response->withStatus(404);
        }
        return $response = $next($request, $response);
    } else {
        return $response = $next($request, $response);
    }
});

//客服接口
$app->group('', function () {
    $this->get('/customerlist', 'App\Controllers\ServerController:customerList');
    $this->post('/addcustomer', 'App\Controllers\ServerController:addCustomer');
    $this->post('/delcustomer', 'App\Controllers\ServerController:delCustomer');
    $this->any('/ajax/upload/{type:[\w-]+}', 'App\Controllers\AjaxController:upload');
})->add(function ($request, $response, $next) {
    if ($_SERVER['HTTP_HOST'] == 'example.invalid' || $_SERVER['HTTP_HOST'] == 'example.invalid' || $_SERVER['REMOTE_ADDR'] == '0.0.0.0' || $_SERVER['REMOTE_ADDR'] == '0.0.0.0' || $_SERVER['REMOTE_ADDR'] == '0.0.0.0') {
        return $response = $next($request, $response);
    } else {
        return $response = $response->withStatus(404);
    }
});

//首页
$app->get('/', 'App\Controllers\IndexController:index')->setName('userIndex');
$app->post('/', 'App\Controllers\IndexController:index');

//计划任务
$app->get('/crontabtest', 'App\Controllers\CrontabController:test');

//游戏日常数据接口
$app->group('/api', function () {
    $this->any('/getReportData/{type:\w+}', 'App\Controllers\ApiReportDataController:core');
    $this->any('/exportdata/{type:\w+}', 'App\Controllers\ExportDataController:core');
})->add(function ($request, $response, $next) {
    //if ($_SERVER['HTTP_HOST'] != "example.invalid" && $_SERVER['HTTP_HOST'] != "example.invalid") {
        //if ($_SERVER['HTTP_HOST'] != "example.invalid") {
        //    return $response = $response->withStatus(404);
        //}
        return $response = $next($request, $response);
    //} else {
    //    return $response = $next($request, $response);
    //}
});

//代理商入口
$app->group('/agentadmin', function () {
    $this->any('/page/{page:.+}', 'App\Controllers\Agentadmin\PageController:agentAdminIndex');
    $this->any('/agent/login', 'App\Controllers\Agentadmin\LoginController:agentAdminLogin')->setName('agentAdminLogin');
    $this->any('/agenttotal/login', 'App\Controllers\Agentadmin\LoginController:agentTotalAdminLogin')->setName('agentTotalAdminLogin');
    $this->any('/agent/logout', 'App\Controllers\Agentadmin\LoginController:agentAdminLogout');
    $this->any('/agenttotal/logout', 'App\Controllers\Agentadmin\LoginController:agentTotalAdminLogout');
    $this->any('/menu/sub', 'App\Controllers\Agentadmin\MenuController:getAgentAdminSubMenu');
    $this->any('/api/{path:.+}', 'App\Controllers\Agentadmin\CoreController:callAgentControllers');
    $this->any('/sms/api/{type:\w+}', 'App\Controllers\Agentadmin\SmsController:core');
})->add(function ($request, $response, $next) {
    return $response = $next($request, $response);
});

//管理后台页面
$app->group('/admin', function () {
    $this->get('', 'App\Controllers\AdminController:welcome')->setName('adminHome');
    $this->get('/login', 'App\Controllers\AdminController:login')->setName('adminLogin');
    $this->post('/login', 'App\Controllers\AdminController:login');
    $this->get('/logout', 'App\Controllers\AdminController:adminlogout')->setName('adminLogout');
    $this->get('/menu', 'App\Controllers\AdminController:menu')->setName('adminMenu');
    $this->get('/menu/add', 'App\Controllers\AdminController:menuAdd');
    $this->get('/menu/{id:[0-9]+}', 'App\Controllers\AdminController:menuEdit');
    $this->get('/menu/del/{id:[0-9]+}', 'App\Controllers\AdminController:menuDel');
    $this->post('/menu/add', 'App\Controllers\AdminController:menuAdd');
    $this->post('/menu/{id:[0-9]+}', 'App\Controllers\AdminController:menuEdit');
    $this->get('/settings', 'App\Controllers\AdminController:settings');
    $this->post('/settings', 'App\Controllers\AdminController:settings');
    $this->any('/exchangesetting', 'App\Controllers\AdminController:exchangeCfg');
    $this->any('/exchangebanksetting', 'App\Controllers\AdminController:exchangeBankCfg');
    //管理员
    $this->get('/users/groups', 'App\Controllers\AdminController:usersGroup');
    $this->post('/users/groups', 'App\Controllers\AdminController:usersGroup');
    $this->get('/users/groups/{id:[0-9]+}', 'App\Controllers\AdminController:usersGroupEdit');
    $this->post('/users/groups/{id:[0-9]+}', 'App\Controllers\AdminController:usersGroupEdit');
    $this->get('/users/groups/del/{id:[0-9]+}', 'App\Controllers\AdminController:usersGroupDel');
    $this->get('/group/priv', 'App\Controllers\AdminController:groupPriv');
    $this->post('/group/priv', 'App\Controllers\AdminController:groupPriv');
    $this->get('/users', 'App\Controllers\AdminController:users');
    $this->post('/users', 'App\Controllers\AdminController:users');
    $this->get('/master', 'App\Controllers\AdminController:master');
    $this->get('/master/set/{id:[0-9]+}', 'App\Controllers\AdminController:master');
    //支付管理
    $this->any('/shop/alipayviporder', 'App\Controllers\AdminOrderController:alipayVipOrder');
    $this->any('/shop/order', 'App\Controllers\AdminOrderController:shopOrder');
    $this->any('/shop/ordersuccess', 'App\Controllers\AdminOrderController:shopOrder');
    $this->any('/shop/platform', 'App\Controllers\AdminOrderController:shopPlatform');
    $this->any('/shop/channel', 'App\Controllers\AdminOrderController:shopChannel');
    $this->get('/shop/agent', 'App\Controllers\AdminOrderController:shopAgent');
    $this->any('/shop/alipaylink', 'App\Controllers\AdminOrderController:alipayLink');
    $this->any('/shop/paycentercfg', 'App\Controllers\AdminOrderController:paycentercfg');
    //提现管理
    $this->any('/payfor/orders', 'App\Controllers\PayForController:orders');
    $this->any('/payfor/configs', 'App\Controllers\PayForController:configs');
    $this->any('/payfor/banks', 'App\Controllers\PayForController:banks');
    $this->any('/payfor/exchange', 'App\Controllers\PayForController:ExchangeConfig');
    //玩家管理
    $this->any('/user', 'App\Controllers\AdminUserController:userlist')->setName('userlist');
    $this->any('/userbrief/{pf:\d{5,}}', 'App\Controllers\AdminUserController:userBriefList');
    $this->any('/user/info/{id:[0-9]+}', 'App\Controllers\AdminUserController:userinfo');
    $this->any('/user/recharge/{id:[0-9]+}', 'App\Controllers\AdminUserController:userRecharge');
    $this->any('/user/login/{id:[0-9]+}', 'App\Controllers\AdminUserController:userLogin');
    $this->any('/user/goldbean/{id:[0-9]+}', 'App\Controllers\AdminUserController:userGoldBean');
    $this->any('/user/cash/{id:[0-9]+}', 'App\Controllers\AdminUserController:userCash');
    $this->any('/user/gamb/{id:[0-9]+}', 'App\Controllers\AdminUserController:userGamb');
    $this->any('/user/bank/{id:[0-9]+}', 'App\Controllers\AdminUserController:userBank');
    $this->any('/user/relate/{id:[0-9]+}', 'App\Controllers\AdminUserController:userRelate');
    $this->any('/user/agentcharge/{id:[0-9]+}', 'App\Controllers\AdminUserController:userAgentRecharge');
    //客服中心  问题反馈、邮件
    $this->any('/question', 'App\Controllers\CustomerController:questionList');
    $this->any('/customer', 'App\Controllers\CustomerController:onlineCustomer');
    $this->any('/customer/chatlog', 'App\Controllers\CustomerController:onlineCustomerChatLog');
    $this->any('/customer/fastreply', 'App\Controllers\CustomerController:onlineCustomerFastReply');
    $this->any('/mail', 'App\Controllers\CustomerController:mailList');
    $this->any('/mail/send', 'App\Controllers\CustomerController:mailSend');
    $this->any('/smsvcode', 'App\Controllers\CustomerController:smsvcode');
    //兑换
    $this->any('/exchangealipay', 'App\Controllers\CustomerController:exchange');
    $this->any('/exchangebank', 'App\Controllers\CustomerController:exchange');
    $this->any('/exchange', 'App\Controllers\CustomerController:exchange');
    $this->any('/exchangexport', 'App\Controllers\CustomerController:exchange');
    $this->any('/exchangejs', 'App\Controllers\CustomerController:exchange');
    $this->any('/exchangeos', 'App\Controllers\CustomerController:exchange');
    $this->any('/kefu/data', 'App\Controllers\CustomerController:kefuData');
    $this->any('/exchangepay', 'App\Controllers\CustomerController:ExchangePay');
    $this->any('/exchangeall', 'App\Controllers\CustomerController:exchange');

    //黑名单
    $this->any('/ipblack', 'App\Controllers\AdminBlackController:ipBlack');
    $this->any('/ipblack/del/{id:[0-9\.]+}', 'App\Controllers\AdminBlackController:ipBlackDel');
    $this->any('/macblack', 'App\Controllers\AdminBlackController:macBlack');
    $this->any('/macblack/del/{id:[\w-]+}', 'App\Controllers\AdminBlackController:macBlackDel');
    $this->any('/playerblack', 'App\Controllers\AdminBlackController:playerBlack');
    $this->any('/playerblack/del/{id:\d+}', 'App\Controllers\AdminBlackController:playerBlackDel');
    $this->any('/blacklist', 'App\Controllers\AdminBlackController:blackListIp');
    $this->any('/blacklist/ip', 'App\Controllers\AdminBlackController:blackListIp');
    $this->any('/blacklist/mac', 'App\Controllers\AdminBlackController:blackListMac');
    $this->any('/blacklist/tel', 'App\Controllers\AdminBlackController:blackListTel');
    $this->any('/playerwaitingblack', 'App\Controllers\AdminBlackController:playerWaitingBlack');
    $this->any('/playerwaitingblack/del/{id:\d+}', 'App\Controllers\AdminBlackController:playerWaitingBlackDel');

    //代理管理
    $this->any('/agent', 'App\Controllers\AgentController:agentList')->setName('agentList');
    $this->any('/agent/{id:[0-9]{4}}', 'App\Controllers\AgentController:agentEdit');
    $this->any('/agent/add', 'App\Controllers\AgentController:agentAdd');
    $this->any('/agent/recharge', 'App\Controllers\AgentController:agentRecharge');
    $this->any('/player/recharge', 'App\Controllers\AgentController:playerRecharge');

    //牌局日志管理
    $this->any('/logs/gambling', 'App\Controllers\AdminLogsController:gambling');
    $this->any('/logs/addgold', 'App\Controllers\AdminLogsController:addGold');
    $this->any('/logs/node', 'App\Controllers\AdminLogsController:node');
    $this->any('/logs/gamecontrollog', 'App\Controllers\AdminLogsController:gameControlLog');
    $this->any('/logs/payOpt', 'App\Controllers\AdminLogsController:optLog');
    $this->any('/logs/loginlog', 'App\Controllers\AdminLogsController:loginlog');

    //系统报表
    $this->any('/report/day', 'App\Controllers\ReportController:reportDay');
    $this->get('/report/{day:\d{4}\-\d{1,2}\-\d{1,2}}', 'App\Controllers\ReportController:reportDayJson');
    $this->any('/report/day/channel', 'App\Controllers\ReportController:reportDayChannel');
    $this->get('/report/day/{day:\d{4}\-\d{1,2}\-\d{1,2}}/{pf:\d+}', 'App\Controllers\ReportController:reportDayChannelJson');
    $this->any('/report/day/channel/{pf:\d{5,}}', 'App\Controllers\ReportController:reportDayChannel');
    $this->any('/report/month', 'App\Controllers\ReportController:reportMonth');
    $this->any('/online', 'App\Controllers\ReportController:online');
    $this->any('/report/recharge', 'App\Controllers\AdminOrderController:recharge');
    $this->any('/report/roomwin', 'App\Controllers\ReportController:roomwin');
    $this->any('/chart/online', 'App\Controllers\ReportController:chartOnline');
    $this->any('/report/roomuserstat', 'App\Controllers\ReportController:roomUserStat');

    //跑马灯、公告、短信码、渠道包配置、游戏状态、VIP等级、奖池
    $this->any('/package', 'App\Controllers\CommonuseController:package');
    $this->any('/channel', 'App\Controllers\CommonuseController:channel');
    $this->any('/game', 'App\Controllers\CommonuseController:game');
    $this->any('/platformgame', 'App\Controllers\CommonuseController:GamePlatform');
    $this->any('/jackpot', 'App\Controllers\CommonuseController:jackpot');
    $this->any('/jackpot/gamecontrol', 'App\Controllers\CommonuseController:gameControl');
    $this->any('/jackpot/gamecontrol/{gameid:[0-9]+}', 'App\Controllers\CommonuseController:gameControl');
    $this->any('/jackpot/gamecontrol/{serverid:[0-9]+}/{tableno:[0-9]+}', 'App\Controllers\CommonuseController:Tableinfo');
    $this->any('/jackpot/singlecontrol', 'App\Controllers\CommonuseController:singleControl');
    $this->any('/gamemessage', 'App\Controllers\CommonuseController:gameMessage');

    $this->any('/exportdata/{type:\w+}', 'App\Controllers\ExportDataController:core');
})->add(function ($request, $response, $next) {
    return $response = $next($request, $response);
});