<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class MiningBlock extends Model
{
    protected $fillable = [
        'currency_id',
        'block_height',
        'block_time',
        'mining_difficulty',
        'mining_change',
        'block_reward',
        'status',
        'operator'
    ];
}
