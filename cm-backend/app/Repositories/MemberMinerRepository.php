<?php

namespace App\Repositories;

use App\Enums\TimeEnum;
use Carbon\Carbon;
use Illuminate\Support\Collection;
use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Helpers\Common;
use App\Enums\MinerEnum as MinerEnum;
use App\Models\MemberMiner;
use Illuminate\Support\Facades\DB;

class MemberMinerRepository extends BaseRepository
{
    public function model(): string
    {
        return MemberMiner::class;
    }

    public function getCountByMinerId($miner_id, $type)
    {
        return $this->model->where('miner_id', $miner_id)
            ->where('type', $type)
            ->where('end_time', '>', date('Y-m-d H:i:s'))
            ->count();
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
            ->select('mm.id as mm_id', 'm.name', 'm.image', 'mm.type', 'mm.created_at', 'mm.end_time')
            ->leftJoin(DB::raw('(' . $subJoin->toSql() . ') as mm'), function ($join) {
                $join->on('mm.miner_id', 'm.id');
            })
            ->mergeBindings($subJoin)
            ->whereNotNull('mm.id')
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
        //主查詢
        $model = DB::table('member_miners as mm')
            ->select(
                'm.*',
                'mm.id as mm_id',
                'mm.type',
                DB::raw('DATE_ADD(mm.created_at ,INTERVAL 8 HOUR) as created_at'),
                DB::raw('DATE_ADD(mm.start_time ,INTERVAL 8 HOUR) as start_time'),
                DB::raw('DATE_ADD(mm.end_time ,INTERVAL 8 HOUR) as end_time'),
                DB::raw('DATE_ADD(mm.can_sellback_time ,INTERVAL 8 HOUR) as can_sellback_time'),
                'example.invalid as mm_info'
            )
            ->leftJoin(DB::raw('(' . $subJoin->toSql() . ') as m'), function ($join) {
                $join->on('m.id', 'mm.miner_id');
            })
            ->mergeBindings($subJoin)
            ->where('mm.member_id', $member_id)
            ->where('mm.id', $mm_id)
            ->where('mm.end_time', '>', Carbon::now());
        return collect($model->first())->toArray() ?? [];
    }

    /**
     * 獲取我的礦機彙整
     *
     * @param array $params 條件
     * @return array|Collection
     */
    public function getMyMinerSummary($params, $member_id)
    {
        //礦機餘額子查詢
        $subDepreciationModel = DB::table('member_miner_depreciations')
            ->groupBy('member_miner_id')
            ->select(
                'member_miner_id',
                DB::raw('max(id) as max_id')
            );
        $depreciationModel = DB::table('member_miner_depreciations as d')
            ->join(
                DB::raw('(' . $subDepreciationModel->toRawSql() . ') as ds'),
                'ds.max_id',
                'd.id'
            )
            ->select(
                'd.member_miner_id',
                'd.residual_value'
            );
        $subModel = DB::table('member_miners as mm')
            ->join('member_miner_transaction_logs as mmtl', 'mm.id', 'mmtl.member_miner_id')
            ->join('miners as miner', function ($join) {
                $join->on('mm.miner_id', 'miner.id')
                    ->whereIn('mm.type', [1, 2]);
            })
            ->leftJoin(
                DB::raw('(' . $depreciationModel->toRawSql() . ') as mmd'),
                'mmd.member_miner_id',
                'mm.id'
            )
            ->where('mm.member_id', $member_id)
            ->whereNotNull('mm.start_time')
            ->whereIn('mm.status', [1, 3])
            ->groupBy('mm.miner_id', 'mmtl.transaction_log_id')
            ->select(
                'mmtl.transaction_log_id',
                'mm.miner_id',
                'miner.name',
                DB::raw('count(*) as cnt'),
                DB::raw('SUM(IFNULL(mmd.residual_value, case when mm.type = 1 then miner.purchase_price else miner.group_price end)) AS residual_value')
            );
        //主查詢
        $model = DB::table(DB::raw('(' . $subModel->toSql() . ') as z'))
            ->mergeBindings($subModel)
            ->join('transaction_logs as tl', function ($join) use ($params) {
                $join->on('tl.id', 'z.transaction_log_id');
            })
            ->whereIn('tl.transaction_type_id', [3, 4, 6])
            ->groupBy('z.miner_id')
            ->select(
                'z.miner_id',
                'z.name',
                DB::raw('0 + CAST(z.residual_value AS CHAR) as residual_value'),
                DB::raw('SUM(case when tl.transaction_type_id in (3,4) then z.cnt else 0 end) as total_count'),
                DB::raw('SUM(case when tl.transaction_type_id = 6 then z.cnt else 0 end) as total_sellback_count'),
                DB::raw('SUM(case when tl.transaction_type_id in (3,4) then tl.point else 0 end) as total_price'),
                DB::raw('SUM(case when tl.transaction_type_id = 6 then tl.point else 0 end) as total_sellback_price')
            );
        return $model->get();
    }

