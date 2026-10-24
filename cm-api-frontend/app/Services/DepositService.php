<?php

namespace App\Services;

use App\Enums\TimeEnum;
use App\Helpers\Common;
use App\Repositories\DepositOrderRepository;
use App\Repositories\BankRepository;
use App\Repositories\BankAccountRepository;
use App\Repositories\CurrencyRepository;
use Carbon\Carbon;
use Exception;

class DepositService extends BaseService
{
    public function __construct()
    {
        $this->repository = new DepositOrderRepository();
    }

    public function create($agent_group_id, $member_id, $amount, $bank_id, $cardholder = '')
    {
        $ip = Common::getUserIp();
        $order_no = Common::getOrderNo('D');

        //取得幣種匯率
        $bankRepository = new BankRepository();
        $currency = $bankRepository->getCurrencyByBankId($agent_group_id, $bank_id);
        if (!$currency) {
            throw new Exception(__('error_message.2001'), 2001);
        }

        $this->bankAccountRepository = new BankAccountRepository();
        $params = [
            'agent_group_id' => $agent_group_id,
            'bank_id' => $bank_id,
        ];
        $resultBankAccount = $this->bankAccountRepository->getBankAccounts($params);
        if (count($resultBankAccount) == 0) {
            throw new Exception(__('error_message.5034'), 5034);
        }

        //多個存款帳號 隨機取一個
        $bankAccount = collect($resultBankAccount)->random();
        if (!$bankAccount) {
            throw new Exception(__('error_message.2001'), 2001);
        }

        $data = array();
        $data['order_no'] = $order_no;
        $data['bank_id'] = $bank_id;
        $data['bank_account_id'] = $bankAccount->id;
        $data['member_id'] = $member_id;
        $data['amount'] = $amount;
        $data['rate'] = $currency->deposit_rate; //充值當下還是成功當下??
        $data['point'] = bcmul($amount, $currency->deposit_rate, 10);
        $data['cardholder'] = $cardholder;
        $data['ip'] = ip2long($ip);
        $this->repository = new DepositOrderRepository();
        $order = $this->repository->create($data);
        if (!$order) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $order;
    }

    public function getOrders($params)
    {
        $page = $params->page ?? 1;
        $per_page = $params->per_page ?? 20;

        $model = $this->repository->getOrders($params);
        $total = $model->count();
        $result = $model->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();
        $result = $result->map(function ($item) {
            $item->status = __('param.deposit_order.status.'. $item->status);
            return $item;
        });
        return [$total, $result];
    }

    public function getOrder($member_id, $order_id)
    {
        $order = $this->repository->getOrder($member_id, $order_id);
        if (!$order) {
            throw new Exception(__('error_message.1521'), 1521);
        }
        return $order;
    }

    public function updateImageAndStatus($member_id, $id, $image_url, $transaction_no)
    {
        $order = $this->getOrder($member_id, $id);
        if ($order->status != 0) {
            throw new Exception(__('error_message.1522'), 1522);
        }
        $result = $this->repository->updateImageAndStatus($id, $image_url, $transaction_no);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }
}
