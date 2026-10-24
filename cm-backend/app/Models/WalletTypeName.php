<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class WalletTypeName extends Model
{
    protected $fillable = [
        'language_id',
        'wallet_type_id',
        'name'
    ];

    public function walletType()
    {
        return $this->belongsTo(WalletType::class);
    }
}
