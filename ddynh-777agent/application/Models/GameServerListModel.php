<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class GameServerListModel extends Model
{
    protected $connection = 'center';
    protected $table = 'CenterDB.ServerInfo.GameServerList';
    public $timestamps = false;

    const BAIREN_GAMEID = [9,11,13];//百人游戏
    const JINLIU_GAMEID = [1,2,3,15,12,7,6,8];//金流游戏
}
