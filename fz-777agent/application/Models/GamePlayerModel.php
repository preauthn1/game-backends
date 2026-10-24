<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class GamePlayerModel extends Model
{
    protected $connection = 'center';
    protected $table = 'CenterDB.Users.UserInfo';
    public $timestamps = false;

    public function playerData()
    {
        return $this->hasOne('App\Models\MoneyCashModel','UserID');
    }


}
