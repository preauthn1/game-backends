<?php

namespace App\Services;

use Illuminate\Support\Carbon;
use Illuminate\Support\Collection;
use App\Repositories\IntervalDailyReportRepository;
use App\Enums\PublicEnum;

class ReportService extends BaseService
{
    public function __construct()
    {
        $this->repository = new IntervalDailyReportRepository();
    }

    public function getDaysReport($reportStart, $reportEnd, $params)
    {
        $reportRepository = new IntervalDailyReportRepository();
        $data = $reportRepository->getReport($params)->get();
        //初始數據列表
        $dailyResult = [];
        $diffDays = ceil(Carbon::parse($reportStart)->diffInDays(Carbon::parse($reportEnd)));
        for ($i = 0; $i < $diffDays; $i++) {
            $dateKey = Carbon::parse($params->start_date)->addDays($i)->format('Ymd');
            $dailyList = [
                'date' => Carbon::parse($dateKey)->format('m/d'),
                'newRegister' => 0,
                'todayLogin' => 0,
                'activeMember' => 0,
                'totalDeposit' => 0.00,
                'totalWithdrawal' => 0.00,
                'netDeposit' => 0.00,
                'totalConsume' => 0.00,
            ];
            if (2 == ($params->type ?? 1)) {
                $dailyList = array_merge($dailyList, [
                    'totalPurchase' => 0.00,
                    'totalRent' => 0.00,
                ]);
            }
            $dailyList = array_merge($dailyList, [
                'totalIncome' => 0.00,
            ]);
            if (2 == ($params->type ?? 1)) {
                $dailyList = array_merge($dailyList, [
                    'minerIncome' => 0.00,
                    'rentIncome' => 0.00,
                    'inviteIncome' => 0.00,
                    'rebateIncome' => 0.00,
                ]);
            }
            $dailyList = array_merge($dailyList, [
                'totalAward' => 0.00,
            ]);
            $dailyResult[$dateKey] = $dailyList;
        }

        $statisticField = ['newRegister', 'todayLogin', 'activeMember', 'totalDeposit', 'totalWithdrawal', 'netDeposit', 'totalConsume'];
        if (2 == ($params->type ?? 1)) {
            $statisticField = array_merge($statisticField, ['totalPurchase', 'totalRent']);
        }
        $statisticField = array_merge($statisticField, ['totalIncome']);
        if (2 == ($params->type ?? 1)) {
            $statisticField = array_merge($statisticField, ['minerIncome', 'rentIncome', 'inviteIncome', 'rebateIncome']);
        }
        $statisticField = array_merge($statisticField, ['totalAward']);

        $statisticResult['rangeTotal']['type'] = 'rangeTotal';
        $statisticResult['dayAverage']['type'] = 'dayAverage';
        $statisticResult['maxValue']['type'] = 'maxValue';
        $statisticResult['minValue']['type'] = 'minValue';
        foreach ($statisticField as $name) {
            $statisticResult['rangeTotal'][$name] = 0.00;
            $statisticResult['dayAverage'][$name] = 0.00;
            $statisticResult['maxValue'][$name] = 0.00;
            $statisticResult['minValue'][$name] = 0.00;
        }

        //根據現有資料取代初始數據
        $data->map(function ($item) use ($params, $diffDays, &$dailyResult, $statisticField, &$statisticResult) {
            $dateKey = Carbon::parse($item->date)->format('Ymd');
            $dailyList = [
                'date' => Carbon::parse($item->date)->format('m/d'),
                'newRegister' => $item->regi_count ?? 0,
                'todayLogin' => $item->login_count ?? 0,
                'activeMember' => $item->active_count ?? 0,
                'totalDeposit' => $item->total_deposit_amount ?? 0,
                'totalWithdrawal' => $item->total_withdrawal_amount ?? 0,
                'netDeposit' => bcsub($item->total_deposit_amount ?? 0, $item->total_withdrawal_amount ?? 0, 6),
                'totalConsume' => bcadd($item->total_purchase_amount ?? 0, $item->total_rent_amount ?? 0, 6),
            ];
            if (2 == ($params->type ?? 1)) {
                $dailyList = array_merge($dailyList, [
                    'totalPurchase' => $item->total_purchase_amount ?? 0,
                    'totalRent' => $item->total_rent_amount ?? 0,
                ]);
            }
            $dailyList = array_merge($dailyList, [
                'totalIncome' => bcadd($item->total_purchase_income ?? 0, bcadd($item->total_group_income ?? 0, bcadd($item->total_rent_income ?? 0, bcadd($item->total_reward_income ?? 0, $item->total_rebate_income ?? 0, 6), 6), 6), 6),
            ]);
            if (2 == ($params->type ?? 1)) {
                $dailyList = array_merge($dailyList, [
                    'minerIncome' => bcadd($item->total_purchase_income ?? 0, $item->total_group_income ?? 0, 6),
                    'rentIncome' => $item->total_rent_income ?? 0,
                    'inviteIncome' => $item->total_reward_income ?? 0,
                    'rebateIncome' => $item->total_rebate_income ?? 0,
                ]);
            }
            $dailyList = array_merge($dailyList, [
                'totalAward' => $item->award_amount ?? 0,
            ]);
            foreach ($statisticField as $name) {
                $statisticResult['rangeTotal'][$name] = bcadd($statisticResult['rangeTotal'][$name], $dailyList[$name], 6);
                $statisticResult['dayAverage'][$name] = bcdiv($statisticResult['rangeTotal'][$name], $diffDays, 6);
                $statisticResult['maxValue'][$name] = max($statisticResult['maxValue'][$name], $dailyList[$name]);
                if (0 == ($statisticResult['minValue'][$name] ?? 0) && ($dailyList[$name] ?? 0) > 0) {
                    $statisticResult['minValue'][$name] = ($dailyList[$name] ?? 0);
                } else if (($statisticResult['minValue'][$name] ?? 0) > 0 && ($dailyList[$name] ?? 0) > 0) {
                    $statisticResult['minValue'][$name] = min($statisticResult['minValue'][$name], $dailyList[$name]);
                }
            }
            $dailyResult[$dateKey] = $dailyList;
        });

        return [collect($dailyResult)->values()->all(), $statisticResult];
    }

    public function getReport($params)
    {
        $reportRepository = new IntervalDailyReportRepository();
        return $reportRepository->getReport($params)->get();
    }

    public function getMajorRank($params)
    {
        return $this->repository->getMajorRank($params)->get();
    }
}
