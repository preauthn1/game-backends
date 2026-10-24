<?php

namespace App\Repositories;

use Carbon\Carbon;
use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\MemberMinerIncome;
use DB;

/**
 * Class MemberMinerIncomeRepository.
 */
class MemberMinerIncomeRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return MemberMinerIncome::class;
    }

    public function getRevenueListByMemberMinerId($member_miner_id, $rows = 14)
    {
        return $this->model->where('member_miner_id', $member_miner_id)
            ->where('status', 1)
            ->orderBy('summary_time', 'desc')
            ->limit($rows)
            ->select(
                'id',
                'type',
                'summary_time',
                'data_time',
                'amount as revenue',
                'info'
            )
            ->get();
    }

    // Today earnings
    public function getTodayEarning($member_id)
    {
        $today = date('Y-m-d', strtotime('+8hour'));
        $start_time = date('Y-m-d H:i:s', strtotime($today . '-8hour'));
        return $this->model->where('member_id', $member_id)
            ->where('status', 1)
            ->where('summary_time', '>=', $start_time)
            ->select('amount')
            ->get();
    }

    // Total Proceeds
    public function getTotalProceeds($member_id)
    {
        return $this->model->where('member_id', $member_id)
            ->where('status', 1)
            ->sum('amount');
    }
}
