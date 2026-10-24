<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class MoneyBankModel extends Model
{
    protected $connection = 'center';
    protected $table = 'CenterDB.Money.Bank';

}
