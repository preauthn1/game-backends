<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class BankAccount extends Model
{
    protected $fillable = [
        'bank_id',
        'account_name',
        'account_nickname',
        'account_no',
        'remark',
        'status'
    ];

    public function bank()
    {
        return $this->belongsTo(Bank::class);
    }
}
