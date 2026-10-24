<?php

namespace App\Enums;

enum TransactionTypeEnum
{
    const TYPE_IN = 1;
    const TYPE_OUT = 2;
    const TYPE_ADJUST = 3;

    const TransactionType = [
        'manualAdd' => 12, //人工上分
        'manualSub' => 13, //人工下分
        'manualReward' => 16, //人工獎勵
        'manualDeduct' => 17, //人工扣分
    ];
}
