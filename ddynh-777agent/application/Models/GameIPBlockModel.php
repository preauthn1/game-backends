<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class GameIPBlockModel extends Model
{
    protected $connection = 'center';
    protected $table = 'Game.ForbidIPAddress';
}
