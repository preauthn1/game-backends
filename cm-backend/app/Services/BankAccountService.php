<?php

namespace App\Services;

use App\Repositories\BankAccountRepository;
use App\Repositories\BankLogRepository;
use Exception;

class BankAccountService extends BaseService
{
    public function __construct()
    {
        $this->repository = new BankAccountRepository();
    }

    public function find($id)
    {
        return $this->repository->getById($id);
    }

    public function writeBankLog($currency_id, $bank_id, $before, $after, $agent_id, $remark = null)
    {
        $repository = new BankLogRepository();
        $data = array();
        $data['agent_id'] = $agent_id;
        $data['currency_id'] = $currency_id;
        $data['bank_id'] = $bank_id;
        $data['before_info'] = json_encode($before);
        $data['after_info'] = json_encode($after);
        $data['remark'] = $remark;
        $result = $repository->create($data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function getList($params)
    {
        $page = $params->page ?? 1;
        $per_page = $params->per_page ?? 10;

        $model = $this->repository->getList($params);
        $total = $model->count();
        $result = $model->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();
        $result = $result->map(function ($item) {
            if ($item->bank_type == 1) {
                $item->gateway_id = 1001;
                $item->gatewayTxt = __('param.deposit_gateway.1001');
            } elseif ($item->bank_type == 3) {
                $item->gateway_id = 1002;
                $item->gatewayTxt = __('param.deposit_gateway.1002');
            }
            return $item;
        });
        return [$total, $result];
    }

    public function updateStatus($params)
    {
        $bank_account_id = $params->bank_account_id;
        $before = $this->find($bank_account_id);

        $data = array();
        $data['status'] = $params->status;

        $result = $this->repository->updateById($bank_account_id, $data);
        if (!$result) {
            throw new Exception(__('error_message.1502'), 1502);
        }
        $after = $this->find($bank_account_id);
        $this->writeBankLog(0, $bank_account_id, $before, $after, $params->ID);
    }

    public function createBankAccount($params)
    {
        $data = array();
        $data['bank_id'] = $params->bank_id;
        if (!empty($params->account_alias ?? '')) {
            $data['account_nickname'] = $params->account_alias;
        }
        $data['account_name'] = $params->account_name;
        $data['account_no'] = $params->account_no;
        $data['status'] = 1;
        $result = $this->repository->create($data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }
}
