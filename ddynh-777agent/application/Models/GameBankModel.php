<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class GameBankModel extends Model
{
    protected $connection = 'log';
    protected $table = 'Money.BankLog';
}
