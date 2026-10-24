<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class MemberIdCard extends Model
{
    protected $fillable = [
        'member_id',
        'image_front',
        'image_back',
        'status',
        'remark',
        'operator'
    ];
}
