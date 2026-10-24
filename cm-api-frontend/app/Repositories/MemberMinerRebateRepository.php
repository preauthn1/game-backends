<?php

namespace App\Repositories;

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

    public function getList($params)
    {
        $today = date('Y-m-d', strtotime('+8hour'));
        $start_date = date('Y-m-d', strtotime($today . '-' . $params->days . 'day'));
        $start_time = date('Y-m-d H:i:s', strtotime($start_date . '-8hour'));

        $model = DB::table('member_miner_rebates as mmr')
            ->join('miners as miner', 'miner.id', 'mmr.miner_id')
            ->where('mmr.summary_time', '>=', $start_time)
            ->where('mmr.member_id', $params->ID)
            ->select(
                'mmr.summary_time',
                'mmr.amount as amount',
                'miner.id as miner_id',
                'miner.name as miner_name',
                DB::raw("JSON_UNQUOTE(JSON_EXTRACT(example.invalid, '$.member_ids')) as member_ids"),
                DB::raw("JSON_UNQUOTE(JSON_EXTRACT(example.invalid, '$.price')) as price"),
                DB::raw("JSON_UNQUOTE(JSON_EXTRACT(example.invalid, '$.percentage')) as percentage")
            );
        return $model;
    }

    public function getLastFiveList($member_id)
    {
        return DB::table('member_miner_rebates as mmr')
            ->join('miners', 'miners.id', 'mmr.miner_id')
            ->where('mmr.member_id', $member_id)
            ->orderBy('mmr.id', 'desc')
            ->limit(5)
            ->select(
                'miners.id as miner_id',
                'miners.name as miner_name',
                'mmr.level',
                DB::raw("JSON_UNQUOTE(JSON_EXTRACT(example.invalid, '$.price')) as sales_amount"),
                DB::raw("JSON_UNQUOTE(JSON_EXTRACT(example.invalid, '$.level')) as levelTxt"),
                DB::raw("JSON_UNQUOTE(JSON_EXTRACT(example.invalid, '$.percentage')) as percentage"),
                //DB::raw("JSON_UNQUOTE(JSON_EXTRACT(example.invalid, '$.member_ids')) as member_ids"),
                'mmr.amount as miner_rebate'
            );
    }

    // Total Rebate
    public function getTotalRebate($member_id)
    {
        return $this->model->where('member_id', $member_id)
            ->where('status', 1)
            ->sum('amount');
    }
}
