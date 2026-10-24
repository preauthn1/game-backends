<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class PromotionName extends Model
{
    protected $fillable = [
        'language_id',
        'promotion_id',
        'title',
        'content',
        'image',
        'url'
    ];

    public function promotion()
    {
        return $this->belongsTo(Promotion::class);
    }

    public function language()
    {
        return $this->belongsTo(Language::class);
    }
}
