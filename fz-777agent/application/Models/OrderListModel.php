<?php
namespace App\Models;

use App\Controllers\PayController;
use Illuminate\Database\Eloquent\Model;

class OrderListModel extends Model
{
    protected $connection = 'center';
    protected $table = 'CenterDB.Pay.OrderList';
    public $timestamps = false;

    public function handleNotDeLiver($container) {
        $time = date('Y-m-d H:i:s', time()-30);
        $orders = self::where("Status", 1)->where("PayTime", "<", $time)->get();
        if ($orders) {
            foreach ($orders as $k => $v) {
                $controller = new PayController($container);
                $sourceTypeId = 11000 + $v['Platform'];
                $controller->deliver($v['Id'], $sourceTypeId, $v['Amount'], $v['IPAddress'], '在线支付');
            }
        }
    }
}