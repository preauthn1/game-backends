<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class AgentLoginLog extends Model
{
    protected $fillable = [
        'agent_id',
        'ip',
        'ipv6',
        'remark',
        'status'
    ];
}
