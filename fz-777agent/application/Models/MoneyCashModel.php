<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class MoneyCashModel extends Model
{
    protected $connection = 'center';
    protected $table = 'CenterDB.Money.Cash';

}
