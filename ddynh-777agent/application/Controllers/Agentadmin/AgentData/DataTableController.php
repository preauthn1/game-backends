<?php
namespace App\Controllers\Agentadmin\AgentData;

use App\Controllers\Agentadmin\BaseController;
use App\Models\AgentReportDayModel;
use App\Models\AgentUserModel;
use App\Module\BjuiGridModule;

class DataTableController extends BaseController
{

    public function actionInitPageData($data){
        $agentReportDayInfo = AgentReportDayModel::leftJoin('agent_user', 'agent_user.id', '=', 'agent_report_day.agent_id')->where('parent_id',$this->session->get('agentTotalId'))->selectRaw('SUM(sale) as total_sale')->first();
        $result['total_sale'] = $agentReportDayInfo['total_sale'];
        return $result;
    }

    public function actionList($data){
        $params = BjuiGridModule::searchFormatterParam($data);
        $params['pageSize']= isset($data['pageSize'])?$data['pageSize']:'';
        $params['pageCurrent'] = isset($data['pageCurrent'])?$data['pageCurrent']:'';

        $list = AgentReportDayModel::leftJoin('agent_user', 'agent_user.id', '=', 'agent_report_day.agent_id')->where('parent_id',$this->session->get('agentTotalId'))->selectRaw('agent_report_day.*,agent_user.name,agent_user.channel,agent_user.remark');
        $list = BjuiGridModule::searchWhere($params,$list);
        return $list;
    }
    
    public function actionEdit($data){
        /*if(empty($data['id'])){
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
      /*  $ids = explode(',', $data['id']);
        $ret = AgentUserModel::whereIn('id',$ids)->delete();
        if(!$ret){
            return $this->retMessage('error','操作失败');
        }*/
        return $this->retMessage('ok','操作成功');
    }
}
