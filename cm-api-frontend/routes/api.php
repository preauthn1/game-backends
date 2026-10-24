<?php

use Illuminate\Support\Facades\Route;

Route::get('/hello', function () {
    return 'Hello！Now is ' . date('Y-m-d H:i:s');
});

/* 內部測試工具 */
Route::post('tools/aes/decrypt', 'Tools\AesController@Decrypt');
Route::post('tools/aes/encrypt', 'Tools\AesController@Encrypt');

/* 登入前可使用的API(沒驗證TOKEN) */
Route::middleware(['non_auth'])->group(function () {
    //驗證碼相關
    Route::prefix('verification')->group(function () {
        //獲取圖形驗證碼
        Route::post('getCaptcha', 'Verification\CaptchaController@getCaptcha');
        //驗證圖形驗證碼
        //Route::post('checkCaptcha', 'Verification\CaptchaController@checkCaptcha');
        //獲取手機驗證碼
        Route::post('getSmsCode', 'Verification\SmsCodeController@getSmsCode');
        /* 傳送驗證碼 */
        //Route::post('sendValidateCode', 'Verification\TokenController@sendValidateCode');
    });

    Route::prefix('country')->group(function () {
        Route::post('list', 'Country\CountryController@getList');
    });

    //Template
    Route::post('template', 'Template\TemplateController@getTemplate');

    //App
    Route::prefix('app')->group(function () {
        //取得App資訊
        Route::post('getInfoByDomain', 'App\AppController@getInfoByDomain');
    });

    //會員註冊
    Route::post('register', 'Member\MemberController@register');
    //會員登入
    Route::post('login', 'Member\MemberController@login');
});

