<?php

use Illuminate\Support\Facades\Route;

/* 內部測試工具 */
Route::post('tools/aes/decrypt', 'Tools\AesController@Decrypt');
Route::post('tools/aes/encrypt', 'Tools\AesController@Encrypt');

Route::post('token/{token}/{language}', 'Auth\TokenController@token');

/* 登入前可使用的API(沒驗證TOKEN) */
Route::middleware(['non_auth'])->group(function () {
    /* 代理登入 */
    Route::post('login', 'Auth\LoginController@login');
    Route::post('auth', 'Auth\MainController@Authenticate');
});

/* 登入後才可使用的API(驗證TOKEN) */
Route::middleware(['auth'])->group(function () {
    Route::post('menu', 'Auth\MenuController@getList');
    Route::post('logout', 'Auth\LoginController@logout');

    //儀表板
    Route::prefix('dashboard')->group(function () {
        //數據統計
        Route::post('list', 'Dashboard\MainController@getInfo');
    });

    //管理員相關
    Route::prefix('admin')->group(function () {
        //ROLE
        Route::post('role', 'Admin\RoleController@getList');
        //Permission
        Route::post('permission', 'Admin\PermissionController@getList');
        //Menu
        Route::post('menu', 'Admin\MenuController@getList');
    });

    //代理相關
    Route::prefix('agent')->group(function () {
        //代理新增
        Route::post('add', 'Agent\AgentController@create');
        //google 2階段啟用
        Route::post('enable2fa', 'Agent\TwoFactorController@enable2fa');
        //google 2階段認證
        Route::post('verify2fa', 'Agent\TwoFactorController@verify2fa');
    });

    //系統管理
    Route::prefix('system')->group(function () {
        //系統管理-活動圖文設置
        Route::post('create', 'System\TwoFactorController@verify2fa');

        //系統管理-下載頁
        Route::post('download', 'System\DwonloadController@getList');
    });

    //邀請活動
    Route::prefix('invite')->group(function () {
        //清單
        Route::post('list', 'System\InviteController@getList');
        //明細
        Route::post('view', 'System\InviteController@view');
        //新增
        Route::post('add', 'System\InviteController@create');
        //修改
        Route::post('update', 'System\InviteController@update');
    });

    //系統管理 - 平台公告
    Route::prefix('announcement')->group(function () {
        //平台公告 - 清單
        Route::post('list', 'Announcement\MainController@getList');
        //平台公告 - 明細
        Route::post('view', 'Announcement\MainController@view');
        //平台公告 - 新增
        Route::post('add', 'Announcement\MainController@create');
        //平台公告 - 修改
        Route::post('update', 'Announcement\MainController@update');
    });

    //系統管理-排行榜
    Route::prefix('ranking')->group(function () {
        //系統管理-排行榜-清單
        Route::post('list', 'Miner\RankingController@getList');
        //系統管理-排行榜-新增
        Route::post('add', 'Miner\RankingController@create');
        //系統管理-排行榜-修改
        Route::post('update', 'Miner\RankingController@update');
        //系統管理-排行榜-刪除
        Route::post('delete', 'Miner\RankingController@delete');
    });

    //下拉選單
    Route::prefix('dropdown')->group(function () {
        //玩家類型
        Route::post('memberType', 'Member\MemberController@getDropdown');
        //玩家身份證狀態類型
        Route::post('memberIdCardStatus', 'Member\IdCardController@getDropdown');
        //礦機機型
        Route::post('miner', 'Miner\MinerController@getDropdown');
        //貨幣清單
        Route::post('currency', 'Currency\MainController@getDropdown');
        //國家清單
        Route::post('country', 'Country\MainController@getDropdown');
        //開分銀行清單
        Route::post('depositBank', 'Deposit\BankController@getDropdown');
        //充值訂單狀態
        Route::post('depositStatus', 'Deposit\MainController@getStatusDropdown');
        //洗分銀行清單
        Route::post('withdrawalBank', 'Withdrawal\BankController@getDropdown');
        //洗分訂單狀態
        Route::post('withdrawalStatus', 'Withdrawal\MainController@getStatusDropdown');
        //公告類別清單
        Route::post('announcementType', 'announcement\MainController@getDropdown');
        //團購訂單狀態
        Route::post('groupOrderStatus', 'Miner\GroupController@getStatusDropdown');
        //充值渠道
        Route::post('depositGateway', 'Deposit\MainController@getGatewayDropdown');
        //提現渠道
        Route::post('withdrawalGateway', 'Withdrawal\MainController@getGatewayDropdown');
        //盈利分析排序欄位下拉選單
        Route::post('analyzeSortColumn', 'Member\ProfitAnalysisController@getOrderBy');
        //盈利分析排序下拉選單
        Route::post('analyzeSortBy', 'Member\ProfitAnalysisController@getSort');
    });

    //會員管理
    Route::prefix('member')->group(function () {
        //會員管理-index
        Route::post('list', 'Member\MemberController@getList');
        //會員管理-詳情
        Route::post('view', 'Member\MemberController@view');
        //會員管理-新增用戶
        Route::post('add', 'Member\MemberController@create');
        //會員管理-個人資料修改
        Route::post('update', 'Member\MemberController@update');
        //會員管理-人工操作

        //會員管理-登入IP
        Route::post('login', 'Member\LogController@getList');

        //會員管理-會員錢包
        Route::post('wallet', 'Member\WalletController@get');

        //會員管理-盈利分析
        Route::post('analysis', 'Member\ProfitAnalysisController@getList');

        //會員管理-人工操作
        Route::post('walletAdjust', 'Member\WalletController@adjust');
        //代理中心
        Route::post('center', 'Member\AgentCenterController@view');
    });

    //會員ID
    Route::prefix('memberIdCard')->group(function () {
        //會員ID-list
        Route::post('list', 'Member\IdCardController@getList');
        //會員ID-詳情
        Route::post('view', 'Member\IdCardController@view');
        //會員ID-個人資料修改
        Route::post('update', 'Member\IdCardController@update');
    });

    //充值管理
    Route::prefix('deposit')->group(function () {
        //充值管理-清單
        Route::post('list', 'Deposit\BankController@getList');
    });

    //會員充值管理
    Route::prefix('depositOrder')->group(function () {
        //會員充值管理-清單
        Route::post('list', 'Deposit\OrderController@getList');
        //會員充值管理-充值明細
        Route::post('view', 'Deposit\OrderController@view');
        //會員充值管理-充值明細狀態編輯
        Route::post('update', 'Deposit\OrderController@update');
    });

    //提現管理
    Route::prefix('withdrawal')->group(function () {
        //提現管理-提現明細
        Route::post('list', 'Withdrawal\BankController@getList');
        //提現管理-明細
        Route::post('view', 'Withdrawal\BankController@view');
        //提現管理-匯率、提現手續費%編輯
        Route::post('update', 'Withdrawal\BankController@update');
        //提現管理-匯率、提現手續費%編輯
        Route::post('updateRate', 'Withdrawal\BankController@updateRate');
        //提現管理-匯率、提現手續費%編輯
        Route::post('updateCurrencyTax', 'Withdrawal\BankController@updateCurrencyTax');
        //提現管理-匯率、提現手續費%編輯
        Route::post('updateTax', 'Withdrawal\BankController@updateTax');
    });

    //會員提現管理
    Route::prefix('withdrawalOrder')->group(function () {
        //會員提現管理-提現清單
        Route::post('list', 'Withdrawal\OrderController@getList');
        //會員提現管理-提現明細
        Route::post('view', 'Withdrawal\OrderController@view');
        //會員提現管理-提現明細狀態編輯
        Route::post('update', 'Withdrawal\OrderController@update');
    });

    //礦機管理
    Route::prefix('miner')->group(function () {
        Route::prefix('purchase')->group(function () {
            //礦機管理-清單
            Route::post('list', 'Miner\PurchaseController@getList');
            //礦機管理-明細
            Route::post('view', 'Miner\PurchaseController@view');
            //礦機管理-新增
            Route::post('add', 'Miner\PurchaseController@create');
            //礦機管理-修改
            Route::post('update', 'Miner\PurchaseController@update');
        });

        //租賃管理
        Route::prefix('rental')->group(function () {
            //租賃管理-清單
            Route::post('list', 'Miner\RentalController@getList');
            //租賃管理-明細
            Route::post('view', 'Miner\RentalController@view');
            //租賃管理-新增
            Route::post('add', 'Miner\RentalController@create');
            //租賃管理-修改
            Route::post('update', 'Miner\RentalController@update');
        });

        Route::prefix('group')->group(function () {
            //團購管理-待拼單清單
            Route::post('waitingList', 'Miner\GroupController@getWaitingList');
            //團購管理-清單
            Route::post('list', 'Miner\GroupController@getList');
            //團購管理-明細
            Route::post('view', 'Miner\GroupController@view');
            //團購管理-新增
            Route::post('add', 'Miner\GroupController@create');
            //團購管理-修改
            Route::post('update', 'Miner\GroupController@update');
        });

        //代理分銷設定
        Route::prefix('rewardSetting')->group(function () {
            //代理分銷設定 - 清單
            Route::post('list', 'Miner\RewardController@getList');
            //代理分銷設定 - 明細
            Route::post('view', 'Miner\RewardController@view');
            //代理分銷設定 - 新增
            Route::post('add', 'Miner\RewardController@create');
            //代理分銷設定 - 修改
            Route::post('update', 'Miner\RewardController@update');
        });

        //邀請活動設定
        Route::prefix('rebateSetting')->group(function () {
            //邀請活動設定 - 清單
            Route::post('list', 'Miner\RebateController@getList');
            //邀請活動設定 - 明細
            Route::post('view', 'Miner\RebateController@view');
            //邀請活動設定 - 新增
            Route::post('add', 'Miner\RebateController@create');
            //邀請活動設定 - 修改
            Route::post('update', 'Miner\RebateController@update');
        });
    });

    //銷售相關
    Route::prefix('purchase')->group(function () {
        //銷售訂單
        Route::post('list', 'Miner\PurchaseController@getList');
    });

    //租賃相關
    Route::prefix('rental')->group(function () {
        //租賃信息
        Route::post('list', 'Miner\RentalController@getList');
    });

    //報表相關
    Route::prefix('record')->group(function () {
        //人工操作清單
        Route::post('manualList', 'Record\TransactionLogController@getManualList');
        //折舊信息
        Route::post('depreciation', 'MemberMiner\DepreciationController@getList');
        //返佣信息
        Route::post('rebate', 'MemberMiner\RebateController@getList');
        //邀請獎勵信息
        Route::post('reward', 'MemberMiner\RewardController@getList');
    });
});

Route::any('{any}', function () {
    return response()->json([
        'code' => 9999,
        'message' => 'fail'
    ], 404);
});
