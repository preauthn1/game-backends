<?php

namespace App\Repositories;

use App\Enums\MinerEnum as MinerEnum;
use App\Helpers\Common;
use Carbon\Carbon;
use Illuminate\Support\Collection;
use Illuminate\Support\Facades\DB;
use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\MemberMiner;

class MemberMinerRepository extends BaseRepository
{
    public function model(): string
    {
        return MemberMiner::class;
    }

    /**
     * 獲取最後幾筆資料
     *
     * @param array $params 條件
     * @return array|Collection
     */
    public function getLatestData(array $params): array|Collection
    {
        $model = MemberMiner::orderBy('id', 'desc');
        if (!empty($params['type'] ?? '')) {
            $model->where('type', $params['type']);
        }
        if (!empty($params['miner_id'] ?? '')) {
            $model->where('miner_id', $params['miner_id']);
        }
        if (!empty($params['member_id'] ?? '')) {
            $model->where('member_id', $params['member_id']);
        }
        $limit = ($params['limit'] ?? 0) ? $params['limit'] : 10;
        $model->limit($limit);
        return $model->get()->toArray();
    }

    /**
     * 獲取等待出租資料(舊到新)
     *
     * @param array $params 條件
     * @return array|Collection
     */
    public function getPendingRent(array $params): array|Collection
    {
        $model = MemberMiner::where('type', MinerEnum::MinerType['rent'])->whereNull('end_time')->orderBy('id', 'asc');
        if (!empty($params['miner_id'] ?? '')) {
            $model->where('miner_id', $params['miner_id']);
        }
        if (!empty($params['member_id'] ?? '')) {
            $model->where('member_id', $params['member_id']);
        }
        $limit = ($params['limit'] ?? 0) ? $params['limit'] : 10;
        $model->limit($limit);
        return $model->get()->toArray();
    }

    /**
     * 獲取我的礦機(end_time新到舊)
     *
     * @param array $params 條件
     * @return array|Collection
     */
    public function getMyMiners(array $params): array|Collection
    {
        //子查詢
        $subJoin = DB::table('member_miners as mm')
            ->where('mm.end_time', '>', Carbon::now());
        //子查詢增加條件
        if (!empty($params['typeList'] ?? [])) {
            $subJoin->whereIn('mm.type', $params['typeList']);
        }
        if (!empty($params['miner_id'] ?? '')) {
            $subJoin->where('mm.miner_id', $params['miner_id']);
        }
        if (!empty($params['member_id'] ?? 0)) {
            $subJoin->where('mm.member_id', $params['member_id']);
        }
        //主查詢
        $model = DB::table('miners as m')
            ->select(
                'mm.id as mm_id', 'm.name', 'm.image', 'mm.type',
                DB::raw('DATE_ADD(mm.created_at, INTERVAL 8 HOUR) AS created_at'),
                DB::raw('DATE_ADD(mm.end_time, INTERVAL 8 HOUR) AS end_time'),
                DB::raw('DATE_ADD(mm.can_sellback_time, INTERVAL 8 HOUR) AS sellback_start'),
            )
            ->leftJoin(DB::raw('(' . $subJoin->toSql() . ') as mm'), function ($join) {
                $join->on('mm.miner_id', 'm.id');
            })
            ->mergeBindings($subJoin)
            ->whereNotNull('mm.id')
            ->when(Common::IsNotNullOrEmptyString($params['organization_id'] ?? ''), function ($query) use ($params) {
                $query->where('m.organization_id', $params['organization_id']);
            })
            ->when(Common::IsNotNullOrEmptyString($params['agent_group_id'] ?? ''), function ($query) use ($params) {
                $query->where('m.agent_group_id', $params['agent_group_id']);
            })
            ->orderByRaw('mm.end_time DESC, mm.created_at DESC, mm.id DESC');
        return $model->get()->toArray();
    }

    /**
     * 獲取我的礦機詳情
     *
     * @param array $params 條件
     * @return array|Collection
     */
    public function getMyMinerDetail(array $params): array|Collection
    {
        $member_id = $params['member_id'] ?? 0;
        $mm_id = $params['mm_id'] ?? 0;
        if (empty($member_id) || empty($mm_id)) {
            return [];
        }
        //子查詢
        $subJoin = DB::table('miners as m')
            ->where('m.status', 1);
        $subJoin2 = DB::table('currencies as c')
            ->where('c.status', 1);
        //主查詢
        $model = DB::table('member_miners as mm')
            ->select('m.*', 'mm.id as mm_id', 'mm.type',
                DB::raw('DATE_ADD(mm.created_at ,INTERVAL 8 HOUR) as created_at'),
                DB::raw('DATE_ADD(mm.start_time ,INTERVAL 8 HOUR) as start_time'),
                DB::raw('DATE_ADD(mm.end_time ,INTERVAL 8 HOUR) as end_time'),
                DB::raw('DATE_ADD(mm.can_sellback_time ,INTERVAL 8 HOUR) as can_sellback_time'),
                'example.invalid as mm_info',
                'c.name as mining_currency'
            )
            ->leftJoin(DB::raw('(' . $subJoin->toSql() . ') as m'), function ($join) {
                $join->on('m.id', 'mm.miner_id');
            })
            ->leftJoin(DB::raw('(' . $subJoin2->toSql() . ') as c'), function ($join) {
                $join->on('c.id', 'm.currency_id');
            })
            ->mergeBindings($subJoin)
            ->mergeBindings($subJoin2)
            ->where('mm.member_id', $member_id)
            ->where('mm.id', $mm_id)
            ->where('mm.end_time', '>', Carbon::now())
            ->when(Common::IsNotNullOrEmptyString($params['organization_id'] ?? ''), function ($query) use ($params) {
                $query->where('m.organization_id', $params['organization_id']);
            })
            ->when(Common::IsNotNullOrEmptyString($params['agent_group_id'] ?? ''), function ($query) use ($params) {
                $query->where('m.agent_group_id', $params['agent_group_id']);
            });
        return collect($model->first())->toArray() ?? [];
    }

    public function getSalesList($member_ids, $miner_id)
    {
        return DB::table('member_miners as mm')
            ->join('members as m', function ($join) use ($member_ids) {
                $join->on('m.id', 'mm.member_id');
            })
            ->join('miners as miner', 'miner.id', 'mm.miner_id')
            ->whereIn('mm.type', [1, 2])
            ->whereIn('mm.member_id', $member_ids)
            ->where('mm.start_time', '<=', date('Y-m-d H:i:s'))
            ->when($miner_id > 0, function ($query) use ($miner_id) {
                $query->where('mm.member_id', $miner_id);
            })
            ->groupBy('m.id')
            ->groupBy('miner.id')
            ->select(
                'm.username as username',
                'miner.id as miner_id',
                'miner.name as miner_name',
                DB::raw('case mm.type when 2 then 0.5 else 1 end as qty'),
                'mm.created_at'
            )
            ->get();

    }
}
