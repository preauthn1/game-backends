<?php
namespace App\Controllers\Agentadmin\Agent;

use App\Controllers\Agentadmin\BaseController;
use App\Models\AgentAdModel;
use App\Models\AgentMoneyUserModel;
use App\Models\AgentPlanModel;
use App\Models\AgentPlayerModel;
use App\Models\AgentPlayerRechargeModel;
use App\Models\AgentReportDayModel;
use App\Models\AgentUserModel;
use App\Models\GamePlayerDataModel;
use App\Models\GamePlayerModel;
use App\Models\InterfaceModel;
use App\Module\BjuiGridModule;
use Illuminate\Database\Capsule\Manager as DB;

class DataSumController extends BaseController
{
    public function actionInitPageData($data){
        $agentInfo= AgentUserModel::where("id",$this->session->get("agentId"))->first();
        $playerInfo=GamePlayerModel::where("UserID",$agentInfo->player_id)->first();
        return array("agent"=>$agentInfo,"player"=>$playerInfo);
    }

    public function actionList($data){
        $params = BjuiGridModule::searchFormatterParam($data);
        $params['pageSize']= isset($data['pageSize'])?$data['pageSize']:'';
        $params['pageCurrent'] = isset($data['pageCurrent'])?$data['pageCurrent']:'';

        $list = AgentReportDayModel::where("agent_id",$this->session->get("agentId"))->select();
        $list = BjuiGridModule::searchWhere($params,$list);
        return $list;
    }
    
    public function actionEdit($data){
        return $data;
    }

    public function actionDel($data){
        return $this->retMessage('ok','操作成功');
    }
    
    //打卡上下班
    public function actionClock($data){
        if(isset($data['clock'])){
            $agentId=$this->session->get("agentId");
            $this->logRecord("actionClock|agentId:".$agentId.'|'.json_encode($data),'agent'.date('Y-m-d'),'agent');
            if($data['clock']==1){
                AgentAdModel::where("agent_id",$agentId)->delete();
                AgentUserModel::where("id",$agentId)->update(array("clock"=>$data['clock']));
                $level=AgentUserModel::where("id",$agentId)->value("level");
                $plan=AgentPlanModel::where("agents","like","%$level%")->pluck("id");
                $ads=array();$clock=date("Y-m-d H:i:s");
                foreach ($plan as $v){
                    $ads[]=array("agent_id"=>$agentId,"plan"=>$v,"clock"=>$clock,"status"=>1);
/*                    $num=AgentAdModel::where('status',1)->where('plan',$v)->count();
                    if($num<16){
                        $ads[]=array("agent_id"=>$agentId,"plan"=>$v,"clock"=>$clock,"status"=>1);
                    }else{
                        $ads[]=array("agent_id"=>$agentId,"plan"=>$v,"clock"=>$clock,"status"=>0);
                    }*/
                }
                AgentAdModel::insert($ads);
            }else if($data['clock']==0){
                AgentUserModel::where("id",$agentId)->update(array("clock"=>$data['clock']));
                AgentAdModel::where("agent_id",$agentId)->delete();
            }
            return $this->retMessage("ok","操作成功！");
        }
        return $this->retMessage("fail");
    }

