<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class GameExchangeModel extends Model
{
    protected $connection = 'log';
    protected $table = 'LogDB.Money.ConvertLog';
    public $timestamps = false;
    const STATUS_DATA = [0 => "等待审核", 1 => "疑似洗钱", 2 => "审核通过", 3 => "已拒绝", 4 => "打款失败", 5 => "异常帐号", 6 => "兑换成功", 7 => "兑换失败", 12 => "代付"];
}
