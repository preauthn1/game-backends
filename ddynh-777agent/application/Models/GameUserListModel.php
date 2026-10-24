<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class GameUserListModel extends Model
{
    protected $connection = 'center';
    protected $table = 'CenterDB.Game.GameUserList';
}
