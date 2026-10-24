<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class MinerPromotion extends Model
{
    protected $fillable = [
        'miner_id',
        'quantity',
        'rant_quantity',
        'rant_cycles',
        'status'
    ];
}
