<?php

namespace App\Repositories;

use App\Helpers\Common;
use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\MemberSalesRanking;
use Illuminate\Support\Facades\DB;

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

    public function getList($params)
    {
        return DB::table('member_sales_rankings AS msr')
            ->join('members AS m', 'm.id', 'msr.member_id')
            ->join('agents AS ag', function ($join) use ($params) {
                $join->on('ag.id', 'm.agent_id')
                    ->when(Common::IsNotNullOrEmptyString($params->HIERARCHY_PATH), function ($query) use ($params) {
                        $query->where('ag.hierarchy_path', 'like', $params->HIERARCHY_PATH . '%');
                    });
            })
            ->leftJoin('avatars AS a', 'a.id', 'm.avatar_id')
            ->select(
                'm.username',
                'a.image',
                'msr.quantity_sold',
                'msr.rebate_amount'
            );
    }
}
