<?php
namespace App\Controllers\Agentadmin\AgentAdmin;

use App\Controllers\Agentadmin\BaseController;
use App\Models\AgentLoginUserModel;
use App\Module\BjuiGridModule;

class AgentLoginlogController extends BaseController
{

    public function actionList($data){
        $params = BjuiGridModule::searchFormatterParam($data);
        $params['pageSize']= isset($data['pageSize'])?$data['pageSize']:'';
        $params['pageCurrent'] = isset($data['pageCurrent'])?$data['pageCurrent']:'';

        $list = AgentLoginUserModel::selectRaw('agent_login_user.*,agent_user.parent_id')->leftjoin('agent_user', 'agent_user.id', '=', 'agent_login_user.agent_id')->where('parent_id', $this->session->get('agentTotalId'));
        $list = BjuiGridModule::searchWhere($params,$list);
        return $list;
    }
    
    public function actionEditOpt($data){
        unset($data['money']);
        if(empty($data['id'])){
//            $ret = AgentLoginUserModel::insert($data);
            return $this->retMessage('error','操作失败');
        }else{
            $ret = AgentLoginUserModel::where('id',$data['id'])->where('parent_id', $this->session->get('agentTotalId'))->update($data);
        }
        if(!$ret){
            return $this->retMessage('error','操作失败');
        }
        return $data;
    }

    public function actionDel($data){
        $ids = explode(',', $data['id']);
        $ret = AgentLoginUserModel::whereIn('id',$ids)->where('parent_id', $this->session->get('agentTotalId'))->delete();
        if(!$ret){
            return $this->retMessage('error','操作失败');
        }
        return $this->retMessage('ok','操作成功');
    }

    public function actionClearLoginOpt($data){
        $ids = explode(',', $data['id']);
        $updataData = array(
            'session_id'=>'',
        );
        $ret = AgentLoginUserModel::whereIn('id',$ids)->where('parent_id', $this->session->get('agentTotalId'))->update($updataData);
        if(!$ret){
            return $this->retMessage('error','操作失败');
        }
        return $this->retMessage('ok','操作成功');
    }

    public function actionAddMoney($data){
        var_dump($data);die;
    }
}
