<?php

namespace App\Services;

use App\Enums\PublicEnum;
use App\Repositories\BankRepository;
use App\Repositories\BankLogRepository;
use App\Helpers\Common;
use Exception;

class BankService extends BaseService
{
    public function __construct()
    {
        $this->repository = new BankRepository();
    }

    public function getDropdown($type)
    {
        return $this->repository->getDropdown($type);
    }

    public function find($id)
    {
        return $this->repository->getById($id);
    }

    public function updateTax($params)
    {
        $bank_id = $params->bank_id;
        $tax = $params->tax;
        $tax_type = $params->tax_type;
        $before = $this->find($bank_id);

        $data = array();
        $data['tax_type'] = $tax_type;
        $data['tax'] = $tax;
        $result = $this->repository->updateById($bank_id, $data);
        if (!$result) {
            throw new Exception(__('error_message.1502'), 1502);
        }
        $after = $this->find($bank_id);
        $this->writeBankLog(0, $bank_id, $before, $after, $params->ID);
    }

    public function getDepositByCurrencyId($currency_id, $request)
    {
        return $this->repository->getByCurrencyId($currency_id, [1, 10], $request);
    }

    public function getWithdrawalByCurrencyId($currency_id, $params)
    {
        return $this->repository->getByCurrencyId($currency_id, [11, 20], $params);
    }

