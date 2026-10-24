<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class GamePlayerTelBlackModel extends Model
{
    protected $connection = 'center';
    protected $table = 'Game.player_tel_black';
    public $timestamps=false;
}
