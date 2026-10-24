<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class MemberLoginLog extends Model
{
    protected $table = 'member_login_logs';

    protected $fillable = [
        'member_id',
        'ip',
        'ipv6',
        'trace_ip',
        'status'
    ];
}