    public function updateWithdrawalTaxByCurrencyId($params)
    {
        $currency_id = $params->currency_id;
        $tax_type = $params->tax_type;
        $tax = $params->tax;

        $before = $this->getWithdrawalByCurrencyId($currency_id, $params);
        $result = $this->repository->updateTaxByCurrencyId($currency_id, [11, 20], $tax_type, $tax);
        if (!$result) {
            throw new Exception(__('error_message.1502'), 1502);
        }
        $after = $this->getWithdrawalByCurrencyId($currency_id, $params);
        $this->writeBankLog($currency_id, 0, $before, $after, $params->ID);
        return $result;
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

    public function getDepositList($agent_group_id, $type = 0)
    {
        return $this->repository->getDepositListByType($agent_group_id, $type);
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

    public function getWithdrawalDropdown($agent_group_id)
    {
        $list = $this->repository->getWithdrawalListByType($agent_group_id, 0);
        $bank = [];
        $crypto = [];
        foreach ($list as $row) {
            $data = array();
            if ($row->bank_type == 1) {
                $data['bank_id'] = $row->bank_id;
                $data['bank_name'] = $row->bank_name;
                $bank[] = $data;
            } elseif ($row->bank_type == 3) {
                $data['crypto_id'] = $row->bank_id;
                $data['crypto_name'] = $row->bank_name;
                $crypto[] = $data;
            }
        }
        return [$bank, $crypto];
    }

    public function getWithdrawalBankList($params)
    {
        $page = $params->page ?? 1;
        $per_page = $params->per_page ?? 10;

        $model = $this->repository->getWithdrawalList($params);
        $total = $model->count();
        $result = $model->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();
        $result = $result->map(function ($item) {
            if ($item->bank_type == 11) {
                $item->bankTypeTxt = __('param.withdrawal_gateway.1001');
            } elseif ($item->bank_type == 13) {
                $item->bankTypeTxt = __('param.withdrawal_gateway.1002');
            } else {
                $item->bankTypeTxt = __('param.other');
            }
            $item->min_amount = floatval($item->min_amount ?? 0);
            $item->max_amount = floatval($item->max_amount ?? 0);
            $item->taxTypeTxt = __('param.withdrawal_fee.type.'.$item->tax_type);
            if(0==$item->tax_type){
                $item->taxTxt = floatval(bcmul($item->tax ?? 0,100,PublicEnum::DefaultScale)).'%';
            } else {
                $item->taxTxt = floatval($item->tax ?? 0);
            }
            return $item;
        });
        return [$total, $result];
    }

    public function withdrawalBankView($params)
    {
        $result = $this->repository->withdrawalBankView($params);
        if (!is_object($result)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        if ($result->bank_type == 11) {
            $result->bankTypeTxt = __('param.withdrawal_gateway.1001');
        } elseif ($result->bank_type == 13) {
            $result->bankTypeTxt = __('param.withdrawal_gateway.1002');
        } else {
            $result->bankTypeTxt = __('param.other');
        }
        $result->min_amount = floatval($result->min_amount ?? 0);
        $result->max_amount = floatval($result->max_amount ?? 0);
        return $result;
    }

    public function update($params)
    {
        $result = $this->repository->getById($params->bank_id);
        if (!is_object($result)) {
            throw new Exception(__('error_message.9001'), 9001);
        }

        $data = array();
        if (Common::IsNotNullOrEmptyString($params->alias)) {
            $data['code'] = $params->alias;
        }
        if (Common::IsNotNullOrEmptyString($params->min_amount)) {
            $data['min_amount'] = $params->min_amount;
        }
        if (Common::IsNotNullOrEmptyString($params->max_amount)) {
            $data['max_amount'] = $params->max_amount;
        }
        if (isset($params->tax_type)) {
            $data['tax_type'] = $params->tax_type;
        }
        if (isset($params->tax)) {
            $data['tax'] = $params->tax;
        }
        if (Common::IsNotNullOrEmptyString($params->sort)) {
            $data['sort'] = $params->sort;
        }
        if (Common::IsNotNullOrEmptyString($params->status)) {
            $data['status'] = $params->status;
        }
        if (Common::IsNotNullOrEmptyString($params->image_url)) {
            $data['image'] = $params->image_url;
        }
        $result = $this->repository->updateById($params->bank_id, $data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function getDepositBankList($params)
    {
        $page = $params->page ?? 1;
        $per_page = $params->per_page ?? 10;

        $model = $this->repository->getDepositBankList($params);
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
            $item->min_amount = floatval($item->min_amount ?? 0);
            $item->max_amount = floatval($item->max_amount ?? 0);
            return $item;
        });
        return [$total, $result];
    }

    public function createBank($params)
    {
        $data = array();
        $data['agent_group_id'] = $params->CASH_FLOW_GROUP;
        $data['type'] = $params->type_id;
        $data['currency_id'] = $params->currency_id;
        $data['name'] = $params->bank_name;
        if (!empty($params->bank_code ?? '')) {
            $data['code'] = $params->bank_code;
        }
        $data['min_amount'] = $params->min_amount;
        $data['max_amount'] = $params->max_amount;
        if (!empty($params->image_url ?? '')) {
            $data['image'] = $params->image_url;
        }
        if (!empty($params->tax_type ?? '')) {
            $data['tax_type'] = $params->tax_type;
        }
        if (!empty($params->tax ?? '')) {
            $data['tax'] = $params->tax;
        }
        if (!empty($params->sort ?? 0)) {
            $data['sort'] = $params->sort;
        }
        $data['status'] = 1;
        $result = $this->repository->create($data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function depositBankView($params)
    {
        $result = $this->repository->depositBankView($params);
        if (!is_object($result)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        if ($result->bank_type == 1) {
            $result->bankTypeTxt = __('param.deposit_gateway.1001');
        } elseif ($result->bank_type == 3) {
            $result->bankTypeTxt = __('param.deposit_gateway.1002');
        } else {
            $result->bankTypeTxt = __('param.other');
        }
        $result->min_amount = floatval($result->min_amount ?? 0);
        $result->max_amount = floatval($result->max_amount ?? 0);
        return $result;
    }

    public function updateDepositBank($params)
    {
        $bank = $this->find($params->bank_id);
        if (!is_object($bank)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        $data = array();
        if (Common::IsNotNullOrEmptyString($params->bank_code)) {
            $data['code'] = $params->bank_code;
        }
        if (Common::IsNotNullOrEmptyString($params->min_amount)) {
            $data['min_amount'] = $params->min_amount;
        }
        if (Common::IsNotNullOrEmptyString($params->max_amount)) {
            $data['max_amount'] = $params->max_amount;
        }
        if (Common::IsNotNullOrEmptyString($params->sort)) {
            $data['sort'] = $params->sort;
        }
        if (Common::IsNotNullOrEmptyString($params->status)) {
            $data['status'] = $params->status;
        }
        if (count($data) > 0) {
            $result = $this->repository->updateById($params->bank_id, $data);
            if (!$result) {
                throw new Exception(__('error_message.2001'), 2001);
            }
        }
    }
}
