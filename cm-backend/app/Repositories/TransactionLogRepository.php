<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Helpers\Common;
use App\Models\TransactionLog;
use DB;

/**
 * Class TransactionLogRepository.
 */
class TransactionLogRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return TransactionLog::class;
    }

    public function updateDataByMemberId($id, $member_id, $data)
    {
        return $this->model->where('id', $id)
            ->where('member_id', $member_id)
            ->update($data);
    }

    public function getTotalPointByType($member_id, $type_id)
    {
        return $this->model->where('member_id', $member_id)
            ->where('transaction_type_id', $type_id)
            ->select(
                DB::raw('IFNULL(SUM(point), 0) as point')
            )
            ->first();
    }

    public function getManualList($params)
    {
        $start_time = date('Y-m-d H:i:00', strtotime($params->start_time . '-8hour'));
        $end_time = date('Y-m-d H:i:59', strtotime($params->end_time . '-8hour'));

        //UID、姓名、操作者、開始時間、結束時間、
        return DB::table('transaction_logs as tl')
            ->join('members as m', function ($join) use ($params) {
                $join->on('m.id', 'tl.member_id')
                    ->when(Common::IsNotNullOrEmptyString($params->uid), function ($query) use ($params) {
                        $query->where('m.uid', $params->uid);
                    })
                    ->when(Common::IsNotNullOrEmptyString($params->username), function ($query) use ($params) {
                        $query->where('m.username', $params->username);
                    });
            })
            ->join('agents as a', function ($join) use ($params) {
                $join->on('a.id', 'm.agent_id')
                    ->when(Common::IsNotNullOrEmptyString($params->HIERARCHY_PATH), function ($query) use ($params) {
                        $query->where('a.hierarchy_path', 'like', $params->HIERARCHY_PATH . '%');
                    });
            })
            ->leftJoin('agents as op', 'op.id', 'tl.operator')
            ->whereBetween('tl.created_at', [$start_time, $end_time])
            ->when(Common::IsNotNullOrEmptyString($params->transaction_type), function ($query) use ($params) {
                $query->where('tl.transaction_type_id', $params->transaction_type);
            })
            ->when(!Common::IsNotNullOrEmptyString($params->transaction_type), function ($query) use ($params) {
                $query->whereIn('tl.transaction_type_id', [12, 13, 16, 17]);
            })
            ->when(Common::IsNotNullOrEmptyString($params->operator), function ($query) use ($params) {
                $query->where('op.username', $params->operator);
            })
            ->orderBy('tl.created_at', 'desc')
            ->orderBy('tl.id', 'desc')
            ->select(
                'tl.id as transaction_log_id',
                'tl.transaction_type_id as type',
                'tl.point',
                'tl.before_balance',
                'tl.after_balance',
                DB::raw("IFNULL(op.username, '') as operator"),
                'tl.status',
                'tl.created_at',
                'm.uid',
                'm.username',
                'tl.memo',
            );
    }

    public function getList(object $params)
    {
        return DB::table('transaction_logs as tl')
            ->join('members as m', function ($join) use ($params) {
                $join->on('m.id', 'tl.member_id')
                    ->when(Common::IsNotNullOrEmptyString($params->member_id), function ($query) use ($params) {
                        $query->where('m.id', $params->member_id);
                    });
            })
            ->join('transaction_types as tt','tt.id', 'tl.transaction_type_id')
            ->when(isset($params->type), function ($query) use ($params) {
                $query->where('tl.transaction_type_id', $params->type);
            })
            ->where('tl.status', 1)
            ->orderByRaw('tl.created_at desc, tl.id desc')
            ->select(
                'tl.id as id',
                'tl.transaction_type_id',
                'tl.point',
                'tl.before_balance',
                'tl.after_balance',
                'tl.created_at',
            );
    }
}
