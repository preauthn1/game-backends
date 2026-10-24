<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class GameLogModel extends Model
{
    protected $connection = 'log';
    protected $table = 'LogDB.Game.GameLog';
}