<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\Bank;
use DB;

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

    public function find($id)
    {
        return $this->model->find($id);
    }

    public function getWithdrawalDropdown($agent_group_id, $type)
    {
        return $this->model->where('agent_group_id', $agent_group_id)
            ->where('type', ($type + 10))
            ->where('status', 1)
            ->orderBy('sort', 'desc')
            ->orderBy('id', 'desc')
            ->select(
                'id',
                'name',
                'image'
            )
            ->get();
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
        $sub = DB::table('bank_accounts')
            ->whereRaw('status = 1')
            ->groupBy('bank_id')
            ->select('bank_id');

        return DB::table('banks as b')
            ->join('currencies as c', function ($join) {
                $join->on('c.id', 'b.currency_id')
                    ->where('c.status', 1)
                    ->where('c.support_deposit', 1);
            })
            ->join(DB::raw('(' . $sub->toSql() . ') as ba'), 'ba.bank_id', 'b.id')
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
                'c.name as currency_name',
                'c.deposit_rate as deposit_rate'
            )
            ->get();
    }

    public function getWithdrawalListByType($agent_group_id, $type)
    {
        return DB::table('banks as b')
            ->join('currencies as c', function ($join) {
                $join->on('c.id', 'b.currency_id')
                    ->where('c.status', 1)
                    ->where('c.support_withdrawal', 1);
            })
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
                'b.tax_type as bank_tax_type',
                'b.tax as bank_tax',
                'c.withdrawal_rate as bank_rate',
                'b.min_amount as bank_min_amount',
                'b.max_amount as bank_max_amount',
                'c.id as currency_id',
                'c.name as currency_name',
                'c.withdrawal_rate as withdrawal_rate'
            )
            ->get();
    }

    public function getAccountByBankId($agent_group_id, $bank_id)
    {

    }
}
