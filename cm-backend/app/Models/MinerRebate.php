<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class MinerRebate extends Model
{
    protected $table = 'miner_rebates';

    protected $fillable = [
        'miner_id',
        'percentage1',
        'percentage2',
        'percentage3',
        'status',
    ];
}
