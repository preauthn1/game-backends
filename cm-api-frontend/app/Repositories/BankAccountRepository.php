<?php

namespace App\Repositories;

use App\Helpers\Common;
use Illuminate\Support\Facades\DB;
use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\BankAccount;

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
                'b.name as bank_name',
                'b.type as bank_type',
                'ba.account_name',
                'ba.account_no'
            )
            ->first();
    }

    public function getBankAccounts($params)
    {
        return DB::table('bank_accounts as ba')
            ->join('banks as b', function ($query) use ($params) {
                $query->on('b.id', 'ba.bank_id')
                    ->where('b.agent_group_id', $params['agent_group_id'])
                    ->where('b.status', 1);
            })
            ->where('ba.bank_id', $params['bank_id'])
            ->when(Common::IsNotNullOrEmptyString($params['bank_account_id'] ?? ''), function ($query) use ($params) {
                $query->where('ba.id', $params['bank_account_id']);
            })
            ->where('ba.status', 1)
            ->select(
                'ba.id',
                'ba.bank_id',
                'b.name as bank_name',
                'b.type as bank_type',
                'ba.account_name',
                'ba.account_no'
            )
            ->get();
    }
}
