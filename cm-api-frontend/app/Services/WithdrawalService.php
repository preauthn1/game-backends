<?php

namespace App\Services;

use App\Helpers\Common;
use App\Repositories\WithdrawalOrderRepository;
use App\Repositories\BankRepository;
use Exception;

class WithdrawalService extends BaseService
{
    public function __construct()
    {
        $this->repository = new WithdrawalOrderRepository();
    }

    public function find($id)
    {
        $order =  $this->repository->find($id);
        if (!$order) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $order;
    }

    public function createOrder($member_id, $order_no, $bank_id, $rate, $fee_type, $fee, $point, $ip, $cardno, $cardholder)
    {
        $finalFee = (1 == $fee_type) ? number_format($fee, 2) : number_format(bcmul($point, $fee, 5), 2);

        $data = array();
        $data['order_no'] = $order_no;
        $data['bank_id'] = $bank_id;
        $data['member_id'] = $member_id;
        $data['point'] = $point;
        $data['fee'] = $finalFee;
        $data['rate'] = $rate;
        $data['amount'] = bcmul(bcsub($point, $finalFee, 10), $rate, 10);
        $data['cardholder'] = $cardholder;
        $data['cardno'] = $cardno;
        $data['ip'] = $ip;

        $result = $this->repository->create($data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function getOrder($member_id, $order_id)
    {
        $order = $this->repository->getOrder($member_id, $order_id);
        if (!$order) {
            throw new Exception(__('error_message.8001'), 8001);
        }
        return $order;
    }

    public function updateData($id, array $data)
    {
        $result = $this->repository->updateData($id, $data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function updateOrderStatus($id, $status)
    {
        $data = array();
        $data['status'] = $status;
        return $this->updateData($id, $data);
    }

    public function getOrders($member_id, $days, $page, $per_page)
    {
        $today = date('Y-m-d', strtotime('+8hour'));
        $start_date = date('Y-m-d', strtotime($today . '-' . $days . 'day'));
        $start_time = date('Y-m-d H:i:s', strtotime($start_date . '-8hour'));

        $model = $this->repository->getOrders($member_id, $start_time);
        $total = $model->count();
        $result = $model->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();
        return [$total, $result];
    }

}
