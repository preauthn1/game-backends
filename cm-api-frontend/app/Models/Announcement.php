<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Announcement extends Model
{
    protected $fillable = [
        'type',
        'title',
        'content',
        'is_top',
        'status'
    ];

    public function getTypeTextAttribute()
    {
        $types = [
            1 => 'System',
            2 => 'Promotion',
            3 => 'Platform',
        ];

        return $types[$this->type] ?? 'Unknown';
    }
}
