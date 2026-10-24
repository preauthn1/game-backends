<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class CurrencyLog extends Model
{
    protected $fillable = [
        'agent_id',
        'currency_id',
        'before_info',
        'after_info',
        'remark'
    ];
}
