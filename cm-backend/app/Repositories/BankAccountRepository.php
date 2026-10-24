<?php

namespace App\Repositories;

use App\Helpers\Common;
use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\BankAccount;
use DB;

/**
 * Class BankAccountRepository.
 */
class BankAccountRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return BankAccount::class;
    }

    public function getBankAccountByBankId($agent_group_id, $bank_id)
    {
        return DB::table('bank_accounts as ba')
            ->join('banks as b', function ($query) use ($agent_group_id) {
                $query->on('b.id', 'ba.bank_id')
                    ->where('b.agent_group_id', $agent_group_id)
                    ->where('b.status', 1);
            })
            ->where('ba.bank_id', $bank_id)
            ->where('ba.status', 1)
            ->select(
                'ba.id',
                'ba.bank_id',
                'ba.account_name',
                'ba.account_no'
            )
            ->first();
    }

    public function getList($params){
        return DB::table('bank_accounts as ba')
            ->join('banks as b', function ($query) use ($params) {
                $query->on('b.id', 'ba.bank_id')
                    ->where('b.agent_group_id', $params->CASH_FLOW_GROUP)
                    ->where('b.status', 1);
            })
            ->join('currencies as c','c.id','b.currency_id')
            ->when($params->type > 0, function ($query) use ($params) {
                $query->where('b.type', $params->type);
            })
            ->when(Common::IsNotNullOrEmptyString($params->currency_id), function ($query) use ($params) {
                $query->where('b.currency_id', $params->currency_id);
            })
            ->when(Common::IsNotNullOrEmptyString($params->account_alias), function ($query) use ($params) {
                $query->where('ba.account_nickname', $params->account_alias);
            })
            ->when(Common::IsNotNullOrEmptyString($params->status), function ($query) use ($params) {
                $query->where('ba.status', $params->status);
            })
            ->select(
                'ba.id',
                'b.currency_id',
                'c.name as currency_name',
                'ba.bank_id',
                'b.name as bank_name',
                'b.type as bank_type',
                'ba.account_name',
                DB::raw("IFNULL(ba.account_nickname, '') as account_nickname"),
                'ba.account_no',
                'ba.status',
            );
    }
}
