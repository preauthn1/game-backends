<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class AgentOperateLog extends Model
{
    protected $table = 'agent_operate_logs';

    protected $fillable = [
        'api_path',
        'api_name',
        'request',
        'response_code',
        'response_message',
        'response_data',
        'time_cost',
        'type',
        'agent_id',
        'agent_ip',
    ];
}
