<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Menu extends Model
{
    protected $fillable = [
        'parent_id',
        'order',
        'name',
        'symbol',
        'uri',
        'permission',
        'is_show',
    ];
}
