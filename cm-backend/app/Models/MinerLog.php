<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class MinerLog extends Model
{
    protected $fillable = [
        'agent_id',
        'miner_id',
        'before_info',
        'after_info',
        'remark'
    ];
}
