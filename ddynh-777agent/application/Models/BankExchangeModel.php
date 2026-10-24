<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class BankExchangeModel extends Model
{
    protected $connection = 'log';
    protected $table = 'LogDB.Money.BankConvertLog';
    public $timestamps = false;
}
