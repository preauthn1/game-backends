<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class MemberWallet extends Model
{
    protected $table = 'member_wallets';

    protected $fillable = [
        'member_id',
        'wallet_type_id',
        'balance',
        'status',
        'memo',
    ];

    public function member()
    {
        return $this->belongsTo(Member::class);
    }

    public function walletType()
    {
        return $this->belongsTo(WalletType::class);
    }
}
