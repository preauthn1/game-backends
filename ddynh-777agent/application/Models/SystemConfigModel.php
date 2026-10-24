<?php
namespace App\Models;


use Illuminate\Database\Eloquent\Model;

class SystemConfigModel extends Model
{
    protected $connection = 'center';
    protected $table = 'CenterDB.ServerInfo.SystemConfig';
    public $timestamps = false;
}