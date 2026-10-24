<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\Currency;
use App\Helpers\Common;

/**
 * Class CurrencyRepository.
 */
class CurrencyRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return Currency::class;
    }

    public function getList($params, $type)
    {
        $model = $this->model
            ->where('type', '!=', 1)
            ->when(Common::IsNotNullOrEmptyString($params->type), function ($query) use ($params) {
                $query->where('type', $params->type);
            })
            ->when(Common::IsNotNullOrEmptyString($params->name), function ($query) use ($params) {
                $query->where('name', 'like', '%' . $params->name . '%');
            })
            ->when(Common::IsNotNullOrEmptyString($params->status), function ($query) use ($params) {
                $query->where('status', $params->status);
            })
            //只撈當前支援代收/代付的貨幣
            ->where(($type == 'deposit' ? 'support_deposit' : 'support_withdrawal'), 1)
            ->orderBy('id', 'asc')
            ->select(
                '*'
            );
        return $model;
    }

    /**
     * 取得幣別下拉選單, 1:Deposit, 2:Withdrawal, 3:mining
     */
    public function getDropdown($type)
    {
        return $this->model
            ->where('status', 1)
            ->when($type == 1, function ($query) {
                $query->where('support_deposit', 1);
            })
            ->when($type == 2, function ($query) {
                $query->where('support_withdrawal', 1);
            })
            ->when($type == 3, function ($query) {
                $query->where('support_mining', 1);
            })
            ->select(
                'id as currency_id',
                'name as currency_name'
            )
            ->get();
    }
}
