<?php
namespace App\Controllers\Agentadmin\AgentAdmin;

use App\Controllers\Agentadmin\BaseController;
use App\Models\AgentAdModel;
use App\Models\AgentUserModel;
use App\Module\BjuiGridModule;

class AgentSearchController extends BaseController
{

    public function actionList($data){
        $params = BjuiGridModule::searchFormatterParam($data);
        $params['pageSize']= isset($data['pageSize'])?$data['pageSize']:'';
        $params['pageCurrent'] = isset($data['pageCurrent'])?$data['pageCurrent']:'';

        $list = AgentUserModel::where('parent_id', $this->session->get('agentTotalId'))->select();
        $list = BjuiGridModule::searchWhere($params,$list);
        return $list;
    }
    
    public function actionEditOpt($data){
        unset($data['money']);
        if(empty($data['id'])){
//            $ret = AgentUserModel::insert($data);
            return $this->retMessage('error','操作失败');
        }else{
            $ret = AgentUserModel::where('id',$data['id'])->where('parent_id', $this->session->get('agentTotalId'))->update($data);
        }
        if(!$ret){
            return $this->retMessage('error','操作失败');
        }
        return $data;
    }

    public function actionDel($data){
        $ids = (int)$data['id'];
        $res=AgentAdModel::where("agent_id",$ids)->delete();
        if($res){
            $ret = AgentUserModel::whereIn('id',$ids)->where('parent_id', $this->session->get('agentTotalId'))->delete();
            if(!$ret){
                return $this->retMessage('error','操作失败');
            }
        }
        return $this->retMessage('ok','操作成功');
    }

    //密码重置
    public function actionPassresetOpt($data){
        $ids = explode(',', $data['agent_id']);
        $agentInfo = AgentUserModel::whereIn("id",$ids)->get()->toArray();
        foreach ($agentInfo as $key => $value) {
            if(empty($value['encrypt'])){
                $updata = $this->getPasswordEncrypt($data['newPassword']);
            }else{
                $updata['password'] =  $this->getPasswordEncrypt($data['newPassword'],$value['encrypt']);
            }
            $update =AgentUserModel::where("id",$value['id'])->update($updata);
        }
        return $this->retMessage("ok","修改成功");
    }

    //清除注销登录
    public function actionClearLoginOpt($data){
        $ids = explode(',', $data['id']);
        $updataData = array(
            'session_id'=>'',
        );
        AgentUserModel::whereIn('id',$ids)->where('parent_id', $this->session->get('agentTotalId'))->update($updataData);
        return $this->retMessage('ok','操作成功');
    }

    public function actionAddMoney($data){
        var_dump($data);die;
    }
}
