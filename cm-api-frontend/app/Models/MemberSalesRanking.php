<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class MemberSalesRanking extends Model
{
    protected $fillable = [
        'member_id',
        'quantity_sold',
        'rebate_amount',
        'info'
    ];
}
