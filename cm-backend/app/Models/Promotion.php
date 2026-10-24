<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Promotion extends Model
{
    protected $fillable = [
        'type',
        'miner_group_id',
        'title',
        'content',
        'image',
        'image_content',
        'start_time',
        'end_time',
        'url',
        'sort',
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
