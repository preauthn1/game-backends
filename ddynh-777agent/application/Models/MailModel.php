<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\SoftDeletes;

class MailModel extends Model
{
    protected $connection = 'center';
    protected $table = 'Message.MessageList';
    public $timestamps  = false;

}
