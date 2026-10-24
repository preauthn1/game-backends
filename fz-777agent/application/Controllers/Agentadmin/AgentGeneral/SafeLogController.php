<?php
namespace App\Controllers\Agentadmin\AgentGeneral;

use App\Controllers\Agentadmin\BaseController;
use App\Models\AgentLoginTotalModel;
use App\Models\AgentLoginUserModel;
use App\Models\AgentTotalModel;
use App\Module\LoginModule;
use App\Module\BjuiGridModule;

class SafeLogController extends BaseController
{

    public function actionList($data){
        $params = BjuiGridModule::searchFormatterParam($data);
        $params['pageSize']= isset($data['pageSize'])?$data['pageSize']:'';
        $params['pageCurrent'] = isset($data['pageCurrent'])?$data['pageCurrent']:'';

        $list = AgentLoginTotalModel::where('agent_id',$this->session->get('agentTotalId'))->select();
        $list = BjuiGridModule::searchWhere($params,$list);
        return $list;
    }
    
    public function actionEdit($data){
        /*if(empty($data['id'])){
            $ret = AgentLoginUserModel::insert($data);
        }else{
            $ret = AgentLoginUserModel::where('id',$data['id'])->update($data);
        }
        if(!$ret){
            return $this->retMessage('error','操作失败');
        }*/
        return $data;
    }

    public function actionDel($data){
       /* $ids = explode(',', $data['id']);
        $ret = AgentLoginUserModel::whereIn('id',$ids)->delete();
        if(!$ret){
            return $this->retMessage('error','操作失败');
        }*/
        return $this->retMessage('ok','操作成功');
    }

    public function actionChangeTelphone($data){
        if(!isset($data['telphone']) || !isset($data['telphoneCode'])){
            return $this->retMessage('error','参数异常');
        }
        $agentTotalInfo = AgentTotalModel::where(['telphone'=>$data['telphone']])->where('status','=','1')->first();
        if(!empty($agentTotalInfo)){
            return $this->retMessage('error','此手机号码已经存在!');
        }
        $ret = AgentTotalModel::where('id',$this->session->get('agentTotalId'))->update(['telphone'=>$data['telphone']]);
        if(!$ret){
            return $this->retMessage('error','操作失败!');
        }
        return $this->retMessage('ok','操作成功');
    }

    public function actionBandIp($data){
        if(!isset($data['bandIp']) || !isset($data['bandIpSwitch'])){
            return $this->retMessage('error','参数异常');
        }
        $ret = AgentTotalModel::where('id',$this->session->get('agentTotalId'))->update(['ip'=>$data['bandIp'],'ip_bind'=>$data['bandIpSwitch']]);
        if(!$ret){
            return $this->retMessage('error','操作失败!');
        }
        return $this->retMessage('ok','操作成功');
    }

    public function actionTelphoneLoginSwitch($data){
        if(!isset($data['telphoneCodeSwitch'])){
            return $this->retMessage('error','参数异常');
        }
        $ret = AgentTotalModel::where('id',$this->session->get('agentTotalId'))->update(['telphone_login'=>$data['telphoneCodeSwitch']]);
        if(!$ret){
            return $this->retMessage('error','操作失败!');
        }
        return $this->retMessage('ok','操作成功');
    }

    public function actionChangePwd($data){
        if(empty($data['password']) || empty($data['passwordNew']) || empty($data['passwordNew2'])){
            return $this->retMessage('error','参数异常');
        }
        if($data['passwordNew'] !== $data['passwordNew2']){
            return $this->retMessage('error','两次新密码不相符');
        }
        $agentTotalInfo = AgentTotalModel::where('id',$this->session->get('agentTotalId'))->where('status','=','1')->first();
        $loginData['password'] = LoginModule::getPasswordEncrypt($data['password'], $agentTotalInfo['encrypt']);
        if($loginData['password'] != $agentTotalInfo['password']){
            return $this->retMessage('error','旧密码不正确!');
        }

        $passwordEncrypt = LoginModule::getPasswordEncrypt($data['passwordNew']);
        $ret = AgentTotalModel::where('id',$this->session->get('agentTotalId'))->update(['password'=>$passwordEncrypt['password'],'encrypt'=>$passwordEncrypt['encrypt']]);
        if(!$ret){
            return $this->retMessage('error','操作失败!');
        }
        return $this->retMessage('ok','操作成功');
    }

}
