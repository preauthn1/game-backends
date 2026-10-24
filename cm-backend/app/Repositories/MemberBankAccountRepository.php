<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\MemberBankAccount;
use DB;
use Exception;

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

    public function getByMemberIdBankId($member_id, $bank_id)
    {
        return $this->model->where('member_id', $member_id)
            ->where('bank_id', $bank_id)
            ->first();
    }

    public function setDisabled($member_id)
    {
        return $this->model->where('member_id', $member_id)
            ->update(['status' => 0]);
    }

    public function insertOrUpdateData($member_id, $bank_id, $account_name, $account_no)
    {
        $account = $this->model->where('member_id', $member_id)
            ->where('bank_id', $bank_id)
            ->where('account_name', $account_name)
            ->where('account_no', $account_no)
            ->first();
        $data = array();
        $data['status'] = 1;
        if (is_object($account)) {
            $result = $account->update($data);
        } else {
            $data['member_id'] = $member_id;
            $data['bank_id'] = $bank_id;
            $data['account_name'] = $account_name;
            $data['account_no'] = $account_no;
            $result = $this->model->create($data);
        }
        return $result;
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
                     ->where('c.status', 1);
            })
            ->where('mba.member_id', $member_id)
            ->where('c.support_withdrawal', 1)
            ->where('mba.status', 1)
            ->orderBy('mba.sort', 'DESC')
            ->select(
                'mba.id as account_id',
                'mba.bank_id',
                'b.name as bank_name',
                'mba.account_name',
                'mba.account_no'
            )
            ->first();
    }

    public function getBankByMemberId($member_id, $bank_id)
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
                     ->where('c.status', 1);
            })
            ->where('mba.member_id', $member_id)
            ->where('c.support_withdrawal', 1)
            ->where('mba.status', 1)
            ->orderBy('mba.sort', 'DESC')
            ->select(
                'mba.id as account_id',
                'mba.bank_id',
                'b.name as bank_name',
                'mba.account_name',
                'mba.account_no',
                'b.min_amount as bank_min_amount',
                'b.max_amount as bank_max_amount',
                'b.tax_type as bank_tax_type',
                'b.tax as bank_tax',
                'c.withdrawal_rate as withdrawal_rate'
            )
            ->first();
    }
}
