<?php
namespace App\Controllers\Agentadmin\AgentGeneral;

use App\Controllers\Agentadmin\BaseController;
use App\Models\AgentRechargeTotalModel;
use App\Models\AgentTotalModel;
use App\Models\AgentUserModel;
use App\Module\BjuiGridModule;

class AddMoneyController extends BaseController
{

    public function actionInitPageData($data){
        $agentTotalInfo = AgentTotalModel::where('id', $this->session->get('agentTotalId'))->where('status','=','1')->first();
//        $agentUserInfo = AgentUserModel::selectRaw('SUM(total_turn_in) as total_turn_in,SUM(total_recharge) as total_recharge,SUM(total_turn_out) as total_turn_out')->where('parent_id', $this->session->get('agentTotalId'))->first();
        $result['total_money'] = $agentTotalInfo['money'];
        $result['total_turn_out'] = $agentTotalInfo['total_turn_out'];
        $result['total_recharge'] = $agentTotalInfo['total_recharge'];
        return $result;
    }

    public function actionList($data){
        $params = BjuiGridModule::searchFormatterParam($data);
        $params['pageSize']= isset($data['pageSize'])?$data['pageSize']:'';
        $params['pageCurrent'] = isset($data['pageCurrent'])?$data['pageCurrent']:'';

        $list = AgentRechargeTotalModel::leftjoin('agent_total', 'agent_total.id', '=', 'agent_recharge_total.total_agent_id');
        $list = $list->selectRaw("agent_total.username,agent_recharge_total.*")->where('total_agent_id',$this->session->get('agentTotalId'));
        $list = BjuiGridModule::searchWhere($params,$list);
        return $list;
    }
    
    public function actionEdit($data){
       /* if(empty($data['id'])){
            $ret = AgentRechargeTotalModel::insert($data);
        }else{
            $ret = AgentRechargeTotalModel::where('id',$data['id'])->update($data);
        }
        if(!$ret){
            return $this->retMessage('error','操作失败');
        }*/
        return $data;
    }

    public function actionDel($data){
       /* $ids = explode(',', $data['id']);
        $ret = AgentRechargeTotalModel::whereIn('id',$ids)->delete();
        if(!$ret){
            return $this->retMessage('error','操作失败');
        }*/
        return $this->retMessage('ok','操作成功');
    }

    public function actionAddMoney($data){
        var_dump($data);die;
    }
}