/* 登入後才可使用的API(驗證TOKEN) */
Route::middleware(['auth'])->group(function () {
    //Home
    Route::prefix('home')->group(function () {
        Route::post('information', 'Miner\InformationController@getHome');
    });

    //玩家相關
    Route::prefix('member')->group(function () {
        //獲取用戶資訊
        Route::post('getProfile', 'Member\MemberController@getProfile');
        //獲取頭像清單及個人頭像
        Route::post('wallet', 'Member\WalletController@getList');
        //獲取頭像清單及個人頭像
        Route::post('avatar', 'Member\AvatarController@getList');
        //設定個人頭像
        Route::post('setAvatar', 'Member\AvatarController@set');
        //創建會員銀行卡
        Route::post('createBank', 'Member\WithdrawalController@createBank');
        //創建會員加密貨幣
        Route::post('createCryptos', 'Member\WithdrawalController@createCryptos');
        //修改會員密碼
        Route::post('changePassword', 'Member\PasswordController@change');
        //創建資金密碼
        Route::post('createFundPassword', 'Member\PasswordController@createFund');
        //修改資金密碼
        Route::post('changeFundPassword', 'Member\PasswordController@changeFund');
        //忘記資金密碼
        Route::post('resetFundPassword', 'Member\PasswordController@resetFund');
        //創建會員郵箱
        Route::post('createEmail', 'Member\EmailController@createEmail');
        //創建會員真實姓名
        Route::post('createName', 'Member\MemberController@createName');
        //手機變更
        Route::post('changeMobile', 'Member\MemberController@changeMobile');
        //上傳ID information
        Route::post('idInformation', 'Member\FileController@uploadIdInformation');
        //邀請
        Route::post('invite', 'Member\InviteController@getInvite');
        //我的礦機
        Route::post('getMyMiners', 'Member\MinerController@getMyMiners');
        //我的礦機詳情
        Route::post('getMyMinerDetail', 'Member\MinerController@getMyMinerDetail');
        //google 2階段啟用
        Route::post('enable2fa', 'Member\TwoFactorController@enable2fa');
        //google 2階段認證
        Route::post('verify2fa', 'Member\TwoFactorController@verify2fa');
    });

    //下拉選單
    Route::prefix('dropdown')->group(function () {
        //取得銀行清單
        Route::post('bank', 'Bank\MainController@getDropdown');
        //取得額度變更類型清單
        Route::post('transType', 'Record\FundController@getDropdown');
    });

    //公告相關
    Route::prefix('announcement')->group(function () {
        //公告資訊
        Route::post('list', 'Announcement\AnnouncementController@getList');
    });

    //充值相關
    Route::prefix('deposit')->group(function () {
        //充值銀行資訊
        Route::post('bank', 'Payment\DepositController@getBank');
        //取得充值的銀行/錢包
        Route::post('account', 'Payment\DepositController@getBankAccount');
        //提交充值
        Route::post('create', 'Payment\DepositController@create');
        //上傳圖檔
        Route::post('updateImage', 'Payment\DepositController@uploadImage');
        //充值清單
        Route::post('orders', 'Payment\DepositController@getOrders');
        //充值明細
        Route::post('order', 'Payment\DepositController@getOrder');
    });

    //提領相關
    Route::prefix('withdrawal')->group(function () {
        //領提銀行資訊
        Route::post('bank', 'Payment\WithdrawalController@getBank');
        //領提銀行資訊
        Route::post('bankAccount', 'Member\MemberController@getBankAccount');
        //領提充值
        Route::post('create', 'Payment\WithdrawalController@create');
        //提領清單
        Route::post('orders', 'Payment\WithdrawalController@getOrders');
        //提領明細
        Route::post('order', 'Payment\WithdrawalController@getOrder');
    });

    //Promotion相關
    Route::prefix('promotion')->group(function () {
        //Promotion清單
        Route::post('list', 'Promotion\PromotionController@getList');
        //Promotion清單
        Route::post('popup', 'Promotion\PromotionController@getPopup');
    });

    //礦機相關
    Route::prefix('miner')->group(function () {
        //獲取待出售礦機列表
        Route::post('getPurchase', 'Miner\MinerController@getPurchase');
        //獲取待團購礦機型號列表
        Route::post('getGroup', 'Miner\MinerController@getGroup');
        //購買礦機
        Route::post('doPurchase', 'Miner\MinerController@doPurchase');
        //獲取待出租礦機列表
        Route::post('getRentAll', 'Miner\MinerController@getRentAll');
        //獲取特定型號待出租礦機列表
        Route::post('getRentList', 'Miner\MinerController@getRentList');
        //租礦機
        Route::post('doRent', 'Miner\MinerController@doRent');
        //創建團購
        Route::post('createGroup', 'Miner\MinerController@createGroup');
        //獲取特定型號待團購礦機列表
        Route::post('getGroupList', 'Miner\MinerController@getGroupList');
        //團購礦機
        Route::post('doGroup', 'Miner\MinerController@doGroup');
        //取消團購礦機
        Route::post('cancelGroup', 'Miner\MinerController@cancelGroup');
        //出售礦機
        Route::post('sellBackMiner', 'Miner\MinerController@sellBackMiner');
        //出售團購礦機
        Route::post('sellBackGroupMiner', 'Miner\MinerController@sellBackGroupMiner');
        //礦機殘值
        Route::post('getRemain', 'Miner\CalculateController@getRemain');
        //礦機殘值試算
        Route::post('getTrailRemain', 'Miner\CalculateController@getTrailRemain');
        //返佣列表
        Route::post('getRebateList', 'Miner\MinerController@getRebateList');
        //推薦獎勵列表
        Route::post('getReferralRewardList', 'Miner\MinerController@getReferralRewardList');

    });

    //Record
    Route::prefix('record')->group(function () {
        //取得銀行清單
        Route::post('fund', 'Record\FundController@getList');
        //取得銀行清單
        Route::post('agentCenter', 'Record\AgentCenterController@getList');
        //Sales Rebate
        Route::post('rebate', 'Record\RebateController@getList');
        //Referral Rewards
        Route::post('reward', 'Record\RewardController@getList');
    });

    //tawk chat 相關
    Route::prefix('tawk')->group(function () {
        //取得tawk user id
        Route::post('getHash', 'TawkController@getHash');
        Route::post('getSSLogin', 'TawkController@getSSLogin');
    });

    //App
    Route::prefix('app')->group(function () {
        //取得App資訊
        Route::post('getInfo', 'App\AppController@getInfo');
    });

});

Route::any('{any}', function () {
    return response()->json([
        'code' => 9999,
        'message' => 'fail'
    ], 404);
});
