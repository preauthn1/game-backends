<?php

namespace App\Repositories;

use App\Helpers\Common;
use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
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

    public function updateByData($id, $data)
    {
        return $this->model->where('id', $id)
            ->update($data);
    }

    public function updateDataByMemberId($id, $member_id, $data)
    {
        return $this->model->where('id', $id)
            ->where('member_id', $member_id)
            ->update($data);
    }

    public function getFundList($params)
    {
        $today = date('Y-m-d', strtotime('+8hour'));
        $start_date = date('Y-m-d', strtotime($today . '-' . $params->days . 'day'));
        $start_time = date('Y-m-d H:i:s', strtotime($start_date . '-8hour'));

        return DB::table('transaction_logs as tl')
            ->join('transaction_types as tt', 'tt.id', 'tl.transaction_type_id')
            ->leftJoin('transaction_type_names as ttn', function ($join) use ($params) {
                $join->on('ttn.transaction_type_id', 'tt.id')
                    ->where('ttn.language_id', $params->LANGUAGE);
            })
            ->join('wallet_types as wt', 'wt.id', 'tl.wallet_type_id')
            ->leftJoin('wallet_type_names as wtn', function ($join) use ($params) {
                $join->on('wtn.wallet_type_id', 'wt.id')
                    ->where('wtn.language_id', $params->LANGUAGE);
            })
            ->leftJoin('agents as a', 'a.id', 'tl.operator')
            ->when(Common::IsNotNullOrEmptyString($params->type ?? 0), function ($query) use ($params) {
                $query->where('tl.transaction_type_id', $params->type);
            })
            ->where('tl.created_at', '>=', $start_time)
            ->where('tl.member_id', $params->ID)
            ->orderBy('tl.created_at', 'desc')
            ->orderBy('tl.id', 'desc')
            ->select(
                'tl.id',
                'tl.wallet_type_id',
                DB::raw('IFNULL(wtn.name, wt.name) as wallet_type_name'),
                'tl.transaction_type_id',
                'tl.point',
                'tl.before_balance',
                'tl.after_balance',
                'tl.memo as remark',
                DB::raw('DATE(DATE_ADD(tl.created_at, INTERVAL 8 HOUR)) as date'),
                DB::raw('DATE_ADD(tl.created_at, INTERVAL 8 HOUR) as created_at'),
                'a.username as operator',
                'tl.status'
            );

    }
}
