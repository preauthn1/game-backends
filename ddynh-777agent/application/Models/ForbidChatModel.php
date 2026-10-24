<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class ForbidChatModel extends Model
{
    protected $connection = 'center';
    protected $table = 'Game.ForbidChatUser';

}
