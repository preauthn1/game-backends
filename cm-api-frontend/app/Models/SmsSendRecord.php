<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class SmsSendRecord extends Model
{
    protected $fillable = [
        'sms_gateway_id',
        'member_id',
        'country_id',
        'mobile',
        'message_content',
        'transaction_id',
        'request_info',
        'response_info',
        'status'
    ];

    public function smsGateway()
    {
        return $this->belongsTo(SmsGateway::class);
    }

    public function member()
    {
        return $this->belongsTo(Member::class);
    }

    public function country()
    {
        return $this->belongsTo(Country::class);
    }
}
