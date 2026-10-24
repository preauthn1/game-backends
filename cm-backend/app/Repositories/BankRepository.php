<?php

namespace App\Repositories;

use App\Helpers\Common;
use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\Bank;
use Illuminate\Support\Facades\DB;

/**
 * Class BankRepository.
 */
class BankRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return Bank::class;
    }

    /**
     * 取得銀行下拉選單
     */
    public function getDropdown($type)
    {
        return $this->model
            ->where('status', 1)
            ->when($type > 0, function ($query) use ($type) {
                $query->whereBetween('status', ($type == 1 ? [1, 10] : [11, 20]));
            })
            ->select(
                'id as bank_id',
                'name as bank_name',
                'currency_id as currency_id'
            )
            ->get();
    }

    public function getByCurrencyId($currency_id, $range, $request)
    {
        return $this->model->where('currency_id', $currency_id)
            ->when(Common::IsNotNullOrEmptyString($request->CASH_FLOW_GROUP), function ($query) use ($request) {
                $query->where('agent_group_id', $request->CASH_FLOW_GROUP);
            })
            ->whereBetween('type', $range)
            ->get();
    }

    public function updateTaxByCurrencyId($currency_id, $range, $tax_type, $tax)
    {
        return $this->model->where('currency_id', $currency_id)
            ->whereBetween('type', $range)
            ->update(['tax_type' => $tax_type, 'tax' => $tax]);
    }

    public function getCurrencyByBankId($agent_group_id, $bank_id)
    {
        return DB::table('banks as b')
            ->join('currencies as c', function ($join) {
                $join->on('c.id', 'b.currency_id')
                    ->where('c.status', 1);
            })
            ->where('b.agent_group_id', $agent_group_id)
            ->where('b.id', $bank_id)
            ->where('b.status', 1)
            ->select(
                'c.*'
            )
            ->first();
    }

    public function getDepositListByType($agent_group_id, $type)
    {
        return DB::table('banks as b')
            ->join('currencies as c', function ($join) {
                $join->on('c.id', 'b.currency_id')
                    ->where('c.status', 1);
            })
            ->where('b.agent_group_id', $agent_group_id)
            ->where('b.type', '<', 10)
            ->when($type > 0, function ($query) use ($type) {
                $query->where('b.type', $type);
            })
            ->where('b.status', 1)
            ->orderBy('b.type', 'asc')
            ->orderBy('b.sort', 'desc')
            ->orderBy('b.id', 'desc')
            ->select(
                'b.id as bank_id',
                'b.type as bank_type',
                'b.name as bank_name',
                'b.image as bank_image',
                'b.tax as bank_tax',
                'b.min_amount as bank_min_amount',
                'b.max_amount as bank_max_amount',
                'c.id as currency_id',
                'c.name as currency_name'
            )
            ->get();
    }

    public function getWithdrawalListByType($agent_group_id, $type)
    {
        return DB::table('banks as b')
            ->join('currencies as c', function ($join) {
                $join->on('c.id', 'b.currency_id')
                    ->where('c.status', 1);
            })
            ->where('c.support_withdrawal', 1)
            ->where('b.agent_group_id', $agent_group_id)
            ->where('b.type', '>', 10)
            ->when($type > 0, function ($query) use ($type) {
                $query->where('b.type', (10 + $type));
            })
            ->where('b.status', 1)
            ->orderBy('b.type', 'asc')
            ->orderBy('b.sort', 'desc')
            ->orderBy('b.id', 'desc')
            ->select(
                'b.id as bank_id',
                DB::raw('(b.type-10) as bank_type'),
                'b.name as bank_name',
                'b.image as bank_image',
                'b.tax as bank_tax',
                'c.withdrawal_rate as bank_rate',
                'b.min_amount as bank_min_amount',
                'b.max_amount as bank_max_amount',
                'c.id as currency_id',
                'c.name as currency_name'
            )
            ->get();
    }

    public function getWithdrawalList($params)
    {
        return DB::table('banks as b')
            ->join('currencies as c', function ($join) {
                $join->on('c.id', 'b.currency_id')
                    ->where('c.status', 1)
                    ->where('c.support_withdrawal', 1);
            })
            ->when(0 == $params->type, function ($query) use ($params) {
                $query->where('b.type', '>=', 10);
            })
            ->when($params->type > 10, function ($query) use ($params) {
                $query->where('b.type', $params->type);
            })
            ->when(Common::IsNotNullOrEmptyString($params->currency_id), function ($query) use ($params) {
                $query->where('b.currency_id', $params->currency_id);
            })
            ->when(Common::IsNotNullOrEmptyString($params->account_alias), function ($query) use ($params) {
                $query->where('b.code', $params->account_alias);
            })
            ->where('b.agent_group_id', $params->CASH_FLOW_GROUP)
            ->select(
                'b.id',
                'b.currency_id',
                'c.name as currency_name',
                'b.name as bank_name',
                'b.type as bank_type',
                DB::raw("IFNULL(b.code, '') as account_alias"),
                'b.min_amount',
                'b.max_amount',
                'b.tax_type',
                'b.tax',
                'b.status',
                'b.sort',
                'b.image'
            );
    }

    public function withdrawalBankView($params)
    {
        return DB::table('banks as b')
            ->join('currencies as c', function ($join) {
                $join->on('c.id', 'b.currency_id')
                    ->where('c.status', 1)
                    ->where('c.support_withdrawal', 1);
            })
            ->where('b.agent_group_id', $params->CASH_FLOW_GROUP)
            ->where('b.id', $params->bank_id)
            ->select(
                'b.id',
                'b.currency_id',
                'c.name as currency_name',
                'b.name as bank_name',
                'b.type as bank_type',
                DB::raw("IFNULL(b.code, '') as account_alias"),
                'b.min_amount',
                'b.max_amount',
                'b.tax_type',
                'b.tax',
                'b.status',
                'b.sort',
                'b.image'
            )->first();
    }

    public function getDepositBankList($params)
    {
        return DB::table('banks as b')
            ->join('currencies as c', function ($join) {
                $join->on('c.id', 'b.currency_id')
                    ->where('c.support_deposit', 1);
            })
            ->where('b.agent_group_id', $params->CASH_FLOW_GROUP)
            ->whereBetween('b.type', [1, 10])
            ->when($params->type > 0, function ($query) use ($params) {
                $query->where('b.type', $params->type);
            })
            ->when(Common::IsNotNullOrEmptyString($params->currency_id), function ($query) use ($params) {
                $query->where('b.currency_id', $params->currency_id);
            })
            ->when(Common::IsNotNullOrEmptyString($params->bank_name), function ($query) use ($params) {
                $query->where('b.name', $params->bank_name);
            })
            ->when(Common::IsNotNullOrEmptyString($params->status), function ($query) use ($params) {
                $query->where('b.status', $params->status);
            })
            ->orderBy('id', 'desc')
            ->select(
                'b.id',
                'b.currency_id',
                'c.name as currency_name',
                'b.name as bank_name',
                'b.code as bank_code',
                'b.type as bank_type',
                'b.min_amount',
                'b.max_amount',
                'b.sort',
                'b.status',
            );
    }

    public function depositBankView($params)
    {
        return DB::table('banks as b')
            ->join('currencies as c', function ($join) {
                $join->on('c.id', 'b.currency_id')
                    ->where('c.support_deposit', 1);
            })
            ->where('b.agent_group_id', $params->CASH_FLOW_GROUP)
            ->where('b.id', $params->bank_id)
            ->select(
                'b.id',
                'b.currency_id',
                'b.name as bank_name',
                'b.code as bank_code',
                'b.type as bank_type',
                'b.min_amount',
                'b.max_amount',
                'b.sort',
                'b.status',
            )->first();
    }
}
