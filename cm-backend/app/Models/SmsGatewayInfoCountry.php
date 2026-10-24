<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class SmsGatewayInfoCountry extends Model
{
    protected $fillable = [
        'sms_gateway_info_id',
        'country_id',
        'status'
    ];

    public function smsGatewayInfo()
    {
        return $this->belongsTo(SmsGatewayInfo::class);
    }

    public function country()
    {
        return $this->belongsTo(Country::class);
    }
}
