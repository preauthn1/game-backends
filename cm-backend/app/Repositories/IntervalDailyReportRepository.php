<?php

namespace App\Repositories;

use App\Enums\TimeEnum;
use App\Helpers\Common;
use Illuminate\Support\Carbon;
use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\IntervalDailyReport;
use Illuminate\Support\Facades\DB;

class IntervalDailyReportRepository extends BaseRepository
{
    public function model()
    {
        return IntervalDailyReport::class;
    }

    public function getReport($params)
    {
        $startMy = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::parse($params->start_date)->startOfDay(), TimeEnum::TimeZone['backend']);
        $endMy = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::parse($params->end_date)->endOfDay(), TimeEnum::TimeZone['backend']);
        if (!empty($params->start_time)) {
            $startMy = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::parse($params->start_time), TimeEnum::TimeZone['backend']);
        }
        if (!empty($params->end_time)) {
            $endMy = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::parse($params->end_time), TimeEnum::TimeZone['backend']);
        }
        $start_time = Carbon::parse($startMy)->setTimezone(TimeEnum::TimeZone['db'])->format('Y-m-d H:i:s');
        $end_time = Carbon::parse($endMy)->setTimezone(TimeEnum::TimeZone['db'])->format('Y-m-d H:i:s');

        return DB::table('interval_daily_reports as idr')
            ->join('members as m', 'm.id', 'idr.member_id')
            ->join('agents as a', 'a.id', 'm.agent_id')
            ->where('idr.data_time', '>=', $start_time)
            ->where('idr.data_time', '<=', $end_time)
            ->where('a.hierarchy_path', 'like', $params->hierarchy_path ? $params->hierarchy_path . '%' : $params->HIERARCHY_PATH . '%')
            ->groupByRaw('DATE(DATE_ADD(idr.data_time, INTERVAL 8 HOUR))')
            ->select(
                DB::raw('DATE(DATE_ADD(idr.data_time, INTERVAL 8 HOUR)) as date'),
                DB::raw('COUNT(DISTINCT CASE WHEN idr.regi_count > 0 THEN idr.member_id END) AS regi_count'),
                DB::raw('COUNT(DISTINCT CASE WHEN idr.login_count > 0 THEN idr.member_id END) AS login_count'),
                DB::raw('COUNT(DISTINCT CASE WHEN idr.active_count > 0 THEN idr.member_id END) AS active_count'),
                DB::raw('SUM(idr.deposit_amount) as total_deposit_amount'),
                DB::raw('SUM(idr.withdrawal_amount) as total_withdrawal_amount'),
                DB::raw('SUM(idr.purchase_amount) as total_purchase_amount'),
                DB::raw('SUM(idr.rent_amount) as total_rent_amount'),
                DB::raw('SUM(idr.purchase_income) as total_purchase_income'),
                DB::raw('SUM(idr.group_income) as total_group_income'),
                DB::raw('SUM(idr.rent_income) as total_rent_income'),
                DB::raw('SUM(idr.reward_income) as total_reward_income'),
                DB::raw('SUM(idr.rebate_income) as total_rebate_income'),
                DB::raw('SUM(idr.award_amount) as award_amount'),
            );
    }

    public function getMajorRank($params)
    {
        $startMy = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::parse($params->start_date)->startOfDay(), TimeEnum::TimeZone['backend']);
        $endMy = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::parse($params->end_date)->endOfDay(), TimeEnum::TimeZone['backend']);
        if (!empty($params->start_time)) {
            $startMy = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::parse($params->start_time), TimeEnum::TimeZone['backend']);
        }
        if (!empty($params->end_time)) {
            $endMy = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::parse($params->end_time), TimeEnum::TimeZone['backend']);
        }
        $start_time = Carbon::parse($startMy)->setTimezone(TimeEnum::TimeZone['db'])->format('Y-m-d H:i:s');
        $end_time = Carbon::parse($endMy)->setTimezone(TimeEnum::TimeZone['db'])->format('Y-m-d H:i:s');

        return DB::table('interval_daily_reports as idr')
            ->join('members as m', 'm.id', 'idr.member_id')
            ->join('agents as a', 'a.id', 'm.agent_id')
            ->where('idr.data_time', '>=', $start_time)
            ->where('idr.data_time', '<=', $end_time)
            ->where('a.hierarchy_path', 'like', $params->hierarchy_path ? $params->hierarchy_path . '%' : $params->HIERARCHY_PATH . '%')
            ->whereRaw('idr.deposit_amount+idr.withdrawal_amount > 0')
            ->groupBy('idr.member_id')
            ->orderBy('total_amount', 'desc')
            ->when(Common::IsNotNullOrEmptyString($params->limit), function ($query) use ($params) {
                $query->limit($params->limit);
            })
            ->select(
                'm.id',
                'm.username',
                'a.id as agent_id',
                'a.username as agent_name',
                DB::raw('SUM(idr.deposit_amount) as total_deposit_amount'),
                DB::raw('SUM(idr.withdrawal_amount) as total_withdrawal_amount'),
                DB::raw('SUM(idr.deposit_amount + idr.withdrawal_amount) as total_amount'),
            );
    }
}
