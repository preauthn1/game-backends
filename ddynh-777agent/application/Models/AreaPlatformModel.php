<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class AreaPlatformModel extends Model
{
    protected $connection = 'center';
    protected $table = 'Area.Platform';
    public $timestamps = false;
}
