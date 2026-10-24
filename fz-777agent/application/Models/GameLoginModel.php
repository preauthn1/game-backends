<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class GameLoginModel extends Model
{
    protected $connection = 'log';
    protected $table = 'Game.GameLog';
}
