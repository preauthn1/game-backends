<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Miner extends Model
{
    protected $fillable = [
        'agent_group_id',
        'name',
        'image',
        'currency_id',
        'hash_rate',
        'power_consumption',
        'day_revenue',
        'depreciation_cycle',
        'purchase_price',
        'purchase_total',
        'rent_price',
        'rent_total',
        'group_price',
        'group_total',
        'promo',
        'sort',
        'status',
        'is_show',
        'info'
    ];
}
