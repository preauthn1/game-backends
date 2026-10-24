<?php

namespace App\Repositories;

use App\Helpers\Common;
use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\MemberMinerRebate;
use DB;

/**
 * Class MemberMinerRebateRepository.
 */
class MemberMinerRebateRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return MemberMinerRebate::class;
    }

    public function getSummaryList($member_id)
    {
        $sub = DB::table('member_miner_rebates as mmr')
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
                DB::raw('IFNULL(SUM(mmr.amount), 0) as miner_rebate')
            )
            ->get();
        return $model;
    }

    public function getList(object $params)
    {
        $start_time = date('Y-m-d H:i:00', strtotime($params->start_time . '-8hour'));
        $end_time = date('Y-m-d H:i:59', strtotime($params->end_time . '-8hour'));

        return DB::table('member_miner_rebates as mmr')
            ->join('miners as miner', function ($join) use ($params) {
                $join->on('miner.id', 'mmr.miner_id')
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
            ->join('members as mem', function ($join) use ($params) {
                $join->whereRaw("FIND_IN_SET(mem.id, JSON_UNQUOTE(JSON_EXTRACT(example.invalid, '$.member_ids'))) > 0")
                    ->when(Common::IsNotNullOrEmptyString($params->account), function ($query) use ($params) {
                        $query->where('mem.username', $params->account);
                    });
            })
            ->whereBetween('mmr.summary_time', [$start_time, $end_time])
            ->groupBy('mmr.id')
            ->select(
                'mmr.id',
                'mmr.member_id',
                'm.username',
                'mmr.miner_id',
                'miner.name',
                'mmr.summary_time',
                'mmr.amount as amount',
                DB::raw("JSON_UNQUOTE(JSON_EXTRACT(example.invalid, '$.member_ids')) as member_ids"),
                DB::raw("GROUP_CONCAT(mem.username SEPARATOR ' / ') as accounts"),
                DB::raw("JSON_UNQUOTE(JSON_EXTRACT(example.invalid, '$.price')) as total_amount"),
                DB::raw("JSON_UNQUOTE(JSON_EXTRACT(example.invalid, '$.percentage')) as percentage")
            );
    }
}
