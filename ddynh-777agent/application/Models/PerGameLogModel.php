<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class PerGameLogModel extends Model
{
    protected $connection = 'perGameLog';
    protected $table = 'PerGameLog.PerLog.PerGameLog';
}