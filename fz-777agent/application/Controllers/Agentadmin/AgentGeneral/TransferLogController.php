<?php
namespace App\Controllers\Agentadmin\AgentGeneral;

use App\Controllers\Agentadmin\BaseController;
use App\Module\BjuiGridModule;
use App\Models\AgentMoneyTotalModel;

class TransferLogController extends BaseController
{

    public function actionList($data){
        $params = BjuiGridModule::searchFormatterParam($data);
        $params['pageSize']= isset($data['pageSize'])?$data['pageSize']:'';
        $params['pageCurrent'] = isset($data['pageCurrent'])?$data['pageCurrent']:'';

        $list = AgentMoneyTotalModel::where('total_agent_id',$this->session->get('agentTotalId'))->select("id","total_agent_id","type","user_agent_id","money","old_money","new_money","created_at","device","remark");
        $list = BjuiGridModule::searchWhere($params,$list);
        return $list;
    }
    
    public function actionEdit($data){
        /*if(empty($data['id'])){
            $ret = AgentMoneyTotalModel::insert($data);
        }else{
            $ret = AgentMoneyTotalModel::where('id',$data['id'])->update($data);
        }
        if(!$ret){
            return $this->retMessage('error','操作失败');
        }*/
        return $data;
    }

    public function actionDel($data){
        /*$ids = explode(',', $data['id']);
        $ret = AgentMoneyTotalModel::whereIn('id',$ids)->delete();
        if(!$ret){
            return $this->retMessage('error','操作失败');
        }*/
        return $this->retMessage('ok','操作成功');
    }

    public function actionAddMoney($data){
        var_dump($data);die;
    }
}
