<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class AreaChannelModel extends Model
{
    protected $connection = 'center';
    protected $table = 'CenterDB.Area.Area';
    public $timestamps = false;
}
