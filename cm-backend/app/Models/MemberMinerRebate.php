<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class MemberMinerRebate extends Model
{
    protected $fillable = [
        'type',
        'summary_time',
        'data_time',
        'member_id',
        'member_miner_id',
        'amount',
        'info',
        'memo',
    ];

    public function member()
    {
        return $this->belongsTo(Member::class);
    }

    public function memberMiner()
    {
        return $this->belongsTo(MemberMiner::class);
    }
}
