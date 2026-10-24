<?php

namespace App\Services;

use Carbon\Carbon;
use App\Repositories\CurrencyRepository;
use App\Repositories\CurrencyLogRepository;
use App\Helpers\Common;

/**
 * Class CurrencyService.
 */
class CurrencyService extends BaseService
{

    public function __construct()
    {
        $this->repository = new CurrencyRepository();
    }

    public function getById($id)
    {
        return $this->repository->getById($id);
    }

    public function updateRate($params)
    {
        $before = $this->getById($params->currency_id);
        $data = array();
        if (Common::IsNotNullOrEmptyString($params->deposit_rate)) {
            $data['deposit_rate'] = $params->deposit_rate;
        }
        if (Common::IsNotNullOrEmptyString($params->withdrawal_rate)) {
            $data['withdrawal_rate'] = $params->withdrawal_rate;
        }
        $result = $this->repository->updateById($params->currency_id, $data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        $after = $this->getById($params->currency_id);
        $this->writeCurrencyLog($params->currency_id, $before, $after, $params->ID);
    }

    public function writeCurrencyLog($currency_id, $before, $after, $agent_id)
    {
        $repository = new CurrencyLogRepository();
        $data = array();
        $data['agent_id'] = $agent_id;
        $data['currency_id'] = $currency_id;
        $data['before_info'] = json_encode($before);
        $data['after_info'] = json_encode($after);
        $result = $repository->create($data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function getList($params, $type)
    {
        $page = isset($params->page) ? $params->page : 1;
        $per_page = isset($params->per_page) ? $params->per_page : 50;
        $model = $this->repository->getList($params, $type);
        $total = $model->count();
        $result = $model->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();

        $result = $result->map(function ($item) use ($type) {
            if ($type == 'deposit') {
                unset($item->support_mining);
                unset($item->support_withdrawal);
                unset($item->withdrawal_rate);
            } else {
                unset($item->support_mining);
                unset($item->support_deposit);
                unset($item->deposit_rate);
            }

            $item->statusTxt = ($item->status == 1) ? __('param.enable') : __('param.disable');
            $item->typeTxt = __('param.currency.type.' . $item->type);

            // 轉換為 +8 時區並格式化為 'Y-m-d H:i:s'
            $item->created_at = Carbon::parse($item->created_at, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d H:i:s');
            $item->updated_at = Carbon::parse($item->updated_at, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d H:i:s');

            return $item;
        });

        return [$total, $result];
    }

    public function getDropdown($type)
    {
        return $this->repository->getDropdown($type);
    }
}