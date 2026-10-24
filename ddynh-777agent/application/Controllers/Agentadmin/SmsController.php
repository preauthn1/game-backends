<?php
namespace App\Controllers\Agentadmin;

use App\Models\AgentTotalModel;

class SmsController extends BaseController
{

    protected static $expireTime = 300;

    public function actionCore(){
        $type = $this->request->getAttribute('type');
        $params = $this->request->getParsedBody();
        $ret = [];
        switch ($type) {
            case 'smsCodeLogin':
                $agentTotalInfo = AgentTotalModel::where('telphone', $params['telphone'])->where('status','=','1')->first();
                if(empty($agentTotalInfo)){
                    return $this->retBjuiData([],'error','无此手机号码相关信息');
                }
                $ret = $this->smsCodeSend($params['telphone']);
                break;
            case 'smsCodeAddMoney':
                $agentTotalInfo = AgentTotalModel::where('id', $this->session->get('agentTotalId'))->where('status','=','1')->first();
                if(empty($agentTotalInfo)){
                    return $this->retBjuiData([],'error','无此手机号码相关信息');
                }
                $ret = $this->smsCodeSend($agentTotalInfo['telphone']);
                break;
            case 'smsCodeVerify':
                $ret = $this->smsCodeVerify($params['telphone'],$params['code']);
                break;
            default:
                # code...
                break;
        }
        return $this->retBjuiData([],'ok','成功');;
    }

    //发送短信验证码
    protected function smsCodeSend($telphone){
        $ret = $this->smsSend($telphone);
        if($ret){
            $ret = $this->retMessage('ok','操作成功');
        }else{
            $ret = $this->retMessage('error','操作失败');
        }
        return $ret;
    }

    //发送短信
    protected function smsCodeVerify($telphone,$code){
        $ret = $this->smsVerify($telphone,$code);
        if($ret){
            $ret = $this->retMessage('ok','操作成功');
        }else{
            $ret = $this->retMessage('error','操作失败');
        }
        return $ret;
    }
}
