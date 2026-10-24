<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class MemberMinerIncome extends Model
{
    protected $fillable = [
        'type',
        'summary_time',
        'data_time',
        'member_id',
        'member_miner_id',
        'amount',
        'info',
        'memo',
        'status'
    ];
}
