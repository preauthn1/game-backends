<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Member extends Model
{
    protected $table = 'members';

    protected $fillable = [
        'uid',
        'agent_id',
        'username',
        'password',
        'level',
        'referral_code',
        'invitation_code',
        'device_id',
        'device_type',
        'mobile_country_code',
        'mobile',
        'country_id',
        'status',
        'mobile_validate',
        'register_ip',
        'register_domain',
        'last_login',
        'login_fail_count',
        'total_login_fail_count',
        'real_name',
        'fund_password',
        'email',
        'is_2fa_enable',
        'google2fa_secret',
        'last_login_ip',
    ];

    public function memberWallets()
    {
        return $this->hasMany(MemberWallet::class);
    }

    public function memberMiners()
    {
        return $this->hasMany(MemberMiner::class);
    }
}
