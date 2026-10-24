<?php

namespace App\Enums;

enum DepositEnum
{
    const OrderType = [
        'created' => 0, //建立
        'success' => 1, //成功
        'fail' => 2,    //失敗
        'pending' => 3  //待審核
    ];
}
