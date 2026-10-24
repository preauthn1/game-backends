<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Agent extends Model
{
    protected $table = 'agents';

    protected $fillable = [
        'host_id',
        'parent',
        'role_id',
        'hierarchy_path',
        'code',
        'username',
        'nickname',
        'email',
        'password',
        'payment_password',
        'im_certify',
        'mobile',
        'frontend_register',
        'has_2fa',
        'email_validate',
        'mobile_validate',
        'mobile_bind',
        'last_login_time',
        'last_login_ip',
        'memo',
        'organization_id',
        'customized_setting_group_id',
        'cash_flow_group_id',
        'sms_gateway_group_id',
        'miner_group_id',
        'last_reset_password',
        'created_by',
        'status',
    ];
}
