<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class StatSourceTypeModel extends Model
{
    protected $connection = 'center';
    protected $table = 'Stat.MoneySourceType';
}
