<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\MemberMinerDepreciation;
use App\Helpers\Common;
use Illuminate\Support\Facades\DB;

/**
 * Class MiningBlockRepository.
 */
class DepreciationRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return MemberMinerDepreciation::class;
    }

    public function getList(object $params)
    {
        $start_time = date('Y-m-d H:i:00', strtotime($params->start_time . '-8hour'));
        $end_time = date('Y-m-d H:i:59', strtotime($params->end_time . '-8hour'));

        return DB::table('member_miner_depreciations as mmd')
            ->join('member_miners as mm', 'mm.id', 'mmd.member_miner_id')
            ->join('miners as miner', function ($join) use ($params) {
                $join->on('miner.id', 'mm.miner_id')
                    ->when(Common::IsNotNullOrEmptyString($params->name), function ($query) use ($params) {
                        $query->where('miner.name', $params->name);
                    });
            })
            ->join('members as m', function ($join) use ($params) {
                $join->on('m.id', 'mm.member_id')
                    ->when(Common::IsNotNullOrEmptyString($params->username), function ($query) use ($params) {
                        $query->where('m.username', $params->username);
                    })
                    ->when(isset($params->member_type), function ($query) use ($params) {
                        $query->where('m.type', $params->member_type);
                    });
            })
            ->join('agents as a', function ($join) use ($params) {
                $join->on('a.id', 'm.agent_id')
                    ->where('a.hierarchy_path', 'like', $params->HIERARCHY_PATH . '%');
            })
            ->when(isset($params->type), function ($query) use ($params) {
                $query->where('mm.type', $params->type);
            })
            ->whereBetween('mmd.summary_time', [$start_time, $end_time])
            ->orderBy('mmd.summary_time', 'desc')
            ->select(
                'mmd.id as id',
                'mmd.member_miner_id as mm_id',
                'mmd.depreciation_amount',
                'mm.member_id',
                'm.username',
                'm.type as member_type',
                'mm.miner_id',
                'miner.name',
                'mm.type',
                'mm.status',
                'mmd.summary_time',
            );
    }
}
