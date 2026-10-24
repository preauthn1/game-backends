<?php
namespace App\Controllers\Agentadmin\AgentGeneral;

use App\Controllers\Agentadmin\BaseController;
use App\Models\AgentTotalModel;

class ChangePasswordController extends BaseController
{

    public function actionList($data){
        return ;
    }
    
    public function actionEditOpt($data){
        return $data;
    }

    public function actionDel($data){
        return $this->retMessage('ok','操作成功');
    }

    public function actionChangeOpt($data){
        if (!$data['oldPassword'] || !$data['newPassword'] || !$data['repeatPassword'] || ($data['newPassword'] != $data['repeatPassword'])) {
            return $this->retMessage("fail","输入有误");
        }
        $checkInputPassword  = [REDACTED]'oldPassword']);
        $checkInputPassword1 = $this->checkPassword($data['newPassword']);
        if ($checkInputPassword && $checkInputPassword1) {
            $agentTotalId=$this->session->get("agentTotalId");
            $agentTotalInfo = AgentTotalModel::where("id",$agentTotalId)->first();
            $oldPassword = [REDACTED]'oldPassword'], $agentTotalInfo['encrypt']);
            if($agentTotalInfo['password']!=$oldPassword){
                return $this->retMessage("fail","密码错误");
            }
            $new['password'] =  $this->getPasswordEncrypt($data['newPassword'],$agentTotalInfo['encrypt']);
            $update =AgentTotalModel::where("id",$agentTotalId)->update($new);
            if ($update) {
                return $this->retMessage("ok","修改成功");
            }
            return $this->retMessage("fail","修改失败");
        } else {
            return $this->retMessage("fail","输入有误");
        }

    }
}
