<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class MemberCryptoAccount extends Model
{
    protected $fillable = [
        'member_id',
        'bank_id',
        'address',
        'sort',
        'status',
    ];
}
