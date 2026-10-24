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

    public function getActiveAccount($member_id)
    {
        return DB::table('member_crypto_accounts as mca')
            ->join('banks as b', function ($join) {
                $join->on('b.id', 'mca.bank_id')
                    ->where('b.status', 1);
            })
            ->join('currencies as c', function ($join) {
                $join->on('c.id', 'b.currency_id')
                    ->where('c.status', 1)
                    ->where('c.support_withdrawal', 1);
            })
            ->where('mca.member_id', $member_id)
            ->where('mca.status', 1)
            ->orderBy('mca.sort', 'DESC')
            ->select(
                'mca.*'
            )
            ->first();
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
                    ->where('c.status', 1)
                    ->where('c.support_withdrawal', 1);
            })
            ->where('mca.member_id', $member_id)
            ->where('mca.status', 1)
            ->orderBy('mca.sort', 'DESC')
            ->select(
                'mca.id as account_id',
                'mca.bank_id',
                'b.name as bank_name',
                'b.image as bank_image',
                'mca.address as crypto_address'
            )
            ->first();
    }

    public function getCountByCryptoAccount($params)
    {
        return $this->model
            ->where('bank_id', $params->bank_id)
            ->where('address', $params->crypto_address)
            ->count();
    }
}
