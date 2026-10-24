<?php

namespace App\Services;

use Carbon\Carbon;
use App\Repositories\MemberMinerRepository;
use App\Repositories\MemberMinerIncomeRepository;
use App\Repositories\MemberMinerRewardRepository;
use App\Repositories\MemberMinerRebateRepository;
use App\Repositories\MemberMinerDepreciationRepository;
use App\Repositories\MemberMinerD;
use Exception;

class MemberMinerService extends BaseService
{
    public function __construct()
    {
        $this->repository = new MemberMinerRepository();
    }

    public function getById($id)
    {
        return $this->repository->getById($id);
    }

    //取得殘值
    public function getResidualValue($id)
    {
        $repository = new MemberMinerDepreciationRepository();
        $memberMinerDepreciation = $repository->getResidualValue($id);
        if (is_object($memberMinerDepreciation)) {
            return $memberMinerDepreciation-> residual_value;
        }
        return 0;
    }

    public function getRevenueList($member_miner_id)
    {
        $repository = new MemberMinerIncomeRepository();
        $list = $repository->getRevenueListByMemberMinerId($member_miner_id);
        $list = $list->map(function ($item) {
            // 轉換為 +8 時區並格式化為 'Y-m-d H:i'
            $item->summary_time = Carbon::parse($item->summary_time, 'UTC')->setTimezone('Asia/Taipei')->format('M d Y');
            //$item->date = Carbon::parse($item->summary_time, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d');
            $item->currency_name = 'USDT';
            return $item;
        });
        return $list;
    }

    // 總收益為每個礦機的收益加總+佣金加總+獎勵加總
    public function getTotalProceeds($member_id)
    {
        $repository = new MemberMinerIncomeRepository();
        $totalIncome = $repository->getTotalProceeds($member_id);

        $repository = new MemberMinerRewardRepository();
        $totalReward = $repository->getTotalReward($member_id);

        $repository = new MemberMinerRebateRepository();
        $totalRebate = $repository->getTotalRebate($member_id);

        return $totalIncome + $totalReward + $totalRebate;
    }

    // Today earnings
    public function getTodayEarning($member_id)
    {
        $repository = new MemberMinerIncomeRepository();
        $todayEarnList = $repository->getTodayEarning($member_id);
        $todayEarn = 0;
        $todayEarnList->map(function ($item) use (&$todayEarn) {
            $todayEarn = bcadd($todayEarn, number_format($item->amount, 2), 2);
        });
        return (float)$todayEarn;
    }
}
