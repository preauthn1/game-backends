<?php
namespace App\Controllers\Agentadmin\AgentAdmin;

use App\Controllers\Agentadmin\BaseController;
use App\Models\AgentUserModel;
use App\Models\GamePlayerModel;
use App\Models\InterfaceModel;
use App\Models\MailModel;
use App\Module\BjuiGridModule;

class AgentAddController extends BaseController
{

    public function actionList($data){
       /* $params = BjuiGridModule::searchFormatterParam($data);
        $params['pageSize']= isset($data['pageSize'])?$data['pageSize']:'';
        $params['pageCurrent'] = isset($data['pageCurrent'])?$data['pageCurrent']:'';

        $list = AgentUserModel::select();
        $list = BjuiGridModule::searchWhere($params,$list);
        return $list;*/
    }
    
    public function actionEdit($data){
        $playInfo = GamePlayerModel::where('UserID',$data['player_id'])->first();
        if(empty($playInfo)){
            return $this->retMessage('error','不存在此游戏ID');
        }
        /*$tel=AgentUserModel::where("telphone",$data['telphone'])->count();
        if($tel>0){
            return $this->retMessage('error','手机号已经存在');
        }*/
        $ids=AgentUserModel::pluck('id')->toArray();
        $newid=rand(10000,99999);
        for($i=0;$i<5;$i++){
            if(in_array($newid,$ids)){
                $newid=rand(10000,99999);
                continue;
            }else{
                break;
            }
        }
        $data['id']=$newid;$reply=$data['auto_reply'];unset($data['auto_reply']);
        $data['created_at'] = date('Y-m-d H:i:s');
        $password         = [REDACTED]"123456");
        $data['password'] = $password['password'];
        $data['encrypt']  = $password['encrypt'];
        if(empty($data['qq']))unset($data['qq']);
        $data['parent_id']=$this->session->get('agentTotalId');
        $ret = AgentUserModel::insert($data);
        if(!$ret){
            return $this->retMessage('error','操作失败');
        }
        //XXX 要修改agent_apply里面的reply字段
        //MailModel::insert(['player'=>$data['player_id'],'content'=>$reply,'title'=>'代理审核通知','time'=>time()]);
        $title = '代理审核通知';
        $content = $reply;
        $emailData = [$data['player_id'], $title, $content, 1];
        $this->sendSysMail($emailData);

        //InterfaceModel::red(0,$data['player_id']);
        return $this->retMessage('ok','操作成功');;
    }

    public function actionDel($data){
       /* $ids = explode(',', $data['id']);
        $ret = AgentUserModel::whereIn('id',$ids)->delete();
        if(!$ret){
            return $this->retMessage('error','操作失败');
        }*/
        return $this->retMessage('ok','操作成功');
    }

    public function actionAddMoney($data){
        var_dump($data);die;
    }
}
