<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Helpers\Common;
use App\Enums\DepositEnum;
use App\Models\DepositOrder;
use DB;

/**
 * Class DepositOrderRepository.
 */
class DepositOrderRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return DepositOrder::class;
    }

    public function getOrder($id)
    {
        return DB::table('deposit_orders AS o')
            ->join('banks AS b', 'b.id', 'o.bank_id')
            ->join('bank_accounts AS ba', 'ba.id', 'o.bank_account_id')
            ->join('currencies AS c', 'c.id', 'b.currency_id')
            ->join('members AS m', 'm.id', 'o.member_id')
            ->leftJoin('agents AS a', 'a.id', 'o.operator')
            ->where('o.id', $id)
            ->select(
                'o.id',
                'o.order_no',
                'o.bank_id',
                'o.bank_account_id',
                'ba.account_name as collection_account', //收款帳戶
                'ba.account_no as collection_account_no', //收到帳號
                'o.amount',  //法幣/USDT充值金額
                'o.rate as exchange_rate', //當下匯率
                'o.point',  //平台點數
                'o.cardholder',  //持卡人
                'o.image',
                'o.transaction_no',
                DB::raw('INET_NTOA(o.ip) as ip'),
                'o.status',
                'o.extra_content',
                'o.remark',
                DB::raw("IFNULL(o.pay_time, '') as pay_time"),
                'a.username as operator',
                'o.created_at',
                'o.updated_at',
                'b.type as bank_type',
                'c.id as currency_id',
                'c.name as currency_name',
                'm.id as member_id',
                'm.uid as member_uid',
                'm.username'
            )
            ->first();
    }

    //資料採集
    public function getDataCollection($params, $start_time)
    {
        return DB::table('deposit_orders as do')
            ->join('members as m', function ($join) {
                $join->on('m.id', 'do.member_id')
                    ->where('m.type', 1);
            })
            ->join('agents as a', function ($join) use ($params) {
                $join->on('a.id', 'm.agent_id')
                    ->where('a.hierarchy_path', 'like', $params->HIERARCHY_PATH . '%');
            })
            ->where('do.pay_time', '>=', $start_time)
            ->where('do.status', 1)
            ->groupBy(DB::raw('DATE(DATE_ADD(do.pay_time, INTERVAL 8 HOUR))'))
            ->select(
                DB::raw('DATE(DATE_ADD(do.pay_time, INTERVAL 8 HOUR)) AS date'),
                DB::raw('COUNT(DISTINCT do.member_id) AS count'),
                DB::raw('SUM(do.point) AS amount')
            )
            ->get();
    }

    public function getOrders($params)
    {
        $start_time = !empty($params->start_time??'')?date('Y-m-d H:i:00', strtotime($params->start_time . '-8hour')):'';
        $end_time = !empty($params->end_time??'')?date('Y-m-d H:i:59', strtotime($params->end_time . '-8hour')):'';
        $pay_time_start = !empty($params->pay_time_start??'')?date('Y-m-d H:i:00', strtotime($params->pay_time_start . '-8hour')):'';
        $pay_time_end = !empty($params->pay_time_end??'')?date('Y-m-d H:i:59', strtotime($params->pay_time_end . '-8hour')):'';

        return DB::table('deposit_orders AS o')
            ->join('banks AS b', function ($join) use ($params) {
                $join->on('b.id', 'o.bank_id')
                    ->when(Common::IsNotNullOrEmptyString($params->gateway_id), function ($query) use ($params) {
                        if ($params->gateway_id == 1001) {
                            $query->where('b.type', 1);
                        } elseif ($params->gateway_id == 1002) {
                            $query->where('b.type', 3);
                        }
                    });
            })
            ->join('currencies AS c', 'c.id', 'b.currency_id')
            ->join('members as m', function ($join) use ($params) {
                $join->on('m.id', 'o.member_id')
                    ->when(Common::IsNotNullOrEmptyString($params->username), function ($query) use ($params) {
                        $query->where('m.username', $params->username);
                    });
            })
            ->join('agents as a', function ($join) use ($params) {
                $join->on('a.id', 'm.agent_id')
                    ->where('a.hierarchy_path', 'like', $params->HIERARCHY_PATH . '%');
            })
            ->when(Common::IsNotNullOrEmptyString($start_time) && Common::IsNotNullOrEmptyString($end_time), function ($query) use ($start_time, $end_time) {
                $query->whereBetween('o.created_at', [$start_time, $end_time]);
            })
            ->when(isset($params->order_id), function ($query) use ($params) {
                $query->where('o.id', $params->order_id);
            })
            ->when(isset($params->order_no), function ($query) use ($params) {
                $query->where('o.order_no', $params->order_no);
            })
            ->when(isset($params->status), function ($query) use ($params) {
                $query->where('o.status', $params->status);
            })
            ->when(isset($params->amount_min), function ($query) use ($params) {
                $query->where('o.amount', '>=', $params->amount_min);
            })
            ->when(isset($params->amount_max), function ($query) use ($params) {
                $query->where('o.amount', '<=', $params->amount_max);
            })
            ->when(Common::IsNotNullOrEmptyString($pay_time_start) && Common::IsNotNullOrEmptyString($pay_time_end), function ($query) use ($pay_time_start, $pay_time_end) {
                $query->whereBetween('o.pay_time', [$pay_time_start, $pay_time_end]);
            })
            ->orderBy('o.created_at', 'desc')
            ->select(
                'o.id',
                'o.order_no',
                'o.bank_id',
                'o.member_id',
                'm.username as username',
                'o.amount',
                'o.rate as exchange_rate',
                'o.point',
                'o.cardholder',
                'o.image',
                DB::raw('INET_NTOA(o.ip) as ip'),
                'o.status',
                'o.extra_content',
                'o.remark',
                DB::raw('DATE_ADD(o.pay_time, INTERVAL 8 HOUR) AS pay_time'),
                'o.operator',
                DB::raw('DATE_ADD(o.created_at, INTERVAL 8 HOUR) AS created_at'),
                DB::raw('DATE_ADD(o.updated_at, INTERVAL 8 HOUR) AS updated_at'),
                DB::raw("IFNULL(o.pay_time, '') as pay_time"),
                'b.type as bank_type',
                'c.id as currency_id',
                'c.name as currency_name',
                'm.memo'
            );
    }

    public function updateOrder($operator_id, $id, $status, $remark)
    {
        $data = array();
        $data['status'] = $status;
        $data['remark'] = $remark;
        $data['operator'] = $operator_id;
        if ($status == 1) {
            $data['pay_time'] = date('Y-m-d H:i:s');
        }

        return $this->model
            ->where('id', $id)
            ->update($data);
    }

    public function updateImageAndStatus($id, $image_url)
    {
        return $this->model
            ->where('id', $id)
            ->where('status', 0)
            ->update([
                'image' => $image_url,
                'status' => 3
            ]);
    }

    public function getOrderCount($params, $field, $startTime, $endTime)
    {
        return DB::table('deposit_orders as do')
            ->join('members as m', function ($join) {
                $join->on('m.id', 'do.member_id')
                    ->where('m.type', 1);
            })
            ->join('agents as a', function ($join) use ($params) {
                $join->on('a.id', 'm.agent_id')
                    ->where('a.hierarchy_path', 'like', $params->HIERARCHY_PATH . '%');
            })
            ->when(!empty($startTime), function ($query) use ($startTime) {
                $query->where('do.pay_time', '>=', $startTime);
            })
            ->when(!empty($endTime), function ($query) use ($endTime) {
                $query->where('do.pay_time', '<=', $endTime);
            })
            ->where('do.status', 1)
            ->select(
                DB::raw('count(' . $field . ') AS count')
            )
            ->get()
            ->toArray();
    }

    public function getOrderSum($params, $field, $startTime, $endTime)
    {
        return DB::table('deposit_orders as do')
            ->join('members as m', function ($join) {
                $join->on('m.id', 'do.member_id')
                    ->where('m.type', 1);
            })
            ->join('agents as a', function ($join) use ($params) {
                $join->on('a.id', 'm.agent_id')
                    ->where('a.hierarchy_path', 'like', $params->HIERARCHY_PATH . '%');
            })
            ->when(!empty($startTime), function ($query) use ($startTime) {
                $query->where('do.pay_time', '>=', $startTime);
            })
            ->when(!empty($endTime), function ($query) use ($endTime) {
                $query->where('do.pay_time', '<=', $endTime);
            })
            ->where('do.status', 1)
            ->select(
                DB::raw('sum(' . $field . ') AS sum')
            )
            ->get()
            ->toArray();
    }

    public function getSummaryByMemberIds($member_ids)
    {
        return $this->model
            ->where('status', 1)
            ->whereIn('member_id', $member_ids)
            ->select(
                DB::raw('IFNULL(SUM(point), 0) as point'),
                DB::raw('COUNT(DISTINCT member_id) AS member_count')
            )
            ->first();
    }
}
