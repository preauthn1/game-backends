<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class BankLog extends Model
{
    protected $fillable = [
        'agent_id',
        'currency_id',
        'bank_id',
        'before_info',
        'after_info',
        'remark'
    ];

    public function bank()
    {
        return $this->belongsTo(Bank::class);
    }

    public function agent()
    {
        return $this->belongsTo(Agent::class);
    }
}
