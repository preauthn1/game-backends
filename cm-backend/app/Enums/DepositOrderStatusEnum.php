<?php

namespace App\Enums;

enum DepositOrderStatusEnum: int
{
    const CREATE = 0; //建立
    const SUCCESS = 1; //成功
    const FAIL = 2;    //失敗
    const PENDING = 3;  //待審核
}
