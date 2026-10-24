<?php

namespace App\Services;

use App\Repositories\MemberWalletRepository;
use Exception;

class MemberWalletService extends BaseService
{
    public function __construct()
    {
        $this->repository = new MemberWalletRepository();
    }

    public function lockForUpdateByMemberId($member_id, $type_id)
    {
        $memberWallet = $this->repository->lockForUpdateByMemberId($member_id, $type_id);
        if (!$memberWallet) {
            //查無錢包
            throw new Exception(__('error_message.1503'), 1503);
        }
        return $memberWallet;
    }

    public function increment($member_id, $point, $wallet_type_id = 1)
    {
        list($before_balance, $after_balance) = $this->repository->increment($member_id, $point, $wallet_type_id);
        return [$before_balance, $after_balance];
    }

    public function decrement($member_id, $point, $wallet_type_id = 1)
    {
        list($before_balance, $after_balance) = $this->repository->decrement($member_id, $point, $wallet_type_id);
        return [$before_balance, $after_balance];
    }

}