<?php
namespace App\Controllers\Agentadmin;

use App\Models\AgentUserModel;
use App\Module\BjuiGridModule;

class AgentManageController extends BaseController
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
            $ids=AgentUserModel::pluck('id')->toArray();
            $newid=rand(10000,99999);
            for($i=0;$i<10;$i++){
                if(in_array($newid,$ids)){
                    $newid=rand(10000,99999);
                    continue;
                }else{
                    break;
                }
            }
            $data['id']=$newid;
            $data['created_at'] = date('Y-m-d H:i:s');
            $data['parent_id']=$this->session->get('agentTotalId');
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
