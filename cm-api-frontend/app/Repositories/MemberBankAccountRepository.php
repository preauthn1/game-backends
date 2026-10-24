<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\MemberBankAccount;
use DB;

/**
 * Class MemberBankAccountRepository.
 */
class MemberBankAccountRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return MemberBankAccount::class;
    }

    //存在使用中的銀行帳戶
    public function getActiveAccount($member_id)
    {
        return DB::table('member_bank_accounts as mba')
            ->join('banks as b', function ($join) {
                $join->on('b.id', 'mba.bank_id')
                    ->where('b.status', 1);
            })
            ->join('currencies as c', function ($join) {
                $join->on('c.id', 'b.currency_id')
                    ->where('c.status', 1)
                    ->where('c.support_withdrawal', 1);
            })
            ->where('mba.member_id', $member_id)
            ->where('mba.status', 1)
            ->select(
                'mba.*'
            )
            ->first();
    }

    public function getByMemberId($member_id, $bank_id)
    {
        return DB::table('member_bank_accounts as mba')
            ->join('banks as b', function ($join) use ($bank_id) {
                $join->on('b.id', 'mba.bank_id')
                    ->when($bank_id > 0, function ($query) use ($bank_id) {
                        $query->where('b.id', $bank_id);
                    })
                    ->where('b.status', 1);
            })
            ->join('currencies as c', function ($join) {
                $join->on('c.id', 'b.currency_id')
                    ->where('c.status', 1)
                    ->where('c.support_withdrawal', 1);
            })
            ->where('mba.member_id', $member_id)
            ->where('mba.status', 1)
            ->orderBy('mba.sort', 'DESC')
            ->select(
                'mba.id as account_id',
                'mba.bank_id',
                'b.name as bank_name',
                'b.image as bank_image',
                'mba.account_name',
                'mba.account_no'
            )
            ->first();

    }

    public function getCountByBankAccount($params)
    {
        return $this->model
            ->where('bank_id', $params->bank_id)
            ->where('account_no', $params->account_no)
            ->count();
    }
}
