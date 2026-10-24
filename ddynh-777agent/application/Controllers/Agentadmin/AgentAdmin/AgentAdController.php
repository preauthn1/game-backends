<?php
namespace App\Controllers\Agentadmin\AgentAdmin;

use App\Controllers\Agentadmin\BaseController;
use App\Models\AgentAdModel;
use App\Models\AgentPlanModel;
use App\Models\GamePlayerDataModel;
use App\Models\GamePlayerModel;
use App\Module\BjuiGridModule;

class AgentAdController extends BaseController
{

    public function actionInitPageData(){
        $agentPlans = AgentPlanModel::get()->toArray();
        $result['agentPlans'] = json_encode($agentPlans);
        return $result;
    }

    public function actionList($data){
        $params = BjuiGridModule::searchFormatterParam($data);
        $params['pageSize']= isset($data['pageSize'])?$data['pageSize']:'';
        $params['pageCurrent'] = isset($data['pageCurrent'])?$data['pageCurrent']:'';
        $params['orders'] = isset($data['orders'])?$data['orders']:'clock asc';
        $plan = isset($data['plan'])?$data['plan']:1;
        $list = AgentAdModel::leftJoin("agent_user","agent_ad.agent_id","=","agent_user.id")->selectRaw("agent_ad.*,agent_user.level,agent_user.min_money,agent_user.money,agent_user.remark")->where("plan",$plan);
        $list = BjuiGridModule::searchWhere($params,$list);
        return $list;
    }
    
    public function actionEdit($data){
        return $data;
    }

    public function actionDel($data){
        return $this->retMessage('ok','操作成功');
    }
    //撤下
    public function actionRemoveOpt($data){
        if(isset($data['id'])){
            $res=AgentAdModel::where('id',$data['id'])->update(['status' =>0,'clock'=>date("Y-m-d H:i:s")]);
            if($res){
                return $this->retMessage('ok','操作成功');
            }
        }
        return $this->retMessage("fail","操作失败");
    }
    //优先
    public function actionPriorityOpt($data){
        if(isset($data['id'])){
            $plan=AgentAdModel::where('id',$data['id'])->value("plan");
            //$clock=AgentAdModel::where('plan',$plan)->where('status',0)->orderBy('clock','asc')->limit(1)->value('clock');
            $res=AgentAdModel::where('id',$data['id'])->update(['clock'=>date("Y-m-d H:i:s"),'status'=>1]);
            if($res){
                return $this->retMessage('ok','操作成功');
            }
        }
        return $this->retMessage("fail","操作失败");
    }

    public function actionPlanEditOpt($data){
        foreach ($data['plans'] as $key => $value) {
            $id = $value['id'];
            unset($value['id']);
            $ret = AgentPlanModel::where('id',$id)->update($value);
        }
        return $this->retMessage('ok','操作成功');
    }
    //修改玩家广告等级
    public function actionAdLevel($data){
        if(isset($data['playerId'])&&isset($data['level'])){
            $playerId=(int)abs($data['playerId']);
            $level=(int)abs($data['level']);
            if($level>4){
                return $this->retMessage("fail","等级不正确！");
            }
            $player=GamePlayerModel::where("UserID",$playerId)->first();
            if($player){
                $AdPlanTime = date('Y-m-d H:i:s');
                GamePlayerModel::where('UserID',$playerId)->update(['AdPlan'=>$level,'AdPlanTime'=>$AdPlanTime]);
                return $this->retMessage("ok","操作成功");
            }
            return $this->retMessage("fail","玩家信息不正确！");
        }
    }
}
