<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class AreaGameListModel extends Model
{
    protected $connection = 'center';
    protected $table = 'Area.GameList';
    public $timestamps = false;
}
