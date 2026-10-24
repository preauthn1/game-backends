<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class ServerGameListModel extends Model
{
    protected $connection = 'center';
    protected $table = 'CenterDB.ServerInfo.GameList';
    public $timestamps = false;
}
