<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class HallServerInfoModel extends Model
{
    protected $connection = 'center';
    protected $table = 'HallServerInfo';
    public $timestamps  = false;
}
