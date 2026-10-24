<?php

namespace App\Repositories;

use Carbon\Carbon;
use Illuminate\Support\Collection;
use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\Miner;
use App\Models\MemberMinerDepreciation;
use Illuminate\Support\Facades\DB;
use App\Enums\MinerEnum as MinerEnum;

class MinerRepository extends BaseRepository
{
    public function model(): string
    {
        return Miner::class;
    }

    public function findById(array $params = [])
    {
        return $this->model
            ->where('id', $params['id'])
            ->where('organization_id', $params['organization_id'])
            ->where('agent_group_id', $params['agent_group_id'])
            ->where('is_show',1)
            ->where('status',1)
            ->first();
    }

    /**
     * 根據條件獲取待購買剩餘數量及礦機資訊
     *
     * @param array $params 條件
     * @return array
     */
    public function getPurchaseMiners(array $params = []): array
    {
        //子查詢
        $subJoin = DB::table('member_miners as mm')
            ->select('miner_id', DB::raw('count(*) as cnt'))
            ->where('mm.end_time', '>', Carbon::now())
            ->where('mm.type', MinerEnum::MinerType['purchase'])
            ->groupBy('mm.miner_id');
        $subJoin2 = DB::table('currencies as c')
            ->where('c.status', 1);
        //主查詢
        $model = DB::table('miners as m')
            ->select(
                DB::raw('(m.purchase_total - IFNULL(mm.cnt, 0)) as count'),
                'm.*',
                'c.name as mining_currency',
            )
            ->leftJoin(DB::raw('(' . $subJoin->toSql() . ') as mm'), function ($join) {
                $join->on('mm.miner_id', 'm.id');
            })
            ->leftJoin(DB::raw('(' . $subJoin2->toSql() . ') as c'), function ($join) {
                $join->on('c.id', 'm.currency_id');
            })
            ->mergeBindings($subJoin)
            ->mergeBindings($subJoin2)
            ->where('m.is_show', 1)
            ->where('m.status', 1)
            ->where('m.organization_id',$params['organization_id'])
            ->where('m.agent_group_id',$params['agent_group_id']);
        //主查詢增加條件
        if (!empty($params['miner_id'] ?? 0)) {
            $model->where('m.id', $params['miner_id']);
        }
        return $model->get()->toArray();
    }

    /**
     * 根據條件獲取待出租剩餘數量及礦機資訊
     *
     * @param array $params 條件
     * @return array|Collection
     */
    public function getRentMiners(array $params = []): array|Collection
    {
        $memberId = !empty($params['member_id'] ?? 0) ? $params['member_id'] : 0;
        //子查詢
        $subJoin = DB::table('member_miners as mm')
            ->select('miner_id', DB::raw('count(*) as cnt'))
            ->whereNull('mm.end_time')
            ->where('mm.type', MinerEnum::MinerType['rent'])
            ->where('mm.member_id', $memberId)
            ->groupBy('mm.miner_id');
        $subJoin2 = DB::table('currencies as c')
            ->where('c.status', 1);
        //主查詢
        $model = DB::table('miners as m')
            ->select(
                DB::raw('IFNULL(mm.cnt, 0) as count'),
                'm.*',
                'c.name as mining_currency',
            )
            ->leftJoin(DB::raw('(' . $subJoin->toSql() . ') as mm'), function ($join) {
                $join->on('mm.miner_id', 'm.id');
            })
            ->leftJoin(DB::raw('(' . $subJoin2->toSql() . ') as c'), function ($join) {
                $join->on('c.id', 'm.currency_id');
            })
            ->mergeBindings($subJoin)
            ->mergeBindings($subJoin2)
            ->where('m.is_show', 1)
            ->where('m.status', 1)
            ->where('m.organization_id',$params['organization_id'])
            ->where('m.agent_group_id',$params['agent_group_id'])
            ->where('mm.cnt', '>', 0);
        //主查詢增加條件
        if (!empty($params['miner_id'] ?? '')) {
            $model->where('m.id', $params['miner_id']);
        }
        return $model->get()->toArray();
    }

    /**
     * 待出租礦機列表
     *
     * @param array $params 條件
     * @return array|Collection
     */
    public function getRentMinersList(array $params = []): array|Collection
    {
        $memberId = $params['member_id'] ?? 0;
        $minerId = $params['miner_id'] ?? 0;
        if (empty($memberId) || empty($minerId)) {
            return [];
        }
        //子查詢
        $subJoin = DB::table('miners as m')
            ->where('m.id', $minerId)
            ->where('m.is_show', 1)
            ->where('m.status', 1);
        $subJoin2 = DB::table('currencies as c')
            ->where('c.status', 1);
        //主查詢
        $model = DB::table('member_miners as mm')
            ->select(
                'mm.id as mm_id',
                DB::raw('example.invalid->"$.cycle_days" as cycle_days'),
                'm.*',
                'c.name as mining_currency',
            )
            ->leftJoin(DB::raw('(' . $subJoin->toSql() . ') as m'), function ($join) {
                $join->on('mm.miner_id', 'm.id');
            })
            ->leftJoin(DB::raw('(' . $subJoin2->toSql() . ') as c'), function ($join) {
                $join->on('c.id', 'm.currency_id');
            })
            ->mergeBindings($subJoin)
            ->mergeBindings($subJoin2)
            ->whereNull('mm.end_time')
            ->where('mm.type', MinerEnum::MinerType['rent'])
            ->where('mm.member_id', $memberId)
            ->where('mm.miner_id', $minerId)
            ->where('m.organization_id',$params['organization_id'])
            ->where('m.agent_group_id',$params['agent_group_id'])
            ->orderByRaw('cycle_days desc, id asc');
        return $model->get()->toArray();
    }

