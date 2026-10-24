<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class WithdrawalOrder extends Model
{
    protected $fillable = [
        'order_no',
        'bank_id',
        'member_id',
        'point',
        'fee',
        'rate',
        'amount',
        'cardholder',
        'cardno',
        'image',
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
