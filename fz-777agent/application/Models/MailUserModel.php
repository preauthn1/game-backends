<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class MailUserModel extends Model
{
    protected $connection = 'center';
    protected $table = 'Message.ReceiveUser';
    public $timestamps  = false;

}
