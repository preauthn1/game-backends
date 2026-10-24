<?php

namespace App\Enums;

enum TransactionTypeEnum
{
    const TYPE_IN = 1; # Deposit
    const TYPE_OUT = 2; # Withdrawal
    const TYPE_PURCHASE = 3; # 購買/團購礦機
    const TYPE_RENTAL = 4; # 租賃礦機
    const TYPE_CANCEL_PURCHASE = 5; # 取消團購礦機
    const TYPE_SELL_MINING = 6; # 賣回購買/團購礦機
    const TYPE_MINING_PROFITS = 7; # 挖礦利潤
    const TYPE_MINING_PROFITS_GROUP = 8; # 挖礦利潤(團購)
    const TYPE_MINING_PROFITS_RENTAL = 9; # 挖礦利潤(租賃)
    const TYPE_REFERRAL_REWARD = 10; # 推薦獎勵
    const TYPE_SALES_REBATE = 11; # 分銷獎勵
    const TYPE_ADJUST = 12; # 人工上分
    const TYPE_REGISTER_REWARD = 15; # 註冊獎勵
    const TYPE_REWARD = 16; # 人工獎勵
    const TYPE_DEDUCT = 17; # 人工扣分
    const TYPE_EXP_COMMISSION = 18; # 體驗返佣
}
