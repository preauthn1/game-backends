<?php
namespace App\Controllers\Agentadmin\AgentData;

use App\Controllers\Agentadmin\BaseController;
use App\Models\AgentUserModel;
use App\Module\BjuiGridModule;

class PlayerUnusualController extends BaseController
{

    public function actionList($data){
        $params = BjuiGridModule::searchFormatterParam($data);
        $params['pageSize']= isset($data['pageSize'])?$data['pageSize']:'';
        $params['pageCurrent'] = isset($data['pageCurrent'])?$data['pageCurrent']:'';

        $list = AgentUserModel::select();
        $list = BjuiGridModule::searchWhere($params,$list);
        return $list;
    }
    
    public function actionEdit($data){
        if(empty($data['id'])){
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
        $ids = explode(',', $data['id']);
        $ret = AgentUserModel::whereIn('id',$ids)->delete();
        if(!$ret){
            return $this->retMessage('error','操作失败');
        }
        return $this->retMessage('ok','操作成功');
    }

    public function actionAddMoney($data){
        var_dump($data);die;
    }
}
