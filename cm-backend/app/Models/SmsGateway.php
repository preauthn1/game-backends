<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class SmsGateway extends Model
{
    protected $fillable = [
        'name',
        'code',
        'info',
        'url',
        'memo',
        'status'
    ];
}
