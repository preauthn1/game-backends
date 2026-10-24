<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class ConvertLogModel extends Model
{
    protected $connection = 'log';
    protected $table = 'LogDB.Money.ConvertLog';
    public $timestamps = false;
}