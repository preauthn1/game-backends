<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class MemberMinerTransactionLog extends Model
{
    protected $fillable = [
        'transaction_log_id',
        'member_miner_id',
    ];
}
