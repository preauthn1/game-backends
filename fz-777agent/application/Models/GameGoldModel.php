<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class GameGoldModel extends Model
{
    protected $connection = 'log';
    protected $table = 'LogDB.Money.MoneyLog';
}
