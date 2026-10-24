<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
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

    public function getOrders($params)
    {
        $today = date('Y-m-d', strtotime('+8hour'));
        $start_date = date('Y-m-d', strtotime($today . '-' . $params->days . 'day'));
        $start_time = date('Y-m-d H:i:s', strtotime($start_date . '-8hour'));

        //$member_id, $start_time, $end_time
        return DB::table('deposit_orders AS do')
            ->join('banks AS b', 'b.id', 'do.bank_id')
            ->join('currencies AS c', 'c.id', 'b.currency_id')
            ->leftJoin('transaction_logs as tl', function ($join) {
                $join->on('tl.order_id', 'do.id')
                    ->where('transaction_type_id', 1);
            })
            ->where('do.member_id', $params->ID)
            ->where('do.created_at', '>=', $start_time)
            ->orderBy('do.created_at', 'desc')
            ->select(
                'do.id',
                'do.order_no',
                'do.bank_id',
                'do.member_id',
                'do.amount',
                'do.rate as exchange_rate',
                'do.point',
                'do.cardholder',
                'do.image',
                DB::raw('INET_NTOA(do.ip) as ip'),
                'do.status',
                'do.extra_content',
                'do.remark',
                DB::raw('DATE_ADD(do.pay_time, INTERVAL 8 HOUR) AS pay_time'),
                'do.operator',
                DB::raw('DATE_ADD(do.created_at, INTERVAL 8 HOUR) AS created_at'),
                DB::raw('DATE_ADD(do.updated_at, INTERVAL 8 HOUR) AS updated_at'),
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

    public function updateImageAndStatus($id, $image_url, $transaction_no)
    {
        return $this->model->lockForUpdate()
            ->where('id', $id)
            ->where('status', 0)
            ->update([
                'image' => $image_url,
                'transaction_no' => $transaction_no,
                'status' => 3
            ]);
    }
}
