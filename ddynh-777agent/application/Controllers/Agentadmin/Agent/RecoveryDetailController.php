<?php
namespace App\Controllers\Agentadmin\Agent;

use App\Controllers\Agentadmin\BaseController;
use App\Models\AgentExchangeBack;
use App\Module\BjuiGridModule;

class RecoveryDetailController extends BaseController
{

    public function actionList($data){
        $params = BjuiGridModule::searchFormatterParam($data);
        $params['pageSize']= isset($data['pageSize'])?$data['pageSize']:'';
        $params['pageCurrent'] = isset($data['pageCurrent'])?$data['pageCurrent']:'';

        $list = AgentExchangeBack::leftJoin("agent_exchange_player",'agent_exchange_player.id','=','agent_exchange_back.exchange_id')
            ->where("agent_id",$this->session->get("agentId"))->selectRaw("agent_exchange_back.*,
        if(agent_exchange_back.`status`=1,`code`,'') as code,
        if(agent_exchange_back.`status`=1,player_id,'') as player_id,
        if(agent_exchange_back.`status`=1,money,'') as money,
        if(agent_exchange_back.`status`=1,fee,'') as fee,
        if(agent_exchange_back.`status`=1,alipay_account,'') as alipay_account,
        if(agent_exchange_back.`status`=1,alipay_name,'') as alipay_name,
        if(agent_exchange_back.`status`=1,bank_account,'') as bank_account,
        if(agent_exchange_back.`status`=1,bank_name,'') as bank_name,
        if(agent_exchange_back.`status`=1,bank_affiliate,'') as bank_affiliate");
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
