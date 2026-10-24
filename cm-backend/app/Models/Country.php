<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Country extends Model
{
    protected $fillable = [
        'code',
        'name',
        'country_code',
        'phone_rule',
        'sort',
        'status',
    ];
}
