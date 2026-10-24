<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class SmsGatewayCountry extends Model
{
    protected $fillable = [
        'sms_gateway_id',
        'country_id',
        'status'
    ];

    public function smsGateway()
    {
        return $this->belongsTo(SmsGateway::class);
    }

    public function country()
    {
        return $this->belongsTo(Country::class);
    }
}
