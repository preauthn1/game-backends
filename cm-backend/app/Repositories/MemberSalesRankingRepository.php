<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Helpers\Common;
use App\Models\MemberSalesRanking;
use DB;

/**
 * Class MemberSalesRankingRepository.
 */
class MemberSalesRankingRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return MemberSalesRanking::class;
    }

    public function getByMemberId($member_id)
    {
        return $this->model->where('member_id', $member_id)->first();
    }

    public function getList($params)
    {
        return DB::table('member_sales_rankings AS msr')
            ->join('members AS m', function ($join) use ($params) {
                $join->on('m.id', 'msr.member_id')
                    ->when(Common::IsNotNullOrEmptyString($params->uid), function ($query) use ($params) {
                        $query->where('m.uid', $params->uid);
                    });
            })
            ->join('agents AS a', function ($join) use ($params) {
                $join->on('a.id', 'm.agent_id')
                    ->when(Common::IsNotNullOrEmptyString($params->HIERARCHY_PATH), function ($query) use ($params) {
                        $query->where('a.hierarchy_path', 'like', $params->HIERARCHY_PATH . '%');
                    });
            })
            ->orderBy('rebate_amount', 'desc')
            ->orderBy('quantity_sold', 'desc')
            ->select(
                'm.id as member_id',
                'm.uid',
                'm.username',
                'm.type',
                'msr.id',
                'msr.quantity_sold',
                'msr.rebate_amount'
            );
    }

    public function getByRankingId($id)
    {
        return DB::table('member_sales_rankings AS msr')
            ->join('members AS m', 'm.id', 'msr.member_id')
            ->select(
                'm.id as member_id',
                'm.username',
                'm.type',
                'msr.id',
                'msr.quantity_sold',
                'msr.rebate_amount'
            )
            ->where('msr.id', $id)->first();
    }

}
