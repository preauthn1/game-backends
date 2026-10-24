<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class GameMessage extends Model
{
    protected $connection = 'center';
    protected $table = 'CenterDB.Game.GameMessage';
    public $timestamps = false;
}