<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class GamePlayerMacBlackModel extends Model
{
    protected $connection = 'center';
    protected $table = 'Game.player_mac_black';
    public $timestamps=false;
}
