<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class MinerReward extends Model
{
    protected $table = 'miner_rewards';

    protected $fillable = [
        'miner_id',
        'quantity',
        'amount',
        'status',
    ];
}
