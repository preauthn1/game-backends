<?php
namespace App\Controllers\Agentadmin;

use App\Models\AgentAdModel;
use App\Models\AgentApplyModel;
use App\Models\AgentChatModel;
use App\Models\AgentMoneyUserModel;
use App\Models\AgentPlanModel;
use App\Models\AgentRedModel;
use App\Models\AgentReportDayModel;
use App\Models\AgentTipModel;
use App\Models\AgentUserModel;
use App\Models\GamePlayerDataModel;
use App\Models\GamePlayerModel;
use Illuminate\Database\Capsule\Manager as DB;

class ApiController extends BaseController
{
protected $pv='';
	public function actionCallControllers(){
		$path = $this->request->getAttribute('path');
        $params = $this->request->getParsedBody();
		$paths = explode('/', $path);//路由分发
		$controller = $this->getCfg('controllers','agents.'.$paths[0].'.'.$paths[1]);
		if(empty($controller)){
			$contrs = explode('_', $paths[1]);
			$controller = '';
			foreach ($contrs as $key => $value) {
				$controller .= ucfirst($value);
			}
			$controller.='Controller';
		}
		$controller = 'App\Controllers\Agentadmin\\'.$controller;
        $clsMethods = get_class_methods($controller);
        if(empty($clsMethods)){
        	return [];
		}
		var_dump(array($controller, 'action'.$paths[2]));die;
		return call_user_func_array(array($controller, 'action'.$paths[2]), array($params));
	}
	//玩家代理聊天接口
    /*public function actionChat(){
        $playerId = $this->request->getAttribute('player');
        $agentId = $this->request->getAttribute('agent');
        if ($this->request->isPost()) {//添加聊天
            $res['status']=0;$res['reply']="";
            $agentInfo = AgentUserModel::where("id",$agentId)->first();
            if($agentInfo){
                $newData = $this->request->getParsedBody();
                $insert['player_id']=$playerId;
                $insert['agent_id']=$agentId;
                $insert['reply']=$agentInfo->auto_reply;
                $insert['msg']=$this->filterEmoji($newData['msg']);
                $insert['created_at']=\Carbon\Carbon::now();
                $status=AgentChatModel::insert($insert);
                if($status){
                    $res['status']=1;
                    $res['reply']=$agentInfo->auto_reply;//自动回复
                }
            }
            return $this->retJsonData($res);
        }else{
            $res['status']=0;$res['data']=[];
            AgentRedModel::where('red_agent_id',$agentId)->where('red_player_id',$playerId)->delete();
            $chat=AgentChatModel::select('player_id','agent_id','msg','reply','created_at')->where("player_id",$playerId)->where("agent_id",$agentId)->get();
            if($chat){
                $res['status']=1;
                $res['data']=$chat;
            }
            return $this->retJsonData($res);
        }
    }*/
    //玩家申请代理
    /*public function actionApply(){
        $playerId = $this->request->getAttribute('player');
        if ($this->request->isPost()) {
            $res['status']=0;$res['msg']='';
            $player=AgentApplyModel::where('player_id',$playerId)->first();
            if($player){
                $res['msg']=$player->status==2?"您的申请被拒！":($player->status==1?'您的申请已通过，请不要重复提交！':'您的申请正在审核中，请不要重复提交！');
            }else{
                $newData = $this->request->getParsedBody();
                if(isset($newData['name'])&&isset($newData['qq'])&&isset($newData['weixin'])&&isset($newData['tel'])&&isset($newData['reason'])){
                    $playerInfo = GamePlayerModel::where("id",$playerId)->first();
                    if($playerInfo){
                        if(empty($playerInfo->password)){
                            return $this->retJsonData($res);
                        }
                        $apply['player_id']=$playerId;
                        $apply['channel']=$playerInfo->pf;
                        $apply['name']=$this->filterEmoji($newData['name']);
                        $apply['weixin']=$this->filterEmoji($newData['weixin']);
                        $apply['qq']=$newData['qq'];
                        $apply['telphone']=$newData['tel'];
                        $apply['reason']=$this->filterEmoji($newData['reason']);
                        $apply['created_at']=\Carbon\Carbon::now();
                        $res['status']=AgentApplyModel::insert($apply);
                    }
                }
            }
            return $this->retJsonData($res);
        }else{
            $chat=AgentApplyModel::where("player_id",$playerId)->get();
            return $this->retJsonData($chat);
        }
    }*/
    //广告定时轮换方案一
    /*public function adRotation1(){
        DB::transaction(function () {
            $num=16;//一次显示代理的个数
            DB::table('agent_ad')->where("plan",1)->where('status',1)->update(['status' =>0,'clock'=>date("Y-m-d H:i:s")]);
            $ads=AgentAdModel::where("plan",1)->where('status',0)->orderBy("clock","asc")->limit($num)->pluck("id");
            DB::table('agent_ad')->whereIn("id",$ads)->update(['status' =>1]);
        }, 2);
    }
    //广告定时轮换方案二
    public function adRotation2(){
        DB::transaction(function () {
            $num=16;//一次显示代理的个数
            DB::table('agent_ad')->where("plan",2)->where('status',1)->update(['status' =>0,'clock'=>date("Y-m-d H:i:s")]);
            $ads=AgentAdModel::where("plan",2)->where('status',0)->orderBy("clock","asc")->limit($num)->pluck("id");
            DB::table('agent_ad')->whereIn("id",$ads)->update(['status' =>1]);
        }, 2);
    }
    //广告定时轮换方案三
    public function adRotation3(){
        DB::transaction(function () {
            $num=16;//一次显示代理的个数
            DB::table('agent_ad')->where("plan",3)->where('status',1)->update(['status' =>0,'clock'=>date("Y-m-d H:i:s")]);
            $ads=AgentAdModel::where("plan",3)->where('status',0)->orderBy("clock","asc")->limit($num)->pluck("id");
            DB::table('agent_ad')->whereIn("id",$ads)->update(['status' =>1]);
        }, 2);
    }
    //广告定时轮换方案四
    public function adRotation4(){
        DB::transaction(function () {
            $num=16;//一次显示代理的个数
            DB::table('agent_ad')->where("plan",4)->where('status',1)->update(['status' =>0,'clock'=>date("Y-m-d H:i:s")]);
            $ads=AgentAdModel::where("plan",4)->where('status',0)->orderBy("clock","asc")->limit($num)->pluck("id");
            DB::table('agent_ad')->whereIn("id",$ads)->update(['status' =>1]);
        }, 2);
    }*/
    /*//定时判断玩家显示方案(PHP执行)
    public function agentPlan(){
        $player=GamePlayerDataModel::where("total_recharge",">",0)->orWhere("total_agent_recharge",">",0)->select("player_id","total_recharge","total_agent_recharge","single_recharge","total_revenue")->get();
        $plan=AgentPlanModel::where("id",">",1)->orderBy('id','desc')->get();
        $ids=array(2=>[],3=>[],4=>[]);
        foreach ($player as $value){
            foreach ($plan as $pv){
                if($value->total_recharge>=$pv->total_recharge||$value->total_agent_recharge>=$pv->total_agent_recharge||$value->single_recharge>=$pv->single_recharge||$value->total_revenue>=$pv->total_pump){
                    $ids[$pv->id][]=$value->player_id;
                    break;
                }
            }
        }
        foreach ($ids as $k =>$v){
            GamePlayerDataModel::whereIn('player_id',$v)->update(['ad_plan'=>$k]);
        }
    }
    //定时判断玩家显示方案(MYSQL执行)
    public function agentPlanSQL(){
        $plan=AgentPlanModel::where("id",">",1)->orderBy('id','desc')->get();
        foreach ($plan as $pv){
            $this->pv=$pv;
            switch ($pv->id){
                case 4:
                    GamePlayerDataModel::where('total_recharge','>=',$pv->total_recharge)->orWhere('total_agent_recharge','>=',$pv->total_agent_recharge)
                        ->orWhere('single_recharge','>=',$pv->single_recharge)->orWhere('total_revenue','>=',$pv->total_pump)->update(['ad_plan'=>4]);
                    break;
                case 3:
                    GamePlayerDataModel::where('ad_plan','<>',4)
                        ->orWhere(function($query){
                            $query->where('total_recharge','>=',$this->pv->total_recharge)->orWhere('total_agent_recharge','>=',$this->pv->total_agent_recharge)
                                ->orWhere('single_recharge','>=',$this->pv->single_recharge)->orWhere('total_revenue','>=',$this->pv->total_pump);
                        })->update(['ad_plan'=>3]);
                    break;
                case 2:
                    GamePlayerDataModel::where('ad_plan','<',3)
                        ->orWhere(function($query){
                            $query->where('total_recharge','>=',$this->pv->total_recharge)->orWhere('total_agent_recharge','>=',$this->pv->total_agent_recharge)
                                ->orWhere('single_recharge','>=',$this->pv->single_recharge)->orWhere('total_revenue','>=',$this->pv->total_pump);
                        })->update(['ad_plan'=>2]);
                    break;
            }
        }
    }
    //分三次判断玩家显示方案
    public function agentPlanSQL4(){
        $pv=AgentPlanModel::where("id",4)->first();
        GamePlayerDataModel::where('total_recharge','>=',$pv->total_recharge)->orWhere('total_agent_recharge','>=',$pv->total_agent_recharge)
            ->orWhere('single_recharge','>=',$pv->single_recharge)->orWhere('total_revenue','>=',$pv->total_pump)->update(['ad_plan'=>4]);
    }
    public function agentPlanSQL3(){
        $this->pv=AgentPlanModel::where("id",3)->first();
        GamePlayerDataModel::where('ad_plan','<>',4)
            ->where(function($query){
                $query->where('total_recharge','>=',$this->pv->total_recharge)->orWhere('total_agent_recharge','>=',$this->pv->total_agent_recharge)
                    ->orWhere('single_recharge','>=',$this->pv->single_recharge)->orWhere('total_revenue','>=',$this->pv->total_pump);
            })->update(['ad_plan'=>3]);
    }
    public function agentPlanSQL2(){
        $this->pv=AgentPlanModel::where("id",2)->first();
        GamePlayerDataModel::where('ad_plan','<',3)
            ->where(function($query){
                $query->where('total_recharge','>=',$this->pv->total_recharge)->orWhere('total_agent_recharge','>=',$this->pv->total_agent_recharge)
                    ->orWhere('single_recharge','>=',$this->pv->single_recharge)->orWhere('total_revenue','>=',$this->pv->total_pump);
            })->update(['ad_plan'=>2]);
    }
    public function agentPlanSQL1(){
        $this->pv=AgentPlanModel::where("id",2)->first();
        GamePlayerDataModel::where('ad_plan','>',1)->where('total_recharge','<',$this->pv->total_recharge)->where('total_agent_recharge','<',$this->pv->total_agent_recharge)
            ->where('single_recharge','<',$this->pv->single_recharge)->where('total_revenue','<',$this->pv->total_pump)->update(['ad_plan'=>1]);
    }*/
    //每天统计一次代理情况
    /*public function agentReportDay(){
        $today = date("Y-m-d",time()-5400);
        $between=[$today.' 00:00:00',$today.' 23:59:59'];
        $report = AgentMoneyUserModel::selectRaw("agent_id,ISNULL(sum(CASE WHEN type=2 THEN money END),0) AS recharge,ISNULL(sum(CASE WHEN type=0 THEN money END),0) AS sale")
            ->whereBetween("created_at",$between)->groupBy('agent_id')->get()->toArray();
        foreach ($report as $rv){
            $rv['profit'] =$rv['recharge']*0.02;
            $count=AgentReportDayModel::where('day',$today)->where('agent_id',$rv['agent_id'])->first();
            if($count){
                AgentReportDayModel::where('id',$count->id)->update($rv);
            }else{
                $rv['day']=$today;
                AgentReportDayModel::insert($rv);
            }
        }
    }*/
    //举报代理
    /*public function actionTip(){
        if ($this->request->isPost()) {
            $res['status']=0;
            $newData = $this->request->getParsedBody();
            if(isset($newData['info'])&&isset($newData['content'])){
                $newData['info']=$this->filterEmoji($newData['info']);
                $newData['content']=$this->filterEmoji($newData['content']);
                $res['status']=AgentTipModel::insert($newData);
            }
            return $this->retJsonData($res);
        }
    }*/
}
