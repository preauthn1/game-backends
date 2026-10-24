<?php

namespace App\Helpers;

use App\Repositories\MemberMinerRepository;
use App\Repositories\MinerRepository;
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
     * 礦機下次折舊時間
     *
     * @param int $mm_id 會員礦機ID
     * @param int $depreciation_cycle 礦機折舊週期
     *
     */
    public static function nextDepreciableTime(int $mm_id = 0, int $depreciation_cycle = 0): string
    {
        if (empty($mm_id)) {
            //現在+礦機折舊天數預設值
            return Carbon::now()->setTimezone(TimeEnum::TimeZone['backend'])->addDays(MinerEnum::DefaultDepreciationDays)->toDateString();
        }

        $minerRepository = new MinerRepository();
        $DepreciationList = $minerRepository->getDepreciationLast([
            'mm_id' => $mm_id,
        ]);
        if (empty($DepreciationList[0]['created_at'] ?? '')) {
            //現在+礦機折舊天數預設值
            return Carbon::now()->setTimezone(TimeEnum::TimeZone['backend'])->addDays(MinerEnum::DefaultDepreciationDays)->toDateString();
        }
        //折舊表最新一筆時間+礦機折舊天數
        $addDays = $depreciation_cycle ?: MinerEnum::DefaultDepreciationDays;
        return Carbon::parse($DepreciationList[0]['created_at'])->addDays($addDays)->toDateString();
    }

    /**
     * 礦機固定折舊金額
     *
     * @param array $params 傳參
     *
     */
    public static function fixDepreciable(array $params): string
    {
        if (empty($params['mm_id'] ?? '') || empty($params['member_id'] ?? '')) {
            return 0;
        }
        $memberMinerRepository = new MemberMinerRepository();
        $myMinerDetail = $memberMinerRepository->getMyMinerDetail([
            'mm_id' => $params['mm_id'],
            'member_id' => $params['member_id'],
        ]);
        $residual_value = $myMinerDetail['purchase_price'] ?? 0;
        if (MinerEnum::MinerType['group'] == ($myMinerDetail['type'] ?? 1)) {
            $residual_value = bcmul($myMinerDetail['purchase_price'], MinerEnum::GroupProportion, 4) ?? 0;
        }
        return $residual_value / MinerEnum::UsefulLifeYear / 12 / ($myMinerDetail['depreciation_cycle'] / MinerEnum::CycleDays);
    }

    /**
     * 礦機殘值計算
     *
     * @param array $params 傳參
     *
     */
    public static function calculateRemain(array $params): string
    {
        $minerRepository = new MinerRepository();
        $DepreciationList = $minerRepository->getDepreciationLast([
            'mm_id' => $params['mm_id'],
        ]);
        $residual_value = $DepreciationList[0]['residual_value'] ?? 0;
        //尚未折舊 取成本
        if (empty($residual_value)) {
            $memberMinerRepository = new MemberMinerRepository();
            $myMinerDetail = $memberMinerRepository->getMyMinerDetail([
                'mm_id' => $params['mm_id'],
                'member_id' => $params['member_id'],
            ]);
            $residual_value = $myMinerDetail['purchase_price'] ?? 0;
            if (MinerEnum::MinerType['group'] == ($myMinerDetail['type'] ?? 1)) {
                $residual_value = bcmul($myMinerDetail['purchase_price'], MinerEnum::GroupProportion, 4) ?? 0;
            }
        }
        return (string)$residual_value;
    }
}
