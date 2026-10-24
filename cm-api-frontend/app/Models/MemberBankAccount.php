<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class MemberBankAccount extends Model
{
    protected $fillable = [
        'member_id',
        'bank_id',
        'account_name',
        'account_no',
        'sort',
        'status',
    ];
}
