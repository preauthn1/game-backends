<?php

namespace App\Enums;

enum WithdrawalEnum
{
    const Channel = [
        'bank' => 1001, //銀行
        'crypto' => 1002  //加密貨幣
    ];
}
