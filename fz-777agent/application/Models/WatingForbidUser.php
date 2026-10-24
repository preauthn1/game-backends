<?php

namespace App\Models;


use Illuminate\Database\Eloquent\Model;

class WatingForbidUser extends Model
{
    protected $connection = 'center';
    protected $table = 'CenterDB.Game.WaitingForbidUser';
    public $timestamps = false;
}