    /**
     * 根據條件獲取待團購剩餘數量及礦機資訊
     *
     * @param array $params 條件
     * @return array
     */
    public function getGroupMiners(array $params = []): array
    {
        //子查詢
        $subJoin = DB::table('member_miners as mm')
            ->select('miner_id', DB::raw('count(*) as cnt'), 'type')
            ->whereNull('mm.end_time')
            ->where('mm.type', MinerEnum::MinerType['group'])
            ->groupBy('mm.miner_id');
        $subJoin2 = DB::table('currencies as c')
            ->where('c.status', 1);
        //主查詢
        $model = DB::table('miners as m')
            ->select(
                DB::raw('IFNULL(mm.cnt, 0) as count'),
                'mm.type',
                'm.*',
                'c.name as mining_currency',
            )
            ->leftJoin(DB::raw('(' . $subJoin->toSql() . ') as mm'), function ($join) {
                $join->on('mm.miner_id', 'm.id');
            })
            ->leftJoin(DB::raw('(' . $subJoin2->toSql() . ') as c'), function ($join) {
                $join->on('c.id', 'm.currency_id');
            })
            ->mergeBindings($subJoin)
            ->mergeBindings($subJoin2)
            ->where('m.is_show', 1)
            ->where('m.status', 1)
            ->where('m.organization_id',$params['organization_id'])
            ->where('m.agent_group_id',$params['agent_group_id']);
        //主查詢增加條件
        if (!empty($params['miner_id'] ?? 0)) {
            $model->where('m.id', $params['miner_id']);
        }
        return $model->get()->toArray();
    }

    /**
     * 待團購礦機列表
     *
     * @param array $params 條件
     * @return array
     */
    public function getGroupMinersList(array $params = []): array
    {
        $minerId = $params['miner_id'] ?? 0;
        if (empty($minerId)) {
            return [];
        }
        //子查詢會員
        $subJoinMember = DB::table('members as member')
            ->where('member.status', 1);

        //子查詢礦機
        $subJoinMiner = DB::table('miners as m')
            ->where('m.id', $minerId)
            ->where('m.is_show', 1)
            ->where('m.status', 1);
        //主查詢
        $model = DB::table('member_miners as mm')
            ->select(
                'mm.id as mm_id',
                'mm.member_id',
                'member.username',
            )
            ->leftJoin(DB::raw('(' . $subJoinMiner->toSql() . ') as m'), function ($join) {
                $join->on('mm.miner_id', 'm.id');
            })
            ->mergeBindings($subJoinMiner)
            ->leftJoin(DB::raw('(' . $subJoinMember->toSql() . ') as member'), function ($join) {
                $join->on('mm.member_id', 'member.id');
            })
            ->mergeBindings($subJoinMember)
            ->whereNull('mm.end_time')
            ->where('mm.type', MinerEnum::MinerType['group'])
            ->where('mm.miner_id', $minerId)
            ->where('m.organization_id',$params['organization_id'])
            ->where('m.agent_group_id',$params['agent_group_id']);
        if (!empty($params['member_id'] ?? '')) {
            $model->orderByRaw('CASE WHEN mm.member_id = ' . $params['member_id'] . ' THEN 0 ELSE 1 END, mm.id');
        } else {
            $model->orderBy('mm.id');
        }
        return $model->get()->toArray();
    }

    //獲取最新一筆折舊紀錄
    public function getDepreciationLast(array $params = []): array
    {
        if (empty($params['mm_id'] ?? '')) {
            return [];
        }
        $model = MemberMinerDepreciation::where('member_miner_id', $params['mm_id'])
            ->orderBy('summary_time', 'desc')->limit(1);
        if (!empty($params['calculate_date'])) {
            $model->where('summary_time', '<=', $params['calculate_date']);
        }
        return $model->get()->toArray();
    }

    public function getTopSelling($tag, $num)
    {
        return $this->model
            ->where('status', 1)
            ->where(DB::raw("JSON_UNQUOTE(JSON_EXTRACT(info, '$.tag'))"), $tag)
            ->limit($num)
            ->select(
                'name',
                'image',
                'purchase_price as amount',
                'hash_rate',
                'power_consumption',
                DB::raw("JSON_UNQUOTE(JSON_EXTRACT(info, '$.day_elec_fee')) as day_elec_fee")
            )
            ->get();
    }

    /**
     * 獲取返佣列表
     */
    public function getRebateList($params): array
    {
        $model = DB::table('miner_rebates as mr')
            ->join('miners as m', 'm.id', 'mr.miner_id')
            ->where('m.status', 1)
            ->where('m.organization_id', $params->ORGANIZATION_ID)
            ->where('m.agent_group_id', $params->MINER_GROUP)
            ->orderBy('sort','desc')
            ->select('m.id as miner_id', 'm.name as miner_name', 'percentage1', 'percentage2', 'percentage3');
        return $model->get()->toArray();
    }
    /**
     * 獲取推薦獎勵列表
     */
    public function getReferralRewardList($params): array
    {
        $model = DB::table('miner_rewards as mr')
            ->join('miners as m', 'm.id', 'mr.miner_id')
            ->where('m.status', 1)
            ->where('m.organization_id', $params->ORGANIZATION_ID)
            ->where('m.agent_group_id', $params->MINER_GROUP)
            ->orderBy('sort','desc')
            ->select('m.id as miner_id', 'm.name as miner_name', 'mr.quantity', 'mr.amount');
        return $model->get()->toArray();
    }
}
