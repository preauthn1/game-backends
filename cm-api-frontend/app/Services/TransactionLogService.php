<?php

namespace App\Services;

use App\Repositories\TransactionLogRepository;
use Exception;

class TransactionLogService extends BaseService
{
    public function __construct()
    {
        $this->repository = new TransactionLogRepository();
    }

    public function create($member_id, $wallet_type_id, $transaction_type_id, $order_id, $point, $before_balance, $after_balance, $status = 0)
    {
        $data = array();
        $data['wallet_type_id'] = $wallet_type_id;
        $data['member_id'] = $member_id;
        $data['transaction_type_id'] = $transaction_type_id;
        $data['order_id'] = $order_id;
        $data['point'] = $point;
        $data['before_balance'] = $before_balance;
        $data['after_balance'] = $after_balance;
        if ($status != 0) {
            $data['status'] = $status;
        }

        $result = $this->repository->create($data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function updateDataByMemberId($id, $member_id, $data)
    {
        $result = $this->repository->updateDataByMemberId($id, $member_id, $data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function setSuccess($id, $member_id)
    {
        $data = array();
        $data['status'] = 1;
        $result = $this->updateDataByMemberId($id, $member_id, $data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function setSuccessAndOrderId($id, $member_id, $order_id)
    {
        $data = array();
        $data['status'] = 1;
        $data['order_id'] = $order_id;
        $result = $this->updateDataByMemberId($id, $member_id, $data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }
}