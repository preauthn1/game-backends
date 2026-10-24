<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class SmsText extends Model
{
    protected $fillable = [
        'agent_group_id',
        'type_id',
        'country_id',
        'text'
    ];

    public function agentGroup()
    {
        return $this->belongsTo(AgentGroup::class);
    }

    public function country()
    {
        return $this->belongsTo(Country::class);
    }
}
