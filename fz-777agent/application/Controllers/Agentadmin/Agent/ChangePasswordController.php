<?php
namespace App\Controllers\Agentadmin\Agent;

use App\Controllers\Agentadmin\BaseController;
use App\Models\AgentUserModel;

class ChangePasswordController extends BaseController
{

    public function actionList($data){
        return ;
    }
    
    public function actionEdit($data){
        return $data;
    }

    public function actionDel($data){
        return $this->retMessage('ok','操作成功');
    }

    public function actionChange($data){
        if (!$data['oldPassword'] || !$data['newPassword'] || !$data['repeatPassword'] || !$data['account'] || ($data['newPassword'] != $data['repeatPassword'])|| ($data['account'] != $this->session->get("agentId"))) {
            return $this->retMessage("fail","输入有误");
        }
        $checkInputPassword  = [REDACTED]'oldPassword']);
        $checkInputPassword1 = $this->checkPassword($data['newPassword']);
        if ($checkInputPassword && $checkInputPassword1) {
            $agentId=$this->session->get("agentId");
            $agentInfo = AgentUserModel::where("id",$agentId)->first();
            $oldPassword = [REDACTED]'oldPassword'], $agentInfo->encrypt);
            if($agentInfo->password!=$oldPassword){
                return $this->retMessage("fail","密码错误");
            }
            $new['password'] =  $this->getPasswordEncrypt($data['newPassword'],$agentInfo->encrypt);
            $update =AgentUserModel::where("id",$agentId)->update($new);
            if ($update) {
                return $this->retMessage("ok","修改成功");
            }
            return $this->retMessage("fail","修改失败");
        } else {
            return $this->retMessage("fail","输入有误");
        }

    }
}
