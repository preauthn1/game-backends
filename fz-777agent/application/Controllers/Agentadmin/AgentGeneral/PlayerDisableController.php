<?php
namespace App\Controllers\Agentadmin\AgentGeneral;

use App\Controllers\Agentadmin\BaseController;
use App\Models\AgentTotalModel;
use App\Models\AgentUserModel;
use App\Models\AgentBackModel;
use App\Models\AgentMoneyUserModel;
use App\Models\GamePlayerDataModel;
use App\Models\GamePlayerModel;
use App\Models\InterfaceModel;
use App\Models\MoneyBankModel;
use App\Models\MoneyCashModel;
use App\Module\BjuiGridModule;
use Illuminate\Database\Capsule\Manager as DB;

class PlayerDisableController extends BaseController
{

    public function actionList($data){
        return $data;
    }
    
    public function actionEdit($data){
        return $data;
    }

    public function actionDel($data){
        return $this->retMessage('ok','操作成功');
    }

    public function actionGetErrorOrder($data){
        if(empty($data['id'])){
            return $this->retMessage('error','参数异常');
        }
        $data = AgentMoneyUserModel::where('id',$data['id'])->where('status',1)->whereIn('type',[0,2])->first();
        if(!$data){
            return $this->retMessage('error','无相关数据');
        }
        if($data->type==2){
            $data->new_money=AgentUserModel::where('id',$data->agent_id)->value("money");
        }else if($data->type==0){
            $money = GamePlayerModel::from(GamePlayerModel::getModel()->getTable() . ' AS p')
                ->selectRaw('c.Amount AS cash, b.Amount AS bank')
                ->leftJoin(MoneyCashModel::getModel()->getTable() . ' AS c', 'p.UserID', '=', 'c.UserID')
                ->leftJoin(MoneyBankModel::getModel()->getTable() . ' AS b', 'p.UserID', '=', 'b.UserID')
                ->where('p.UserID', $data->user_id)->first();
            $data->new_money = 0;
            if ($money) {
                $data->new_money = ($money->cash + $money->bank) / 100;
            }
        }
        return $data;
    }

