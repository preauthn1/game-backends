<?php
namespace App\Controllers\Agentadmin\AgentData;

use App\Controllers\Agentadmin\BaseController;
use App\Models\AgentMoneyUserModel;
use App\Models\AgentUserModel;
use App\Module\BjuiGridModule;

class AgentDetailController extends BaseController
{

    public function actionInitPageData($data){
        $AgentMoneyUser = AgentMoneyUserModel::selectRaw('SUM(agent_money_user.money) as total_money')->leftjoin('agent_user', 'agent_user.id', '=', 'agent_money_user.agent_id')->where('parent_id',$this->session->get('agentTotalId'))->first();
        $result['total_money'] = $AgentMoneyUser['total_money'];
        return $result;
    }

    public function actionList($data){
        $params = BjuiGridModule::searchFormatterParam($data);
        $params['pageSize']= isset($data['pageSize'])?$data['pageSize']:'';
        $params['pageCurrent'] = isset($data['pageCurrent'])?$data['pageCurrent']:'';
        $params['orders'] = isset($data['orders'])?$data['orders']:'agent_money_user.id desc';

        $list = AgentMoneyUserModel::selectRaw('agent_money_user.*,agent_user.name as agent_name,agent_user.channel')->leftjoin('agent_user', 'agent_user.id', '=', 'agent_money_user.agent_id')->where('parent_id',$this->session->get('agentTotalId'));
        $list = BjuiGridModule::searchWhere($params,$list);
        return $list;
    }
    
    public function actionEdit($data){
       /* if(empty($data['id'])){
            $ret = AgentUserModel::insert($data);
        }else{
            $ret = AgentUserModel::where('id',$data['id'])->update($data);
        }
        if(!$ret){
            return $this->retMessage('error','操作失败');
        }*/
        return $data;
    }

    public function actionDel($data){
       /* $ids = explode(',', $data['id']);
        $ret = AgentUserModel::whereIn('id',$ids)->delete();
        if(!$ret){
            return $this->retMessage('error','操作失败');
        }*/
        return $this->retMessage('ok','操作成功');
    }

    public function actionAddMoney($data){
        var_dump($data);die;
    }
}
