<?php
namespace App\Controllers\Agentadmin\AgentAdmin;

use App\Controllers\Agentadmin\BaseController;
use App\Models\AgentApplyModel;
use App\Models\AgentUserModel;
use App\Models\InterfaceModel;
use App\Models\MailModel;
use App\Module\BjuiGridModule;
use Illuminate\Database\Capsule\Manager as DB;

class AgentApplyController extends BaseController
{

    public function actionList($data){
        $params = BjuiGridModule::searchFormatterParam($data);
        $params['pageSize']= isset($data['pageSize'])?$data['pageSize']:'';
        $params['pageCurrent'] = isset($data['pageCurrent'])?$data['pageCurrent']:'';

        $list = AgentApplyModel::select();
        $list = BjuiGridModule::searchWhere($params,$list);
        return $list;
    }
    
    public function actionEdit($data){
       /* if(empty($data['id'])){
            $ret = AgentApplyModel::insert($data);
        }else{
            $ret = AgentApplyModel::where('id',$data['id'])->update($data);
        }
        if(!$ret){
            return $this->retMessage('error','操作失败');
        }*/
        return $data;
    }

    public function actionDel($data){
        if(!empty($data) && !empty($data['id'])){
            $ids = explode(',', $data['id']);
            $ret = AgentApplyModel::whereIn('id',$ids)->delete();
        }else{
            return $this->retMessage('error','操作失败');
        }
        return $this->retMessage('ok','操作成功');
    }

    public function actionReplyOpt($data){
        if(!empty($data) && !empty($data['id'])){
            $ids = explode(',', $data['id']);
            $ret = AgentApplyModel::whereIn('id',$ids)->update(['reply'=>$data['reply']]);
            if($ret){
                $player=AgentApplyModel::whereIn('id',$ids)->pluck("player_id");
                foreach ($player as $id) {
                    //MailModel::insert(['player'=>$id,'content'=>$data['reply'],'title'=>$data['title'],'time'=>time()]);
                    $emailData = [$id, $data['title'], $data['reply'], 1];
                    $this->sendSysMail($emailData);
                    //InterfaceModel::red(0,$id);
                }
            }
        }else{
            return $this->retMessage('error','操作失败');
        }
        return $this->retMessage('ok','操作成功');
    }

    public function actionPassOpt($data){
        if(empty($data) && empty($data['id'])){
            return $this->retMessage('error','参数异常');
        }
        $applyId=$data['id'];
        DB::beginTransaction();
        try {
            $ret = DB::table("agent_apply")->where('id',$applyId)->where('status',0)->update(['status'=>1]);
            if(!$ret){
                throw new \Exception("操作失败3");
            }
            
            $ids=DB::table("agent_user")->pluck('id');
            $newid=rand(10000,99999);
            for($i=0;$i<5;$i++){
                if(in_array($newid,$ids)){
                    $newid=rand(10000,99999);
                    continue;
                }else{
                    break;
                }
            }
            $data['id']=$newid;
            $data['created_at'] = date('Y-m-d H:i:s');
            $password         = [REDACTED]"123456");
            $data['password'] = $password['password'];
            $data['encrypt']  = $password['encrypt'];
            if(empty($data['qq']))unset($data['qq']);
            $data['parent_id'] = $this->session->get('agentTotalId');
            $reply=$data['auto_reply'];unset($data['auto_reply']);
            $ret = DB::table("agent_user")->insert($data);
            if(!$ret){
                throw new \Exception("操作失败3");
            }
            AgentApplyModel::where('id',$applyId)->update(['reply'=>$reply]);
            //MailModel::insert(['player'=>$data['player_id'],'content'=>$reply,'title'=>'代理审核通知','time'=>time()]);
            $title = '代理审核通知';
            $content = $reply;
            $emailData = [$data['player_id'], $title, $content, 1];
            $this->sendSysMail($emailData);

            //InterfaceModel::red(0,$data['player_id']);
            DB::commit();
            return $this->retMessage('ok','操作成功');
        } catch (\Exception $e) {
            $this->log($e,"agentTurn.log");
            DB::rollBack();
            return $this->retMessage("fail",$e->getMessage());
        }

    }

    public function actionRefuseOpt($data){
        if(!empty($data) && !empty($data['id'])){
            $ids = explode(',', $data['id']);
            $ret = AgentApplyModel::whereIn('id',$ids)->where('status',0)->update(['status'=>2]);
        }else{
            return $this->retMessage('error','操作失败');
        }
        return $this->retMessage('ok','操作成功');
    }

}
