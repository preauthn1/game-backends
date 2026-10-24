<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class LoginLog extends Model
{
    protected $table    = 'log_login';
    public $timestamps = false;
}
