<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
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

    public function updateData($id, $data)
    {
        return $this->model->where('id', $id)->update($data);
    }

    public function getOrders($member_id, $start_time)
    {
        return DB::table('withdrawal_orders AS wo')
            ->join('banks AS b', 'b.id', 'wo.bank_id')
            ->join('currencies AS c', 'c.id', 'b.currency_id')
            ->leftJoin('transaction_logs as tl', function ($join) {
                $join->on('tl.order_id', 'wo.id')
                    ->where('transaction_type_id',2);
            })
            ->where('wo.member_id', $member_id)
            ->where('wo.created_at', '>=', $start_time)
            ->orderBy('wo.created_at', 'desc')
            ->select(
                'wo.id',
                'wo.order_no',
                'wo.bank_id',
                'wo.member_id',
                'wo.amount',
                'wo.rate as exchange_rate',
                'wo.point',
                'wo.cardholder',
                DB::raw('INET_NTOA(wo.ip) as ip'),
                DB::raw(
                    "case wo.status" .
                    " when 0 then '" . __('param.withdrawal_order.status.0') . "'".
                    " when 1 then '" . __('param.withdrawal_order.status.1') . "'".
                    " when 2 then '" . __('param.withdrawal_order.status.2') . "'".
                    " when 3 then '" . __('param.withdrawal_order.status.3') . "'".
                    " when 4 then '" . __('param.withdrawal_order.status.2') . "'".
                    " else '" . __('param.other') . "'".
                    " end as status"
                ),
                'wo.extra_content',
                DB::raw(
                    "case wo.status" .
                    " when 2 then wo.remark".
                    " when 4 then wo.remark".
                    " else ''".
                    " end as remark"
                ),
                DB::raw('DATE_ADD(wo.pay_time, INTERVAL 8 HOUR) AS pay_time'),
                'wo.operator',
                DB::raw('DATE_ADD(wo.created_at, INTERVAL 8 HOUR) AS created_at'),
                DB::raw('DATE_ADD(wo.updated_at, INTERVAL 8 HOUR) AS updated_at'),
                'b.name as bank_name',
                'b.type as bank_type',
                'c.id as currency_id',
                'c.name as currency_name',
                'tl.after_balance'
            );
    }

    public function getOrder($member_id, $order_id)
    {
        return $this->model
            ->where('id', $order_id)
            ->where('member_id', $member_id)
            ->first();
    }
}
