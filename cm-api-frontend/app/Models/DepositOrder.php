<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class DepositOrder extends Model
{
    protected $fillable = [
        'order_no',
        'bank_id',
        'bank_account_id',
        'member_id',
        'amount',
        'rate',
        'point',
        'cardholder',
        'image',
        'transaction_no',
        'ip',
        'status',
        'extra_content',
        'remark',
        'pay_time',
        'operator'
    ];

    public function member()
    {
        return $this->belongsTo(Member::class);
    }

    public function bank()
    {
        return $this->belongsTo(Bank::class);
    }
}
