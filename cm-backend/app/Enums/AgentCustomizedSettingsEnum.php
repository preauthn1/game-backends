<?php

namespace App\Enums;


class AgentCustomizedSettingsEnum
{
    const SMS_VERIFY = 1001; //短信验证
    const CAPTCHA_VERIFY = 1002; //图形验证码验证
    const REGISTER_PROMO_AMOUNT = 1003; //註冊送金額
    const USDT_DEPOSIT_PROMO_TXT = 1101; //USDT充值优惠文字
    const WITHDRAWAL_REQUIRES_VERIFIED_ID = 1301; //提现需实名认证
    const PURCHASE_MINER_REQUIRES_VERIFIED_ID = 1302; //购买矿机需实名认证
    const RENT_MINER_REQUIRES_VERIFIED_ID = 1303; //租赁矿机需实名认证
    const GROUP_MINER_REQUIRES_VERIFIED_ID = 1304; //团购矿机需实名认证
    const ANDROID_DOWNLOAD_URL = 1401; //安卓下载地址
    const IOS_DOWNLOAD_URL = 1402; //IOS下载地址
    const ANDROID_MIN_VERSION = 1403; //安卓最低版本
    const IOS_MIN_VERSION = 1404; //IOS最低版本
    const TWO_FA_WINDOW = 1601; //2fa時間
    const MANUAL_ADD_OPTIONS = 1701; //人工上分选项
    const MANUAL_SUB_OPTIONS = 1702; //人工下分选项
    const MINING_BLOCK_URL = 1703; //挖礦難度網址
    const MANUAL_REWARD_OPTIONS = 1704; //人工獎勵选项
    const MANUAL_DEDUCT_OPTIONS = 1705; //人工扣分选项
}
