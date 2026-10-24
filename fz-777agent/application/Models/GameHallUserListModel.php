<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class GameHallUserListModel extends Model
{
    protected $connection = 'center';
    protected $table = 'Game.HallUserList';
}