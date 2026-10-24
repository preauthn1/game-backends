<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class GamePlayerRechargeBlackModel extends Model
{
    protected $connection = 'center';
    protected $table = 'Game.player_recharge_black';
    public $timestamps=false;
}
