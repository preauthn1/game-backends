<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class ServerGameServerListModel extends Model
{
    protected $connection = 'center';
    protected $table = 'CenterDB.ServerInfo.GameServerList';
}