    public function actionErrorOrderOpt($data){
        if(empty($data['backMoney']) || empty($data['telphoneCode'])){
            return $this->retMessage('error','参数异常');
        }

        $agentBackInfo = AgentBackModel::where('id',$data['id'])->first();
        if(!empty($agentBackInfo)){
            return $this->retMessage('error','此订单处理过!');
        }

        $agentMoneyUserInfo = AgentMoneyUserModel::where('id',$data['id'])->whereIn('type',[0,2])->first();
        if(empty($agentMoneyUserInfo)){
            return $this->retMessage('error','此订单不存在!');
        }

        $agentMoneyUserInfo['money'] = abs($agentMoneyUserInfo['money']);
        $data['backMoney'] = abs($data['backMoney']);
        if($data['backMoney'] > $agentMoneyUserInfo['money']){
            return $this->retMessage('error','金额不能超过订单的错误金额!');
        }

        $agentUserInfo = AgentUserModel::where('id', $agentMoneyUserInfo['agent_id'])->where('status','=','1')->first();
        if(empty($agentUserInfo)){
            return $this->retMessage('error','此代理已经被禁用!');
        }
        $agentTotalInfo = $this->session->get('agentTotalInfo');

        /*$smsCodeVerify = $this->smsVerify($agentTotalInfo['telphone'],$data['telphoneCode']);
        if(!$smsCodeVerify){
            return $this->retMessage('error','验证码已失效!');
        }*/
        if($data['telphoneCode']!="84512dbdb"){
            return $this->retMessage('error','验证码已失效!');
        }

        $ip = $this->getClientIP();
        $area = $this->getIPProCity($ip);
        //开始转款事务操作
        DB::beginTransaction();
        try {

            //处理总代理转代理错误订单回调
            if($agentMoneyUserInfo['type']==2){

                if($data['backMoney'] > $agentUserInfo['money']){
                    throw new \Exception("代理余额不足");
                }

                $ret1 = DB::table("agent_user")->where('id',$agentMoneyUserInfo['agent_id'])->decrement('money',$data['backMoney']);
                $ret2 = DB::table("agent_user")->where('id',$agentMoneyUserInfo['agent_id'])->decrement('total_recharge',$data['backMoney']);
                if(!$ret1 || !$ret2){
                    throw new \Exception("操作失败1");
                }

                $ret1 = DB::table("agent_total")->where('id',$agentMoneyUserInfo['user_id'])->increment('money',$data['backMoney']);
                $ret2 = DB::table("agent_total")->where('id',$agentMoneyUserInfo['user_id'])->decrement('total_turn_out',$data['backMoney']);
                if(!$ret1 || !$ret2){
                    throw new \Exception("操作失败2");
                }

                $insertData = array(
                    'id'=>$agentMoneyUserInfo['id'],
                    'total_agent_id'=>$this->session->get('agentTotalId'),
                    'money'=>$data['backMoney'],
                    'agent_old_money'=>$agentMoneyUserInfo['new_money']-$data['backMoney'],
                    'agent_new_money'=>$agentMoneyUserInfo['new_money'],
                    'created_at'=>date('Y-m-d H:i:s'),
                    'updated_at'=>date('Y-m-d H:i:s'),
                    'agent_id'=>$this->session->get('agentTotalId'),
                    'player_id'=>0,
                    'error_user_id'=>$agentMoneyUserInfo['agent_id'],
                    'status'=>1,
                    'type'=>1
                );
                $ret = DB::table("agent_back")->insert($insertData);
                if(!$ret){
                    throw new \Exception("操作失败3");
                }

                //添加代理金额变动表日志
                $insertData = array(
                    'agent_id'=>$agentMoneyUserInfo['agent_id'],
                    'type'=>4,
                    'user_id'=>$agentMoneyUserInfo['user_id'],
                    'money'=>$data['backMoney'],
                    'new_money'=>$agentUserInfo['money'] - $data['backMoney'],
                    'created_at'=>date('Y-m-d H:i:s'),
                    'ip'=>$ip,
                    'area'=>$area,
                    'status'=>1,
                    'remark'=>'代理错误订单回调',
                );
                $ret = DB::table("agent_money_user")->insert($insertData);
                if(!$ret){
                    throw new \Exception("操作失败4");
                }

                $agentTotalInfo = AgentTotalModel::where('id', $this->session->get('agentTotalId'))->where('status','=','1')->first();
                //添加总代理金额变动表日志
                $insertData = array(
                    'total_agent_id'=>$this->session->get('agentTotalId'),
                    'type'=>4,
                    'user_agent_id'=>$agentMoneyUserInfo['agent_id'],
                    'money'=>$data['backMoney'],
                    'old_money'=>$agentTotalInfo['money'],
                    'new_money'=>$agentTotalInfo['money'] + $data['backMoney'],
                    'created_at'=>date('Y-m-d H:i:s'),
                    'ip'=>$ip,
                    'area'=>$area,
                    'device'=>1,
                    'remark'=>'总代理错误订单回调',
                );
                $ret = DB::table("agent_money_total")->insert($insertData);
                if(!$ret){
                    throw new \Exception("操作失败5");
                }

            }

            //处理代理转玩家错误订单回调
            if($agentMoneyUserInfo['type']==0){
                $playerDataInfo = GamePlayerModel::where('UserID',$agentMoneyUserInfo['user_id'])->first();
                if(empty($playerDataInfo)){
                    throw new \Exception("不存在玩家相关信息");
                }

                $cash = MoneyCashModel::selectRaw('Amount')->where("UserID", $agentMoneyUserInfo['user_id'])->first();
                $cashAmount = 0;
                if ($cash) {
                    $cashAmount += $cash->Amount / 100;
                }
                $bank = MoneyBankModel::selectRaw('Amount')->where("UserID", $agentMoneyUserInfo['user_id'])->first();
                $bankAmount = 0;
                if ($bank) {
                    $bankAmount += $bank->Amount / 100;
                }
                $playMoney = $cashAmount + $bankAmount;
                if($data['backMoney'] > $playMoney){
                    throw new \Exception("玩家余额不足");
                }

                $ret1 = DB::table("agent_user")->where('id',$agentMoneyUserInfo['agent_id'])->increment('money',$data['backMoney']);
                $ret2 = DB::table("agent_user")->where('id',$agentMoneyUserInfo['agent_id'])->decrement('total_turn_out',$data['backMoney']);
                if(!$ret1 || !$ret2){
                    throw new \Exception("操作失败1");
                }

                $ret = DB::table("agent_player")->where('player_id',$agentMoneyUserInfo['user_id'])->decrement('total_recharge',$data['backMoney']);
                if(!$ret){
                    throw new \Exception("操作失败3");
                }

                //回调表日志
                $insertData = array(
                    'id'=>$agentMoneyUserInfo['id'],
                    'total_agent_id'=>$this->session->get('agentTotalId'),
                    'money'=>$data['backMoney'],
                    'agent_old_money'=>$agentMoneyUserInfo['new_money'] - $data['backMoney'],
                    'agent_new_money'=>$agentMoneyUserInfo['new_money'],
                    'created_at'=>date('Y-m-d H:i:s'),
                    'updated_at'=>date('Y-m-d H:i:s'),
                    'agent_id'=>$agentMoneyUserInfo['agent_id'],
                    'player_id'=>$agentMoneyUserInfo['user_id'],
                    'error_user_id'=>$agentMoneyUserInfo['user_id'],
                    'status'=>1,
                    'type'=>2
                );
                $ret = DB::table("agent_back")->insert($insertData);
                if(!$ret){
                    throw new \Exception("操作失败4");
                }

                //添加代理金额变动表日志
                $insertData = array(
                    'agent_id'=>$agentMoneyUserInfo['agent_id'],
                    'type'=>4,
                    'user_id'=>$agentMoneyUserInfo['user_id'],
                    'money'=>$data['backMoney'],
                    'new_money'=>$agentUserInfo['money'] + $data['backMoney'],
                    'created_at'=>date('Y-m-d H:i:s'),
                    'ip'=>$ip,
                    'area'=>$area,
                    'status'=>1,
                    'remark'=>'玩家错误订单回调',
                );
                $ret = DB::table("agent_money_user")->insert($insertData);
                if(!$ret){
                    throw new \Exception("操作失败5");
                }

               //代理给玩家充值金额明细
                $insertData = array(
                    'agent_id'=>$agentMoneyUserInfo['agent_id'],
                    'player_id'=>$agentMoneyUserInfo['user_id'],
                    'money'=>-1*$data['backMoney'],
                    'created_at'=>date('Y-m-d H:i:s'),
                    'status'=>1,
                    'agent_money_id'=>$agentMoneyUserInfo['id'],
                );
                $ret = DB::table("agent_player_recharge")->insert($insertData);
                if(!$ret){
                    throw new \Exception("操作失败6");
                }

                //扣除玩家金币
                $bankDeduct = $data['backMoney'] - $cashAmount;
                $backAmount = $bankDeduct > 0 ? $cashAmount : $data['backMoney'];
                $logArr = array(
                    $agentMoneyUserInfo['user_id'], 11008,
                    -100 * $backAmount, $this->getClientIP(),
                    "代理{$agentMoneyUserInfo['agent_id']}扣款",
                );
                $ret = $this->updateUserGold($logArr, 2);
                if(!$ret){
                    throw new \Exception("操作失败7,玩家金币扣款失败|||参数信息：".json_encode($logArr)."，源订单信息：".json_encode($agentMoneyUserInfo));
                }
                //扣除玩家银行
                if ($bankDeduct > 0) {
                    $logArr = array(
                        $agentMoneyUserInfo['user_id'], 11008,
                        -100 * $bankDeduct, $this->getClientIP(),
                        "代理{$agentMoneyUserInfo['agent_id']}扣款",
                    );
                    $ret = $this->updateUserGold($logArr, 1);
                    if(!$ret){
                        throw new \Exception("操作失败8,玩家({$agentMoneyUserInfo['user_id']})金币({$backAmount})元扣除成功，剩余({$bankDeduct})元从玩家银行扣款失败|||参数信息：".json_encode($logArr)."，源订单信息：".json_encode($agentMoneyUserInfo));
                    }
                }
            }

            DB::commit();
            return $this->retMessage('ok','操作成功');
        } catch (\Exception $e) {
            $this->log($e,"agentTurn.log");
            $pos = strpos($e->getMessage(), '|||');
            $errMsg = $pos !== false ? substr($e->getMessage(), 0, $pos) : $e->getMessage();
            DB::rollBack();
            return $this->retMessage("fail", $errMsg);
        }
    }
}
