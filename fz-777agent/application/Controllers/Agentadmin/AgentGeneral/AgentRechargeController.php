<?php
namespace App\Controllers\Agentadmin\AgentGeneral;

use App\Controllers\Agentadmin\BaseController;
use App\Models\AgentReportDayModel;
use App\Module\BjuiGridModule;

class AgentRechargeController extends BaseController
{
    //商人充值
    public function actionList($data){
        $params = BjuiGridModule::searchFormatterParam($data);
        $params['pageSize']= isset($data['pageSize'])?$data['pageSize']:'';
        $params['pageCurrent'] = isset($data['pageCurrent'])?$data['pageCurrent']:'';

        $list = AgentReportDayModel::leftjoin('agent_user', 'agent_user.id', '=', 'agent_report_day.agent_id');
        $list = $list->selectRaw("agent_user.name as agent_name,agent_report_day.*")->where('parent_id',$this->session->get('agentTotalId'));
        $list = BjuiGridModule::searchWhere($params,$list);
        return $list;
    }

    public function actionEdit($data){
        /*if(empty($data['id'])){
            $ret = AgentReportDayModel::insert($data);
        }else{
            $ret = AgentReportDayModel::where('id',$data['id'])->update($data);
        }
        if(!$ret){
            return $this->retMessage('error','操作失败');
        }*/
        return $data;
    }

    public function actionDel($data){
       /* $ids = explode(',', $data['id']);
        $ret = AgentReportDayModel::whereIn('id',$ids)->delete();
        if(!$ret){
            return $this->retMessage('error','操作失败');
        }*/
        return $this->retMessage('ok','操作成功');
    }

    public function actionAddMoney($data){
        var_dump($data);die;
    }
}
