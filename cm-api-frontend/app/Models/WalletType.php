<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class WalletType extends Model
{
    protected $fillable = [
        'type',
        'code',
        'name',
        'info',
        'status',
        'memo'
    ];

    public function memberWallet()
    {
        return $this->hasMany(MemberWallet::class);
    }

    public function walletTypeName()
    {
        return $this->hasMany(WalletTypeName::class);
    }
}
