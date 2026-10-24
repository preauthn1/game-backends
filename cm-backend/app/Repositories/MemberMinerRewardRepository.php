<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\MemberMinerReward;
use App\Helpers\Common;
use Illuminate\Support\Facades\DB;

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

    public function getSummaryList($member_id)
    {
        $sub = DB::table('member_miner_rewards as mmr')
            ->where('mmr.member_id', $member_id)
            ->groupBy('mmr.member_miner_id')
            ->select(
                'mmr.member_miner_id',
                DB::raw('SUM(mmr.amount) as amount')
            );

        $model = DB::table('member_miners as mm')
            ->join('miners', 'miners.id', 'mm.miner_id')
            ->leftJoin(
                DB::raw('(' . $sub->toRawSql() . ') as mmr'),
                'mmr.member_miner_id',
                'mm.id'
            )
            ->where('mm.member_id', $member_id)
            ->whereNotNull('mm.end_time')
            ->groupBy('miners.id')
            ->select(
                'miners.id as miner_id',
                'miners.name as miner_name',
                DB::raw('count(*) as miner_quantity'),
                DB::raw('IFNULL(SUM(mmr.amount), 0) as miner_reward')
            )
            ->get();
        return $model;
    }

    public function getList(object $params)
    {
        $start_time = date('Y-m-d H:i:00', strtotime($params->start_time . '-8hour'));
        $end_time = date('Y-m-d H:i:59', strtotime($params->end_time . '-8hour'));

        return DB::table('member_miner_rewards as mmr')
            ->join('miner_rewards as mr', function ($join) use ($params) {
                $join->on('mr.id', 'mmr.miner_reward_id')
                    ->when(Common::IsNotNullOrEmptyString($params->reward_people), function ($query) use ($params) {
                        $query->where('mr.quantity', $params->reward_people);
                    });
            })
            ->join('miners as miner', function ($join) use ($params) {
                $join->on('miner.id', 'mr.miner_id')
                    ->when(Common::IsNotNullOrEmptyString($params->name), function ($query) use ($params) {
                        $query->where('miner.name', $params->name);
                    });
            })
            ->join('members as m', function ($join) use ($params) {
                $join->on('m.id', 'mmr.member_id')
                    ->when(Common::IsNotNullOrEmptyString($params->username), function ($query) use ($params) {
                        $query->where('m.username', $params->username);
                    });
            })
            ->join('agents as a', function ($join) use ($params) {
                $join->on('a.id', 'm.agent_id')
                    ->where('a.hierarchy_path', 'like', $params->HIERARCHY_PATH . '%');
            })
            ->whereBetween('mmr.summary_time', [$start_time, $end_time])
            ->select(
                'mmr.id',
                'm.id as member_id',
                'm.username',
                'miner.id as miner_id',
                'miner.name',
                'mr.quantity as reward_people',
                'mmr.amount as reward_amount',
                'mmr.summary_time',
            );
    }
}
