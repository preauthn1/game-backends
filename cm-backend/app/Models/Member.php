<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Member extends Model
{
    protected $table = 'members';

    protected $fillable = [
        'uid',
        'type',
        'agent_id',
        'username',
        'nickname',
        'avatar_id',
        'password',
        'fund_password',
        'referral_code',
        'invitation_code',
        'is_validate',
        'email_validate',
        'email',
        'mobile_validate',
        'mobile',
        'country_id',
        'device_id',
        'device_type',
        'real_name',
        'member_id_card_id',
        'is_2fa_enable',
        'google2fa_secret',
        'register_country_id',
        'register_ip',
        'register_domain',
        'status',
        'sys_lock',
        'last_login',
        'last_login_ip',
        'first_deposit',
        'first_withdrawal',
        'total_login_fail_count',
        'login_fail_count',
        'forget_pwd_count',
        'memo',
        'level'
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