    //代理转账给玩家
    public function actionAgentTurn($data){
        if(isset($data['playerId'])&&isset($data['money'])){
            $playerId=(int)abs($data['playerId']);
            $money=(int)abs($data['money']);
            $agentId=$this->session->get('agentId');
            if($money<$this->session->get('agentInfo')['min_money']){
                return $this->retMessage("fail","转账金额不能低于".$this->session->get('agentInfo')['min_money']);
            }
            $player=GamePlayerModel::where("UserID",$playerId)->first();
            if($player){
                //机器人
                if ($player->UserType == 2) {
                    return $this->retMessage('fail', '不能转给机器人');
                }
                $gold=AgentUserModel::where('id', $agentId)->value("money");
                if($gold>=$money){
                    //防止NGINX多次请求
                    $export_time=$this->session->get("operator_time");
                    if($export_time){
                        if($export_time+1>=time()){
                            return $this->retMessage("fail","警告！请求过于频繁！");
                        }
                    }
                    $this->session->set("operator_time",time());
                    //一分钟内同一玩家同一额度只允许一次
                    $same=AgentMoneyUserModel::where("agent_id",$agentId)->where("user_id",$playerId)->where("money",$money)->where("created_at",">",date("Y-m-d H:i:s",time() - 60))->first();
                    if($same){
                        return $this->retMessage("fail","一分钟内同一玩家同一额度只允许转账一次");
                    }
                    DB::beginTransaction();
                    try {
                        //添加代理金额明细
                        $agentData['agent_id']=$agentId;
                        $agentData['type']=0;
                        $agentData['user_id']=$playerId;
                        $agentData['money']=-$money;
                        $agentData['new_money']=$gold-$money;
                        $agentData['created_at']=\Carbon\Carbon::now();
                        $agentData['ip']=$this->getClientIP();
                        $agentData['area']=$this->getIPCity($agentData['ip']);
                        $agentData['status']=1;
                        $agentMoneyStatus = DB::table('agent_money_user')->insertGetId($agentData);
                        //添加玩家充值记录
                        $insertData['agent_id']     = $agentId;
                        $insertData['player_id']     = $playerId;
                        $insertData['money']        = $money;
                        $insertData['status'] = 1;
                        $insertData['agent_money_id']        = $agentMoneyStatus;
                        $insertData['created_at']=\Carbon\Carbon::now();
                        $add       = DB::table("agent_player_recharge")->insertGetId($insertData);
                        //减掉代理金币，增加总转出
                        $decrement = DB::table("agent_user")->where("id",$agentId)->decrement("money",$money);
                        $increment = DB::table("agent_user")->where("id",$agentId)->increment("total_turn_out",$money);
                        //更新玩家总充值
                        $playerTotal = AgentPlayerModel::where("player_id",$playerId)->first();
                        if($playerTotal){
                            $playerTotalStatus = DB::table('agent_player')->where('player_id',$playerId)->increment('total_recharge',$money);
                        }else{
                            $playerTotalStatus = DB::table('agent_player')->insert(array("player_id"=>$playerId,"total_recharge"=>$money));
                        }
                        //给玩家加金币
                        if($add&&$decrement&&$playerTotalStatus&&$agentMoneyStatus&&$increment){
                            $newMoney = 100 * $money;
                            $logArr = array(
                                $playerId, 11008,
                                $newMoney, $this->getClientIP(),
                                "代理{$agentId}转账",
                            );
                            $ret = $this->updateUserGold($logArr, 2);
                            if($ret){
                                DB::commit();
                                return $this->retMessage("ok","转账成功");
                            }else{
                                DB::rollBack();
                                return $this->retMessage("fail","转账接口调用失败");
                            }
                        }else{
                            DB::rollBack();
                            return $this->retMessage("fail","转账失败，请重试");
                        }
                    } catch (\Exception $e) {
                        $this->log($e,"agentTurn.log");
                        DB::rollBack();
                        return $this->retMessage("fail","转账失败，请联系管理");
                    }
                }
                return $this->retMessage("fail","余额不足");
            }else{
                /*$maxId=GamePlayerModel::max('id');
                if($playerId<$maxId){
                    //转给不存在的玩家ID时直接扣除
                    $insertData['agent_id']     = $agentId;
                    $insertData['player_id']     = $playerId;
                    $insertData['money']        = $money;
                    $insertData['status'] = 1;
                    $insertData['agent_money_id']        = 0;
                    $insertData['created_at']=\Carbon\Carbon::now();
                    $add       = AgentPlayerRechargeModel::insertGetId($insertData);
                    $decrement = AgentUserModel::where("id",$agentId)->decrement("money",$money);
                    return $this->retMessage("ok","转账成功");
                }*/
                $this->logRecord("actionAgentTurn|player:".json_encode($player)."|playerId:$playerId",'agent'.date('Y-m-d'),'agent');
                return $this->retMessage("fail","转账失败，玩家异常");
            }
        }
        return $this->retMessage("fail","操作失败");
    }
}
