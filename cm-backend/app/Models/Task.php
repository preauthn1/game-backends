<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Task extends Model
{
    protected $fillable = [
        'id',
        'command',
        'description',
        'expression',
        'status',
        'info',
        'operator',
        'last_run_at',
    ];
}
