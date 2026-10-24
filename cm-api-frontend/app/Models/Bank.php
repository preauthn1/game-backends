<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Bank extends Model
{
    protected $fillable = [
        'agent_group_id',
        'type',
        'name',
        'code',
        'image',
        'currency_id',
        'info',
        'rate',
        'tax_type',
        'tax',
        'min_amount',
        'max_amount',
        'sort',
        'status',
        'remark'
    ];

    public function agentGroup()
    {
        return $this->belongsTo(AgentGroup::class);
    }

    public function currency()
    {
        return $this->belongsTo(Currency::class);
    }
}
