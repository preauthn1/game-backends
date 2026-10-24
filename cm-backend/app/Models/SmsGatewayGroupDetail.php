<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class SmsGatewayGroupDetail extends Model
{
    protected $fillable = [
        'agent_group_id',
        'setting'
    ];

    public function agentGroup()
    {
        return $this->belongsTo(AgentGroup::class);
    }
}
