<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class SmsvcodeModel extends Model
{
    protected $connection = 'notify';
    protected $table = 'Notify.SMSVcode';
    public $timestamps = false;
}
