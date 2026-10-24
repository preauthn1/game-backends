<?php
namespace App\Controllers\Agentadmin;

class SelectController extends BaseController
{

    public static function actionExchangeStatus(){
        return [0=>'兑换完成',1=>'审核中',2=>'审核通过', 3=>'已打款',4=>'订单异常', 5=>'异常帐号', 6=>'黑名单', 7=>'账户错误' ,8=>"拒绝兑换", 9=>"疑似洗钱"];
    }
}
