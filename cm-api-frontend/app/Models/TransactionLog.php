<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class TransactionLog extends Model
{
    protected $fillable = [
        'wallet_type_id',
        'member_id',
        'transaction_type_id',
        'order_id',
        'point',
        'before_balance',
        'after_balance',
        'status'
    ];
}
