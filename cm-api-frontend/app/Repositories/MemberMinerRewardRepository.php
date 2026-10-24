<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\MemberMinerReward;
use DB;

/**
 * Class MemberMinerRewardRepository.
 */
class MemberMinerRewardRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return MemberMinerReward::class;
    }

    public function getList($params)
    {
        $today = date('Y-m-d', strtotime('+8hour'));
        $start_date = date('Y-m-d', strtotime($today . '-' . $params->days . 'day'));
        $start_time = date('Y-m-d H:i:s', strtotime($start_date . '-8hour'));

        $model = DB::table('member_miner_rewards as mmr')
            ->join('miner_rewards as mr', 'mr.id', 'mmr.miner_reward_id')
            ->join('miners as miner', 'miner.id', 'mr.miner_id')
            ->where('mmr.summary_time', '>=', $start_time)
            ->where('mmr.member_id', $params->ID)
            ->select(
                'mmr.summary_time',
                'mmr.amount as amount',
                'mr.quantity as members',
                'miner.id as miner_id',
                'miner.name as miner_name'
            );
        return $model;
    }

    public function getLastFiveList($member_id)
    {
        return DB::table('member_miner_rewards as mmr')
            ->join('miner_rewards as mr', 'mr.id', 'mmr.miner_reward_id')
            ->join('miners as miner', 'miner.id', 'mr.miner_id')
            ->where('mmr.member_id', $member_id)
            ->orderBy('mmr.summary_time', 'desc')
            ->orderBy('mmr.id', 'desc')
            ->limit(5)
            ->select(
                'miner.id as miner_id',
                'miner.name as miner_name',
                'mr.quantity as miner_quantity',
                'mmr.summary_time',
                DB::raw('mmr.amount as miner_reward')
            )
            ->get();
    }

    // Total Reward
    public function getTotalReward($member_id)
    {
        return $this->model->where('member_id', $member_id)
            ->where('status', 1)
            ->sum('amount');
    }
}
