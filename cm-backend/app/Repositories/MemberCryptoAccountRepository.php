<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\MemberCryptoAccount;
use DB;

/**
 * Class MemberCryptoAccountRepository.
 */
class MemberCryptoAccountRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return MemberCryptoAccount::class;
    }

    public function getEnableByMemberId($member_id)
    {
        return $this->model->where('member_id', $member_id)
            ->where('status', 1)
            ->first();
    }

    public function setDisableByMemberId($member_id)
    {
        return $this->model->where('member_id', $member_id)
            ->update(['status' => 0]);
    }

    public function insertOrUpdateData($member_id, $bank_id, $address)
    {
        $account = $this->model->where('member_id', $member_id)
            ->where('bank_id', $bank_id)
            ->where('address', $address)
            ->first();
        $data = array();
        $data['status'] = 1;
        if (is_object($account)) {
            $result = $account->update($data);
        } else {
            $data['member_id'] = $member_id;
            $data['bank_id'] = $bank_id;
            $data['address'] = $address;
            $result = $this->model->create($data);
        }
        return $result;
    }

    public function getByMemberId($member_id, $bank_id)
    {
        return DB::table('member_crypto_accounts as mca')
            ->join('banks as b', function ($join) use ($bank_id) {
                $join->on('b.id', 'mca.bank_id')
                    ->when($bank_id > 0, function ($query) use ($bank_id) {
                        $query->where('b.id', $bank_id);
                    })
                    ->where('b.status', 1);
            })
            ->join('currencies as c', function ($join) {
                $join->on('c.id', 'b.currency_id')
                    ->where('c.status', 1);
            })
            ->where('mca.member_id', $member_id)
            ->where('c.support_withdrawal', 1)
            ->where('mca.status', 1)
            ->orderBy('mca.sort', 'DESC')
            ->select(
                'mca.id as account_id',
                'mca.bank_id',
                'b.name as bank_name',
                'mca.address as crypto_address'
            )
            ->first();
    }

    public function getCryptoByMemberId($member_id, $bank_id)
    {
        return DB::table('member_crypto_accounts as mca')
            ->join('banks as b', function ($join) use ($bank_id) {
                $join->on('b.id', 'mca.bank_id')
                    ->when($bank_id > 0, function ($query) use ($bank_id) {
                        $query->where('b.id', $bank_id);
                    })
                    ->where('b.status', 1);
            })
            ->join('currencies as c', function ($join) {
                $join->on('c.id', 'b.currency_id')
                    ->where('c.status', 1);
            })
            ->where('mca.member_id', $member_id)
            ->where('c.support_withdrawal', 1)
            ->where('mca.status', 1)
            ->orderBy('mca.sort', 'DESC')
            ->select(
                'mca.id as account_id',
                'mca.bank_id',
                'b.name as bank_name',
                'mca.address as account_no',
                'b.min_amount as bank_min_amount',
                'b.max_amount as bank_max_amount',
                'b.tax_type as bank_tax_type',
                'b.tax as bank_tax',
                'c.withdrawal_rate as withdrawal_rate'
            )
            ->first();
    }
}
