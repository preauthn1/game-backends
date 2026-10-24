<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class GamePlayerIpBlackModel extends Model
{
    protected $connection = 'center';
    protected $table = 'Game.player_ip_black';
    public $timestamps=false;
}
