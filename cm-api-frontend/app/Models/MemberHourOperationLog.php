<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class MemberHourOperationLog extends Model
{
    protected $fillable = [
        'data_time',
        'member_id',
        'operation_count'
    ];
}
