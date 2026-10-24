<?php
namespace App\Controllers\Agentadmin\Agent;

use App\Controllers\Agentadmin\BaseController;
use App\Models\AgentMoneyUserModel;
use App\Module\BjuiGridModule;

class TransactionDetailController extends BaseController
{

    public function actionList($data){
        $params = BjuiGridModule::searchFormatterParam($data);
        $params['pageSize']= isset($data['pageSize'])?$data['pageSize']:'';
        $params['pageCurrent'] = isset($data['pageCurrent'])?$data['pageCurrent']:'';

        $list = AgentMoneyUserModel::where("agent_id",$this->session->get("agentId"))->select("id","user_id","money","type","new_money","status","remark","created_at","area");
        $list = BjuiGridModule::searchWhere($params,$list);
        return $list;
    }
    
    public function actionEdit($data){
        return $data;
    }

    public function actionDel($data){
        return $this->retMessage('ok','操作成功');
    }

    public function actionAddMoney($data){
        var_dump($data);die;
    }
}
