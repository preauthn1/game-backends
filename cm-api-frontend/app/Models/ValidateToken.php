<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class ValidateToken extends Model
{
    protected $fillable = [
        'member_id',
        'type_id',
        'mobile',
        'info',
        'token',
        'expire_date',
        'status',
        'times',
    ];
}
