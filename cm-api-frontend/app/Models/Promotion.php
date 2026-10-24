<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Promotion extends Model
{
    protected $fillable = [
        'agent_id',
        'title',
        'content',
        'image',
        'start_time',
        'end_time',
        'url',
        'status'
    ];

    public function agent()
    {
        return $this->belongsTo(Agent::class);
    }

    public function promotionName()
    {
        return $this->hasMany(PromotionName::class);
    }
}
