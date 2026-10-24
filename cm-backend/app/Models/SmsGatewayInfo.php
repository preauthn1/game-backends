<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class SmsGatewayInfo extends Model
{
    protected $fillable = [
        'agent_group_id',
        'sms_gateway_id',
        'info',
        'sort',
        'status'
    ];

    public function smsGateway()
    {
        return $this->belongsTo(SmsGateway::class);
    }

    public function agentGroup()
    {
        return $this->belongsTo(AgentGroup::class);
    }
}
