<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\MemberWallet;
use DB;
use Exception;

class MemberWalletRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return MemberWallet::class;
    }

    public function increment($member_id, $point, $wallet_type_id)
    {
        $memberWallet = $this->model->where('member_id', $member_id)
            ->where('wallet_type_id', $wallet_type_id)
            ->first();
        $before_balance = $memberWallet->balance;
        $result = $memberWallet->increment('balance', $point);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        $memberWallet = $this->model->where('member_id', $member_id)
            ->where('wallet_type_id', $wallet_type_id)
            ->first();
        $after_balance = $memberWallet->balance;
        return [$before_balance, $after_balance];
    }

    public function decrement($member_id, $point, $wallet_type_id)
    {
        $memberWallet = $this->model->where('member_id', $member_id)
            ->where('wallet_type_id', $wallet_type_id)
            ->first();
        if (($memberWallet->balance - $point) < 0) {
            throw new Exception(__('error_message.1500'), 1500);
        }
        $before_balance = $memberWallet->balance;
        $result = $memberWallet->decrement('balance', $point);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        $memberWallet = $this->model->where('member_id', $member_id)
            ->where('wallet_type_id', $wallet_type_id)
            ->first();
        $after_balance = $memberWallet->balance;
        return [$before_balance, $after_balance];
    }

    public function findByMemberId($member_id, $wallet_type_id, $language_id)
    {
        $model = DB::table('member_wallets as mw')
            ->join('wallet_types as wt', function ($join) {
                $join->on('wt.id', 'mw.wallet_type_id')
                    ->where('wt.status', 1);
            })
            ->leftJoin('wallet_type_names as wtn', function ($join) use ($language_id) {
                $join->on('wtn.wallet_type_id', 'wt.id')
                    ->where('wtn.language_id', $language_id);
            })
            ->where('mw.member_id', $member_id)
            ->when($wallet_type_id > 0, function ($query) use ($wallet_type_id) {
                $query->where('mw.wallet_type_id', $wallet_type_id);
            })
            ->select(
                'mw.id',
                'mw.wallet_type_id',
                'mw.balance',
                'mw.member_id',
                DB::raw('IFNULL(wtn.name, wt.name) as wallet_type_name')
            );
        $data = ($wallet_type_id > 0) ? $model->first() : $model->get();
        return $data;
    }
}
