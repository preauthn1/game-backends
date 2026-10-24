<?php

namespace App\Helpers;

use Carbon\Carbon;
use App\Enums\MinerEnum as MinerEnum;
use App\Enums\TimeEnum as TimeEnum;

class MinerHelper
{
    /**
     * 礦機可售回判斷
     *
     * @param array $miner 礦機資訊
     *
     */
    public static function sellBack(array $miner): array
    {
        $result = [
            'qualify' => 0,//資格 0=無資格; 1=有資格;
            'status' => 0,//狀態 0=不可售回; 1=可售回;
        ];
        if (empty($miner)) {
            return $result;
        }
        //可售回type
        $type = $miner['type'] ?? [];
        if (in_array($type, [1, 2])) {
            $result['qualify'] = 1;
            //可售回狀態
            $sellBackTime = $miner['can_sellback_time'] ?? '';
            if (!empty($sellBackTime) && Carbon::now()->gt($sellBackTime)) {
                $result['status'] = 1;
            }
        }
        return $result;
    }

    /**
     * 礦機下次收益時間
     */
    public static function nextRevenueTime(): string
    {
        $nowMy = Carbon::now()->setTimezone(TimeEnum::TimeZone['backend']);
        $result = $nowMy->format('Y-m-d ' . MinerEnum::DayRevenueTime . ':00:00');
        if ($nowMy->gte($result)) {
            $result = Carbon::parse($result)->addDay()->toDateTimeString();
        }
        return $result;
    }

    /**
     * 礦機下次折舊時間 miner todo 抓取折舊表最新一筆時間+2周 如果沒有就現在+2周
     */
    public static function nextDepreciableTime(): string
    {
        return Carbon::now()->setTimezone(TimeEnum::TimeZone['backend'])->addWeeks(2)->toDateString();
    }

    //礦機相關時間 先用+8時區計算後轉UTC+0
    public function getMinerTimesUtc($cycleDays = 0): array
    {
        $nowMy = Carbon::now()->setTimezone(TimeEnum::TimeZone['backend']);
        //開始時間start = 隔天00:00:00
        $startMy = Carbon::parse($nowMy)->addDay()->startOfDay();
        //結束時間end = 今天23:59:59 +3年
        $endMy = Carbon::parse($nowMy)->addYears(3)->endOfDay();
        //可售回時間 can_sellback = 今天23:59:59 + 28天
        $canSellBackMy = Carbon::parse($nowMy)->addDays(28)->endOfDay();
        //若有週期 按周期天數計算
        if (!empty($cycleDays ?? 0)) {
            $endMy = Carbon::parse($startMy)->addDays(($cycleDays - 1))->endOfDay();
            $canSellBackMy = Carbon::parse($startMy)->addDays(($cycleDays - 1))->endOfDay();
        }
        return [
            'start' => Carbon::parse($startMy)->setTimezone(TimeEnum::TimeZone['db'])->toDateTimeString(),
            'end' => Carbon::parse($endMy)->setTimezone(TimeEnum::TimeZone['db'])->toDateTimeString(),
            'can_sellback' => Carbon::parse($canSellBackMy)->setTimezone(TimeEnum::TimeZone['db'])->toDateTimeString(),
        ];
    }

    //購買或團購礦機可租數量及週期
    public function getPromo($promoJson = ''): array
    {
        $promoData = [];
        if (!empty($promoJson)) {
            $promoData = json_decode($promoJson, true);
        }
        return [
            'p_num' => $promoData['purchase']['num'] ?? 0,
            'p_days' => $promoData['purchase']['days'] ?? 0,
            'g_num' => $promoData['group']['num'] ?? 0,
            'g_days' => $promoData['group']['days'] ?? 0,
            'r_ec' => $promoData['rent']['experience_commission'] ?? '0.00',
        ];
    }
}
