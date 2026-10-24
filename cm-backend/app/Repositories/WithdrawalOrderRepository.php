<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Helpers\Common;
use App\Models\WithdrawalOrder;
use DB;

/**
 * Class WithdrawalOrderRepository.
 */
class WithdrawalOrderRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return WithdrawalOrder::class;
    }

    //資料採集
    public function getDataCollection($params, $start_time)
    {
        return DB::table('withdrawal_orders as wo')
            ->join('members as m', function ($join) {
                $join->on('m.id', 'wo.member_id')
                    ->where('m.type', 1);
            })
            ->join('agents as a', function ($join) use ($params) {
                $join->on('a.id', 'm.agent_id')
                    ->where('a.hierarchy_path', 'like', $params->HIERARCHY_PATH . '%');
            })
            ->where('wo.pay_time', '>=', $start_time)
            ->where('wo.status', 1)
            ->groupBy(DB::raw('DATE(DATE_ADD(wo.pay_time, INTERVAL 8 HOUR))'))
            ->select(
                DB::raw('DATE(DATE_ADD(wo.pay_time, INTERVAL 8 HOUR)) AS date'),
                DB::raw('COUNT(DISTINCT wo.member_id) AS count'),
                DB::raw('SUM(wo.point) AS amount')
            )
            ->get();
    }

    public function getOrders($params)
    {
        $start_time = !empty($params->start_time??'')?date('Y-m-d H:i:00', strtotime($params->start_time . '-8hour')):'';
        $end_time = !empty($params->end_time??'')?date('Y-m-d H:i:59', strtotime($params->end_time . '-8hour')):'';
        $pay_time_start = !empty($params->pay_time_start??'')?date('Y-m-d H:i:00', strtotime($params->pay_time_start . '-8hour')):'';
        $pay_time_end = !empty($params->pay_time_end??'')?date('Y-m-d H:i:59', strtotime($params->pay_time_end . '-8hour')):'';

        return DB::table('withdrawal_orders AS wo')
            ->join('members AS m', function ($join) use ($params) {
                $join->on('m.id', 'wo.member_id')
                    ->when(Common::IsNotNullOrEmptyString($params->username), function ($query) use ($params) {
                        $query->where('m.username', $params->username);
                    });
            })
            ->join('agents as a', function ($join) use ($params) {
                $join->on('a.id', 'm.agent_id')
                    ->where('a.hierarchy_path', 'like', $params->HIERARCHY_PATH . '%');
            })
            ->join('banks AS b', function ($join) use ($params) {
                $join->on('b.id', 'wo.bank_id')
                    ->when(Common::IsNotNullOrEmptyString($params->gateway_id), function ($query) use ($params) {
                        if ($params->gateway_id == 1001) {
                            $query->where('b.type', 11);
                        } elseif ($params->gateway_id == 1002) {
                            $query->where('b.type', 13);
                        }
                    });
            })
            ->join('currencies AS c', function ($join) use ($params) {
                $join->on('c.id', 'b.currency_id')
                    ->when(Common::IsNotNullOrEmptyString($params->currency_id), function ($query) use ($params) {
                        $query->where('c.id', $params->currency_id);
                    });
            })
            ->when(Common::IsNotNullOrEmptyString($start_time) && Common::IsNotNullOrEmptyString($end_time), function ($query) use ($start_time, $end_time) {
                $query->whereBetween('wo.created_at', [$start_time, $end_time]);
            })
            ->when(Common::IsNotNullOrEmptyString($params->order_id), function ($query) use ($params) {
                $query->where('wo.id', $params->order_id);
            })
            ->when(Common::IsNotNullOrEmptyString($params->order_no), function ($query) use ($params) {
                $query->where('wo.order_no', $params->order_no);
            })
            ->when(Common::IsNotNullOrEmptyString($params->status), function ($query) use ($params) {
                $query->where('wo.status', $params->status);
            })
            ->when(Common::IsNotNullOrEmptyString($params->amount_min), function ($query) use ($params) {
                $query->where('wo.point', '>=', $params->amount_min);
            })
            ->when(Common::IsNotNullOrEmptyString($params->amount_max), function ($query) use ($params) {
                $query->where('wo.point', '<=', $params->amount_max);
            })
            ->when(Common::IsNotNullOrEmptyString($params->cardholder), function ($query) use ($params) {
                $query->where('wo.cardholder', $params->cardholder);
            })
            ->when(Common::IsNotNullOrEmptyString($params->cardno), function ($query) use ($params) {
                $query->where('wo.cardno', $params->cardno);
            })
            ->when(Common::IsNotNullOrEmptyString($pay_time_start) && Common::IsNotNullOrEmptyString($pay_time_end), function ($query) use ($pay_time_start, $pay_time_end) {
                $query->whereBetween('wo.pay_time', [$pay_time_start, $pay_time_end]);
            })
            ->orderBy('wo.created_at', 'desc')
            ->select(
                'wo.id',
                'wo.order_no',
                'wo.bank_id',
                'wo.member_id',
                'wo.point',
                'wo.rate as exchange_rate',
                'wo.fee as fee',
                'wo.amount',
                'wo.cardholder',
                'wo.cardno',
                DB::raw('INET_NTOA(wo.ip) as ip'),
                'wo.extra_content',
                'wo.remark',
                'wo.status',
                DB::raw('DATE_ADD(wo.pay_time, INTERVAL 8 HOUR) AS pay_time'),
                'wo.operator',
                DB::raw('DATE_ADD(wo.created_at, INTERVAL 8 HOUR) AS created_at'),
                DB::raw('DATE_ADD(wo.updated_at, INTERVAL 8 HOUR) AS updated_at'),
                DB::raw("IFNULL(wo.pay_time, '') as pay_time"),
                'b.name as bank_name',
                'b.type as bank_type',
                'c.id as currency_id',
                'c.name as currency_name',
                'm.username',
                'm.type as member_type',
                'm.memo'
            );
    }

    public function getOrder($id)
    {
        return DB::table('withdrawal_orders AS wo')
            ->join('members AS m', 'm.id', 'wo.member_id')
            ->join('banks AS b', 'b.id', 'wo.bank_id')
            ->join('currencies AS c', 'c.id', 'b.currency_id')
            ->leftJoin('agents AS a', 'a.id', 'wo.operator')
            ->where('wo.id', $id)
            ->select(
                'wo.id',
                'wo.order_no',
                'wo.bank_id',
                'wo.point',  //平台點數
                'wo.rate as exchange_rate', //當下匯率
                'wo.fee as fee', //手續費
                'wo.amount',  //法幣/USDT充值金額
                'wo.cardholder',  //持卡人
                'wo.cardno',  //卡號
                DB::raw('INET_NTOA(wo.ip) as ip'),
                'wo.status',
                'wo.extra_content',
                'wo.remark',
                DB::raw('DATE_ADD(wo.pay_time, INTERVAL 8 HOUR) AS pay_time'),
                'a.username as operator',
                DB::raw('DATE_ADD(wo.created_at, INTERVAL 8 HOUR) AS created_at'),
                DB::raw('DATE_ADD(wo.updated_at, INTERVAL 8 HOUR) AS updated_at'),
                DB::raw("IFNULL(wo.pay_time, '') as pay_time"),
                'b.type as bank_type',
                'c.id as currency_id',
                'c.name as currency_name',
                'm.id as member_id',
                'm.uid as member_uid',
                'm.username',
                'm.type as member_type'
            )
            ->first();
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
            ->when($status == 1, function ($query) {
                $query->where('status', 3);
            })
            ->when($status == 3, function ($query) {
                $query->where('status', 0);
            })
            ->update($data);
    }

    public function getOrderCount($params, $field, $startTime, $endTime)
    {
        return DB::table('withdrawal_orders as wo')
            ->join('members as m', function ($join) {
                $join->on('m.id', 'wo.member_id')
                    ->where('m.type', 1);
            })
            ->join('agents as a', function ($join) use ($params) {
                $join->on('a.id', 'm.agent_id')
                    ->where('a.hierarchy_path', 'like', $params->HIERARCHY_PATH . '%');
            })
            ->when(!empty($startTime), function ($query) use ($startTime) {
                $query->where('wo.pay_time', '>=', $startTime);
            })
            ->when(!empty($endTime), function ($query) use ($endTime) {
                $query->where('wo.pay_time', '<=', $endTime);
            })
            ->where('wo.status', 1)
            ->select(
                DB::raw('count(' . $field . ') AS count')
            )
            ->get()
            ->toArray();
    }

    public function getOrderSum($params, $field, $startTime, $endTime)
    {
        return DB::table('withdrawal_orders as wo')
            ->join('members as m', function ($join) {
                $join->on('m.id', 'wo.member_id')
                    ->where('m.type', 1);
            })
            ->join('agents as a', function ($join) use ($params) {
                $join->on('a.id', 'm.agent_id')
                    ->where('a.hierarchy_path', 'like', $params->HIERARCHY_PATH . '%');
            })
            ->when(!empty($startTime), function ($query) use ($startTime) {
                $query->where('wo.pay_time', '>=', $startTime);
            })
            ->when(!empty($endTime), function ($query) use ($endTime) {
                $query->where('wo.pay_time', '<=', $endTime);
            })
            ->where('wo.status', 1)
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
