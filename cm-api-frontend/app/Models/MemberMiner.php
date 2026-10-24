<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class MemberMiner extends Model
{
    protected $table = 'member_miners';

    protected $fillable = [
        'member_id',
        'miner_detail_id',
        'parent_id',
        'check_time',
        'start_time',
        'end_time',
        'can_sellback_time',
        'cancel_time',
        'status'
    ];

    public function member()
    {
        return $this->belongsTo(Member::class);
    }
}
