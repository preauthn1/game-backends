<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class ReportDayModel extends Model
{
    //protected $connection = 'db';
	protected $table = 'report_day';
    public $timestamps = false;


	public function reportDayJson()
    {
        return $this->hasOne('App\Models\ReportDayJsonModel','day','day');
    }

	/**
	  * 获取表结构
	  * @return array
	  */
	public static function getTableColumns()
	{

	    $instance = new ReportDayModel();
	    return $instance->getConnection()->getSchemaBuilder()->getColumnListing($instance->getTable());
	}
}
