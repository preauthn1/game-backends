<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use App\Models\InterfaceModel;

class OrderModel extends Model
{
    protected $table = 'orders';
    /**
     * 充值
     * @param $thisOrder
     */
    public static function recharge($thisOrder){
        $createtime=strtotime($thisOrder->created_at);
        $getStr="user_id=$thisOrder->user_id&out_trade_no=$thisOrder->out_trade_no&total_fee=$thisOrder->total_fee&created_at=$createtime&channel=$thisOrder->channel&platform=$thisOrder->platform";
        $result=InterfaceModel::recharge($getStr);
        $res=json_decode($result);
        if($res->result==1){
            $data['status']=2;$data['oldgold']=$res->oldgold;$data['bankgold']=$res->brankgold;$data['package']=$res->package;$data['pf']=$res->pf;$data['agent']=$res->agent;$data['gold']=$res->gold;
            self::where('out_trade_no',$thisOrder->out_trade_no)->update($data);
        }else{
            self::log('发放失败：'.$thisOrder->out_trade_no.' '.$result,'pay'.date('md').'3.log');
        }
    }
    /**
     * 检查是否有通知失败订单
     */
    public static function checkRecharge(){
        $orders = self::where('status', 1)->get();
        foreach ($orders as $order){
            self::recharge($order);
        }
    }

    protected static function log ($content = '', $filename = 'debug.log')
    {
        error_log(date('Y-m-d H:i:s') . ' --- ' . $content . "\r\n", 3, dirname(dirname(__FILE__)) . '/logs/' .$filename);
    }
}
