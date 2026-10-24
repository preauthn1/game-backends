<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class DetailLogModel extends Model
{
    protected $connection = 'perGameLog';
    protected $table = 'PerGameLog.PerLog.DetailLog';
}