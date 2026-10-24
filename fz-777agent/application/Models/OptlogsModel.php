<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class OptlogsModel extends Model
{
    protected $table = 'optlogs';
    protected $fillable = ['userid','title', 'key', 'value'];
}