    /**
     * 獲取待拼單礦機清單
     *
     * @param array $params 條件
     * @return array|Collection
     */
    public function getWaitingList(object $params)
    {
        $start_time = date('Y-m-d H:i:00', strtotime($params->start_time . '-8hour'));
        $end_time = date('Y-m-d H:i:59', strtotime($params->end_time . '-8hour'));

        return DB::table('member_miners as mm')
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
                    ->when(Common::IsNotNullOrEmptyString($params->member_type), function ($query) use ($params) {
                        $query->where('m.type', $params->member_type);
                    });
            })
            ->join('agents as a', function ($join) use ($params) {
                $join->on('a.id', 'm.agent_id')
                    ->where('a.hierarchy_path', 'like', $params->HIERARCHY_PATH . '%');
            })
            ->leftJoin('member_miners as omm', function ($join) {
                $join->on('omm.parent_id', 'mm.id')
                    ->where('omm.type', 2);
            })
            ->whereBetween('mm.created_at', [$start_time, $end_time])
            ->where('mm.type', 2)
            ->where('mm.status', 0)
            ->whereNull('mm.parent_id')
            ->whereNull('omm.id')
            ->select(
                'mm.id as mm_id',
                'mm.type',
                'mm.created_at',
                'miner.name',
                'm.username',
                'm.type as member_type'
            );
    }

    public function getWaiting($id)
    {
        return DB::table('member_miners AS mm')
            ->join('members AS member', 'member.id', 'mm.member_id')
            ->join('miners AS miner', 'miner.id', 'mm.miner_id')
            ->where('mm.id', $id)
            ->select(
                'mm.id',
                'mm.miner_id',
                'miner.name',
                'mm.member_id',
                'member.username',
                'member.type as member_type',
                'mm.created_at'
            )
            ->first();
    }

    /**
     * 獲取購買清單
     *
     * @param object $params 條件
     */
    public function getPurchaseList(object $params)
    {
        $create_time_start = '';
        $create_time_end = '';
        if (!empty($params->create_time_start ?? '') && !empty($params->create_time_end ?? '')) {
            $createStartBackend = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::parse($params->create_time_start)->format('Y-m-d H:i:00'), TimeEnum::TimeZone['backend']);
            $createEndBackend = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::parse($params->create_time_end)->format('Y-m-d H:i:59'), TimeEnum::TimeZone['backend']);
            $create_time_start = Carbon::parse($createStartBackend)->setTimezone(TimeEnum::TimeZone['db'])->format('Y-m-d H:i:s');
            $create_time_end = Carbon::parse($createEndBackend)->setTimezone(TimeEnum::TimeZone['db'])->format('Y-m-d H:i:s');
        }
        $check_time_start = '';
        $check_time_end = '';
        if (!empty($params->check_time_start ?? '') && !empty($params->check_time_end ?? '')) {
            $checkStartBackend = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::parse($params->check_time_start)->format('Y-m-d H:i:00'), TimeEnum::TimeZone['backend']);
            $checkEndBackend = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::parse($params->check_time_end)->format('Y-m-d H:i:59'), TimeEnum::TimeZone['backend']);
            $check_time_start = Carbon::parse($checkStartBackend)->setTimezone(TimeEnum::TimeZone['db'])->format('Y-m-d H:i:s');
            $check_time_end = Carbon::parse($checkEndBackend)->setTimezone(TimeEnum::TimeZone['db'])->format('Y-m-d H:i:s');
        }

        //沒有任何時間區間預設抓今天創建的
        if (empty($create_time_start) && empty($create_time_end) && empty($check_time_start) && empty($check_time_end)) {
            $todayStartMy = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::now()->startOfDay(), TimeEnum::TimeZone['backend']);
            $todayEndMy = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::now()->endOfDay(), TimeEnum::TimeZone['backend']);
            $create_time_start = Carbon::parse($todayStartMy)->setTimezone(TimeEnum::TimeZone['db'])->format('Y-m-d H:i:s');
            $create_time_end = Carbon::parse($todayEndMy)->setTimezone(TimeEnum::TimeZone['db'])->format('Y-m-d H:i:s');
        }

        return DB::table('member_miners as mm')
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
                    });
            })
            ->join('agents as a', function ($join) use ($params) {
                $join->on('a.id', 'm.agent_id')
                    ->where('a.hierarchy_path', 'like', $params->HIERARCHY_PATH . '%');
            })
            ->leftjoin('members as pm', function ($join) use ($params) {
                $join->on('pm.invitation_code', 'm.referral_code')
                    ->when(Common::IsNotNullOrEmptyString($params->parent_username), function ($query) use ($params) {
                        $query->where('pm.username', $params->parent_username);
                    });
            })
            ->when(isset($params->status), function ($query) use ($params) {
                $query->where('mm.status', $params->status);
            })
            ->when(Common::IsNotNullOrEmptyString($create_time_start) && Common::IsNotNullOrEmptyString($create_time_end), function ($query) use ($create_time_start, $create_time_end) {
                $query->whereBetween('mm.created_at', [$create_time_start, $create_time_end]);
            })
            ->when(Common::IsNotNullOrEmptyString($check_time_start) && Common::IsNotNullOrEmptyString($check_time_end), function ($query) use ($check_time_start, $check_time_end) {
                $query->whereBetween('mm.check_time', [$check_time_start, $check_time_end]);
            })
            ->where('mm.type', MinerEnum::MinerType['purchase'])
            ->orderBy('mm.created_at', 'desc')
            ->when(Common::IsNotNullOrEmptyString($params->parent_username), function ($query) use ($params) {
                $query->whereNotNull('pm.username');
            })
            ->select(
                'mm.id as mm_id',
                'm.id as member_id',
                'm.username',
                'miner.name',
                'miner.purchase_price',
                'pm.id as parent_id',
                'pm.username as parent_username',
                'mm.status as status',
                'mm.created_at',
                'mm.check_time'
            );
    }

    /**
     * 獲取租賃清單
     *
     * @param object $params 條件
     */
    public function getRentList(object $params)
    {
        $c_start_time = '';
        if (!empty($params->c_start_time ?? '')) {
            $c_start_time = date('Y-m-d H:i:00', strtotime($params->c_start_time . '-8hour'));
        }
        $c_end_time = '';
        if (!empty($params->c_end_time ?? '')) {
            $c_end_time = date('Y-m-d H:i:59', strtotime($params->c_end_time . '-8hour'));
        }
        $s_start_time = '';
        if (!empty($params->s_start_time ?? '')) {
            $s_start_time = date('Y-m-d H:i:00', strtotime($params->s_start_time . '-8hour'));
        }
        $s_end_time = '';
        if (!empty($params->s_end_time ?? '')) {
            $s_end_time = date('Y-m-d H:i:59', strtotime($params->s_end_time . '-8hour'));
        }
        $e_start_time = '';
        if (!empty($params->e_start_time ?? '')) {
            $e_start_time = date('Y-m-d H:i:00', strtotime($params->e_start_time . '-8hour'));
        }
        $e_end_time = '';
        if (!empty($params->e_end_time ?? '')) {
            $e_end_time = date('Y-m-d H:i:59', strtotime($params->e_end_time . '-8hour'));
        }
        $check_time_start = '';
        $check_time_end = '';
        if (!empty($params->check_time_start ?? '') && !empty($params->check_time_end ?? '')) {
            $checkStartBackend = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::parse($params->check_time_start)->format('Y-m-d H:i:00'), TimeEnum::TimeZone['backend']);
            $checkEndBackend = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::parse($params->check_time_end)->format('Y-m-d H:i:59'), TimeEnum::TimeZone['backend']);
            $check_time_start = Carbon::parse($checkStartBackend)->setTimezone(TimeEnum::TimeZone['db'])->format('Y-m-d H:i:s');
            $check_time_end = Carbon::parse($checkEndBackend)->setTimezone(TimeEnum::TimeZone['db'])->format('Y-m-d H:i:s');
        }
        //沒有任何時間區間預設抓今天創建的
        if (empty($c_start_time) && empty($c_end_time) && empty($s_start_time) && empty($s_end_time) && empty($e_start_time) && empty($e_end_time) && empty($check_time_start) && empty($check_time_end)) {
            $todayStartMy = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::parse($params->start_date)->startOfDay(), TimeEnum::TimeZone['backend']);
            $todayEndMy = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::parse($params->end_date)->endOfDay(), TimeEnum::TimeZone['backend']);
            $c_start_time = Carbon::parse($todayStartMy)->setTimezone(TimeEnum::TimeZone['db'])->format('Y-m-d H:i:s');
            $c_end_time = Carbon::parse($todayEndMy)->setTimezone(TimeEnum::TimeZone['db'])->format('Y-m-d H:i:s');
        }

        return DB::table('member_miners as mm')
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
                    });
            })
            ->join('agents as a', function ($join) use ($params) {
                $join->on('a.id', 'm.agent_id')
                    ->where('a.hierarchy_path', 'like', $params->HIERARCHY_PATH . '%');
            })
            ->leftjoin('members as pm', function ($join) use ($params) {
                $join->on('pm.invitation_code', 'm.referral_code')
                    ->when(Common::IsNotNullOrEmptyString($params->parent_username), function ($query) use ($params) {
                        $query->where('pm.username', $params->parent_username);
                    });
            })
            ->leftJoin('agents as o', 'o.id', 'mm.operator')
            ->when((0 == ($params->isExperience ?? -1)), function ($query) {
                $query->where('mm.operator', 0);
            })
            ->when((1 == ($params->isExperience ?? -1)), function ($query) {
                $query->where('mm.operator', '>', 0);
            })
            ->when(isset($params->status), function ($query) use ($params) {
                $query->where('mm.status', $params->status);
            })
            ->when(Common::IsNotNullOrEmptyString($c_start_time) && Common::IsNotNullOrEmptyString($c_end_time), function ($query) use ($c_start_time, $c_end_time) {
                $query->whereBetween('mm.created_at', [$c_start_time, $c_end_time]);
            })
            ->when(Common::IsNotNullOrEmptyString($s_start_time) && Common::IsNotNullOrEmptyString($s_end_time), function ($query) use ($s_start_time, $s_end_time) {
                $query->whereBetween('mm.start_time', [$s_start_time, $s_end_time]);
            })
            ->when(Common::IsNotNullOrEmptyString($e_start_time) && Common::IsNotNullOrEmptyString($e_end_time), function ($query) use ($e_start_time, $e_end_time) {
                $query->whereBetween('mm.end_time', [$e_start_time, $e_end_time]);
            })
            ->when(Common::IsNotNullOrEmptyString($check_time_start) && Common::IsNotNullOrEmptyString($check_time_end), function ($query) use ($check_time_start, $check_time_end) {
                $query->whereBetween('mm.check_time', [$check_time_start, $check_time_end]);
            })
            ->where('mm.type', MinerEnum::MinerType['rent'])
            ->orderBy('mm.created_at', 'desc')
            ->when(Common::IsNotNullOrEmptyString($params->parent_username), function ($query) use ($params) {
                $query->whereNotNull('pm.username');
            })
            ->when((1 == ($params->isExperience ?? -1)), function ($query) {
                $query->whereNotNull('o.username');
            })
            ->when(Common::IsNotNullOrEmptyString($params->operator), function ($query) use ($params) {
                $query->where('o.username', $params->operator);
            })
            ->select(
                'mm.id as mm_id',
                'm.id as member_id',
                'm.username',
                'miner.name',
                'miner.rent_price',
                'pm.id as parent_id',
                'pm.username as parent_username',
                'mm.status as status',
                'mm.start_time',
                'mm.end_time',
                DB::raw('example.invalid->"$.cycle_days" as cycle_days'),
                DB::raw('case when mm.operator>0 then 1 else 0 end as isExperience'),
                DB::raw("IFNULL(o.username, '') as operator"),
                'mm.created_at',
                'mm.check_time',
            );
    }

    /**
     * 獲取已匹配團購清單
     *
     * @param object $params 條件
     */
    public function getGroupList(object $params)
    {
        $create_time_start = '';
        $create_time_end = '';
        if (!empty($params->create_time_start ?? '') && !empty($params->create_time_end ?? '')) {
            $createStartBackend = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::parse($params->create_time_start)->format('Y-m-d H:i:00'), TimeEnum::TimeZone['backend']);
            $createEndBackend = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::parse($params->create_time_end)->format('Y-m-d H:i:59'), TimeEnum::TimeZone['backend']);
            $create_time_start = Carbon::parse($createStartBackend)->setTimezone(TimeEnum::TimeZone['db'])->format('Y-m-d H:i:s');
            $create_time_end = Carbon::parse($createEndBackend)->setTimezone(TimeEnum::TimeZone['db'])->format('Y-m-d H:i:s');
        }
        $check_time_start = '';
        $check_time_end = '';
        if (!empty($params->check_time_start ?? '') && !empty($params->check_time_end ?? '')) {
            $checkStartBackend = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::parse($params->check_time_start)->format('Y-m-d H:i:00'), TimeEnum::TimeZone['backend']);
            $checkEndBackend = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::parse($params->check_time_end)->format('Y-m-d H:i:59'), TimeEnum::TimeZone['backend']);
            $check_time_start = Carbon::parse($checkStartBackend)->setTimezone(TimeEnum::TimeZone['db'])->format('Y-m-d H:i:s');
            $check_time_end = Carbon::parse($checkEndBackend)->setTimezone(TimeEnum::TimeZone['db'])->format('Y-m-d H:i:s');
        }

        //沒有任何時間區間預設抓今天創建的
        if (empty($create_time_start) && empty($create_time_end) && empty($check_time_start) && empty($check_time_end)) {
            $todayStartMy = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::now()->startOfDay(), TimeEnum::TimeZone['backend']);
            $todayEndMy = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::now()->endOfDay(), TimeEnum::TimeZone['backend']);
            $create_time_start = Carbon::parse($todayStartMy)->setTimezone(TimeEnum::TimeZone['db'])->format('Y-m-d H:i:s');
            $create_time_end = Carbon::parse($todayEndMy)->setTimezone(TimeEnum::TimeZone['db'])->format('Y-m-d H:i:s');
        }

        return DB::table('member_miners as mm')
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
                    });
            })
            ->join('agents as a', function ($join) use ($params) {
                $join->on('a.id', 'm.agent_id')
                    ->where('a.hierarchy_path', 'like', $params->HIERARCHY_PATH . '%');
            })
            ->leftjoin('members as pm', function ($join) use ($params) {
                $join->on('pm.invitation_code', 'm.referral_code')
                    ->when(Common::IsNotNullOrEmptyString($params->parent_username), function ($query) use ($params) {
                        $query->where('pm.username', $params->parent_username);
                    });
            })
            ->when(isset($params->status), function ($query) use ($params) {
                $query->where('mm.status', $params->status);
            })
            ->when(Common::IsNotNullOrEmptyString($create_time_start) && Common::IsNotNullOrEmptyString($create_time_end), function ($query) use ($create_time_start, $create_time_end) {
                $query->whereBetween('mm.created_at', [$create_time_start, $create_time_end]);
            })
            ->when(Common::IsNotNullOrEmptyString($check_time_start) && Common::IsNotNullOrEmptyString($check_time_end), function ($query) use ($check_time_start, $check_time_end) {
                $query->whereBetween('mm.check_time', [$check_time_start, $check_time_end]);
            })
            ->where('mm.type', MinerEnum::MinerType['group'])
            ->orderBy('mm.created_at', 'desc')
            ->when(Common::IsNotNullOrEmptyString($params->parent_username), function ($query) use ($params) {
                $query->whereNotNull('pm.username');
            })
            ->whereNot('mm.status', 0)
            ->select(
                'mm.id as mm_id',
                'mm.parent_id',
                'm.id as member_id',
                'm.username',
                'miner.name',
                'miner.group_price',
                'pm.username as parent_username',
                'mm.status as status',
                'mm.created_at',
                'mm.check_time'
            );
    }
}
