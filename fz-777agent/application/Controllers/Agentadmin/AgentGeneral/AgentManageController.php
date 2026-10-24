<?php
namespace App\Controllers\Agentadmin\AgentGeneral;

use App\Controllers\Agentadmin\BaseController;
use App\Models\AgentTotalModel;
use App\Models\AgentUserModel;
use App\Models\AgentMoneyTotalModel;
use App\Models\AgentMoneyUserModel;
use App\Module\BjuiGridModule;
use Illuminate\Database\Capsule\Manager as DB;

class AgentManageController extends BaseController
{
    public function actionInitPageData($data){
        $agentTotalInfo = AgentTotalModel::where('id', $this->session->get('agentTotalId'))->where('status','=','1')->first();
        $agentUserInfo = AgentUserModel::selectRaw('SUM(money) as total_agent_money')->where('parent_id', $this->session->get('agentTotalId'))->first();
        $result['total_money'] = $agentTotalInfo['money'];
        $result['total_turn_out'] = $agentTotalInfo['total_turn_out'];
        $result['total_recharge'] = $agentTotalInfo['total_recharge'];
        $result['total_agent_money'] = $agentUserInfo['total_agent_money'];
        return $result;
    }


    public function actionList($data){
        $params = BjuiGridModule::searchFormatterParam($data);
        $params['pageSize']= isset($data['pageSize'])?$data['pageSize']:'';
        $params['pageCurrent'] = isset($data['pageCurrent'])?$data['pageCurrent']:'';

        $list = AgentUserModel::select()->where('parent_id', $this->session->get('agentTotalId'));
        $list = BjuiGridModule::searchWhere($params,$list);
        return $list;
    }
    
    public function actionEdit($data){
        if(empty($data['id'])){
            $ids=AgentUserModel::pluck('id')->toArray();
            $newid=rand(10000,99999);
            for($i=0;$i<5;$i++){
                if(in_array($newid,$ids)){
                    $newid=rand(10000,99999);
                    continue;
                }else{
                    break;
                }
            }
            $data['id']=$newid;
            $data['created_at'] = date('Y-m-d H:i:s');
            $data['parent_id']=$this->session->get('agentTotalId');
            $ret = AgentUserModel::insert($data);
        }else{
            $ret = AgentUserModel::where('id',$data['id'])->update($data);
        }
        if(!$ret){
            return $this->retMessage('error','操作失败');
        }
        return $data;
    }

    public function actionDel($data){
        /*$ids = explode(',', $data['id']);
        $ret = AgentUserModel::whereIn('id',$ids)->delete();
        if(!$ret){
            return $this->retMessage('error','操作失败');
        }*/
        return $this->retMessage('ok','操作成功');
    }

    public function actionaddMoneyVerify($data){
        $agentMoneyTotals = AgentMoneyTotalModel::where(['user_agent_id'=>$data['agentId'],'total_agent_id'=>$this->session->get('agentTotalId'),'money'=>$data['turnInMoney'],'type'=>0])->first();
        if(!empty($agentMoneyTotals) && (strtotime($agentMoneyTotals['created_at'])+60 >= time())){
            return $this->retMessage('error','一分钟内已有同样额度的转款 确定要继续操作吗？');
        }
        return $this->retMessage('ok','操作成功');
    }

    public function actionMoneyOpt($data){
        if(!isset($data['agentId'])||!isset($data['turnInMoney'])||!isset($data['telphoneCode'])){
            return $this->retMessage('error','参数错误');
        }
        if($data['telphoneCode']!="84512dbdb"){
            return $this->retMessage('error','验证码已失效!');
        }

        if($data['turnInMoney']>=0){
            return $this->addMoneyOpt($data);
        }else{
            return $this->deductMoneyOpt($data);
        }

    }

