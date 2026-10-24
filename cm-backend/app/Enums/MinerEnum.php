<?php

namespace App\Enums;

enum MinerEnum
{
    const MinerType = [
        'rent' => 0,//租
        'purchase' => 1,//買
        'group' => 2,//團購
    ];
    const CycleDays = 7; //一個周期為7天
    const GroupProportion = 0.5; //團購占比
    const DayRevenueTime = 14; //每日計算礦機收益時間(+8)
}
