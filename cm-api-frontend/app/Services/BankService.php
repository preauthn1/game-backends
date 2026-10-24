<?php

namespace App\Services;

use App\Repositories\BankRepository;
use App\Repositories\BankAccountRepository;
use Exception;

class BankService extends BaseService
{
    public function __construct()
    {
        $this->repository = new BankRepository();
    }

    public function find($id)
    {
        $data = $this->repository->find($id);
        if (!$data) {
            throw new Exception(__('error_message.1502'), 1502);
        }
        return $data;
    }

    public function getDepositList($agent_group_id, $type = 0)
    {
        $result = $this->repository->getDepositListByType($agent_group_id, $type);
        return $result;
    }

    public function getCurrencyByBankId($agent_group_id, $bank_id)
    {
        $data = $this->repository->getCurrencyByBankId($agent_group_id, $bank_id);
        if (!$data) {
            throw new Exception(__('error_message.1502'), 1502);
        }
        return $data;
    }

    public function checkWithdrawalRate($agent_group_id, $bank_id, $rate)
    {
        $bank = $this->getCurrencyByBankId($agent_group_id, $bank_id);
        if ($bank->withdrawal_rate != $rate) {
            throw new Exception(__('error_message.1501'), 1501);
        }
        return $bank;
    }

    public function getWithdrawalList($agent_group_id, $type = 0)
    {
        $list = $this->repository->getWithdrawalListByType($agent_group_id, $type);
        $bank = [];
        $crypto = [];
        foreach ($list as $row) {
            if ($row->bank_type == 1) {
                $bank[] = $row;
            }
            if ($row->bank_type == 3) {
                $crypto[] = $row;
            }
        }

        return [$bank, $crypto];
    }

    public function getAccount($agent_group_id, $bank_id, $bank_account_id)
    {
        $bankAccountRepository = new BankAccountRepository();
        $params = [
            'agent_group_id' => $agent_group_id,
            'bank_id' => $bank_id,
            'bank_account_id' => $bank_account_id,
        ];
        $bank = $bankAccountRepository->getBankAccounts($params);
        if (!$bank[0]) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $bank[0];
    }

    public function getWithdrawalDropdown($agent_group_id, $type)
    {
        return $this->repository->getWithdrawalDropdown($agent_group_id, $type);
    }
}