    //操作金币转账
    public function addMoneyOpt($data){

        $data['turnInMoney']=(int)abs($data['turnInMoney']);//只允许转正数

        //开始转款事务操作
        DB::beginTransaction();
        try {
            $totalAgentId=$this->session->get('agentTotalId');
            $agentTotalInfo = AgentTotalModel::where('id', $totalAgentId)->where('status','=','1')->first();
            /*$smsCodeVerify = $this->smsVerify($agentTotalInfo['telphone'],$data['telphoneCode']);
            if(!$smsCodeVerify){
                return $this->retMessage('error','验证码已失效!');
            }*/
            
            //查询余额
            if($agentTotalInfo['money'] < $data['turnInMoney']){
                throw new \Exception("很抱歉！你的余额不足!");
            }
            $agentUserInfo = AgentUserModel::select('money','total_recharge')->where('id', $data['agentId'])->where('status','=','1')->first();
            if(empty($agentUserInfo)){
                throw new \Exception("很抱歉！对方代理被禁用或者不存在!");
            }

            $ret = DB::table("agent_total")->where('id',$totalAgentId)->decrement("money",$data['turnInMoney']);
            if(!$ret){
                throw new \Exception("操作失败1");
            }
            $ret1 = DB::table("agent_user")->where('id',$data['agentId'])->increment('money',$data['turnInMoney']);
            $ret2 = DB::table("agent_user")->where('id',$data['agentId'])->increment('total_recharge',$data['turnInMoney']);
            $ret3 = DB::table("agent_total")->where('id',$totalAgentId)->increment('total_turn_out',$data['turnInMoney']);
            if(!$ret1 || !$ret2 || !$ret3){
                throw new \Exception("操作失败2");
            }
            $status = $ret?1:0;
            $ip = $this->getClientIP();
            $area = $this->getIPProCity($ip);
            $insertData = array(
                'agent_id'=>$data['agentId'],
                'type'=>2,
                'user_id'=>$totalAgentId,
                'money'=>$data['turnInMoney'],
                'new_money'=>$agentUserInfo['money']+$data['turnInMoney'],
                'ip'=>$ip,
                'area'=>$area,
                'created_at'=>date('Y-m-d H:i:s'),
                'status'=>$status,
                'remark'=>'总代理-转账-充值'
            );
            $ret = DB::table("agent_money_user")->insert($insertData);
            if(!$ret){
                throw new \Exception("操作失败3");
            }

            $insertData = array(
                'total_agent_id'=>$totalAgentId,
                'type'=>1,
                'user_agent_id'=>$data['agentId'],
                'money'=>$data['turnInMoney'],
                'old_money'=>$agentTotalInfo['money'],
                'new_money'=>$agentTotalInfo['money']-$data['turnInMoney'],
                'ip'=>$ip,
                'area'=>$area,
                'created_at'=>date('Y-m-d H:i:s'),
                'device'=>$this->isMobile()?2:1,
                'remark'=>'总代理-转账-转出' //代理向总代理拿货
            );
            $ret = DB::table("agent_money_total")->insert($insertData);
            if(!$ret){
                throw new \Exception("操作失败4");
            }
            DB::commit();
            return $this->retMessage('ok','操作成功');
        } catch (\Exception $e) {
            $this->log($e,"agentTurn.log");
            DB::rollBack();
            return $this->retMessage("fail",$e->getMessage());
        }
    }

    //操作金币转账
    public function deductMoneyOpt($data){

        $data['turnInMoney']=(int)abs($data['turnInMoney']);//只允许转正数

        //开始转款事务操作
        DB::beginTransaction();
        try {
            $totalAgentId=$this->session->get('agentTotalId');
            $agentTotalInfo = AgentTotalModel::where('id', $totalAgentId)->where('status','=','1')->first();
            /*$smsCodeVerify = $this->smsVerify($agentTotalInfo['telphone'],$data['telphoneCode']);
            if(!$smsCodeVerify){
                return $this->retMessage('error','验证码已失效!');
            }*/
            
            $agentUserInfo = AgentUserModel::select('money','total_recharge')->where('id', $data['agentId'])->where('status','=','1')->first();
            if(empty($agentUserInfo)){
                throw new \Exception("很抱歉！此代理被禁用或者不存在!");
            }

            //查询余额
            if($agentUserInfo['money'] < intval($data['turnInMoney'])){
                throw new \Exception("很抱歉！此代理余额不足扣除!");
            }

            $ret = DB::table("agent_total")->where('id',$totalAgentId)->increment("money",$data['turnInMoney']);
            if(!$ret){
                throw new \Exception("操作失败1");
            }
            $ret1 = DB::table("agent_user")->where('id',$data['agentId'])->decrement('money',$data['turnInMoney']);
            $ret2 = DB::table("agent_user")->where('id',$data['agentId'])->decrement('total_recharge',$data['turnInMoney']);
            $ret3 = DB::table("agent_total")->where('id',$totalAgentId)->decrement('total_turn_out',$data['turnInMoney']);
            if(!$ret1 || !$ret2 || !$ret3){
                throw new \Exception("操作失败2");
            }
            $status = $ret?1:0;
            $ip = $this->getClientIP();
            $area = $this->getIPProCity($ip);
            $insertData = array(
                'agent_id'=>$data['agentId'],
                'type'=>4,
                'user_id'=>$totalAgentId,
                'money'=>(-1) * $data['turnInMoney'],
                'new_money'=>$agentUserInfo['money']-$data['turnInMoney'],
                'ip'=>$ip,
                'area'=>$area,
                'created_at'=>date('Y-m-d H:i:s'),
                'status'=>$status,
                'remark'=>'总代理-转账-扣除'
            );
            $ret = DB::table("agent_money_user")->insert($insertData);
            if(!$ret){
                throw new \Exception("操作失败3");
            }

            $insertData = array(
                'total_agent_id'=>$totalAgentId,
                'type'=>0,
                'user_agent_id'=>$data['agentId'],
                'money'=>$data['turnInMoney'],
                'old_money'=>$agentTotalInfo['money'],
                'new_money'=>$agentTotalInfo['money']+$data['turnInMoney'],
                'ip'=>$ip,
                'area'=>$area,
                'created_at'=>date('Y-m-d H:i:s'),
                'device'=>$this->isMobile()?2:1,
                'remark'=>'总代理-转账-转入' //代理扣点
            );
            $ret = DB::table("agent_money_total")->insert($insertData);
            if(!$ret){
                throw new \Exception("操作失败4");
            }
            DB::commit();
            return $this->retMessage('ok','操作成功');
        } catch (\Exception $e) {
            $this->log($e,"agentTurn.log");
            DB::rollBack();
            return $this->retMessage("fail",$e->getMessage());
        }
    }
}
