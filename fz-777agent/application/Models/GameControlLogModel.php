<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class GameControlLogModel extends Model{
    protected $connection = 'log';
    protected $table = 'LogDB.Game.GameControlLog';
    public $timestamps = false;
    const MODPOOLAMOUNT = 1;//修改水池金币
    const MODPOPWATERRATE = 2;//修改奖池注水率
    const MODRESULT = 3;//修改开奖结果
    const MODSETCANCELTABLE = 4;//强制取消一桌牌局
    const MODSETSETWINLINES = 5;//强制取消一桌牌局
    const VIEWDATA = 6;//查询数据
    public static $logTypeData = [self::MODPOOLAMOUNT => '修改水池金币', self::MODPOPWATERRATE => '修改奖池注水率', self::MODRESULT => '修改开奖结果', self::MODSETCANCELTABLE => '强制取消牌局', self::MODSETSETWINLINES => '设置水池线', self::VIEWDATA => '查看数据'];
}
