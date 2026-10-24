<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class ForbidUserModel extends Model
{
    protected $connection = 'center';
    protected $table = 'Game.ForbidUser';

}
