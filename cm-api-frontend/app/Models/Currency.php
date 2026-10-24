<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Currency extends Model
{
    protected $fillable = [
        'code',
        'name',
        'deposit_rate',
        'withdrawal_rate',
        'status',
    ];

    protected $casts = [
        'deposit_rate' => 'decimal:10', // 转换为 decimal 类型，精度为 10
        'withdrawal_rate' => 'decimal:10'
    ];

    public function bank()
    {
        return $this->hasMany(Bank::class);
    }
}
