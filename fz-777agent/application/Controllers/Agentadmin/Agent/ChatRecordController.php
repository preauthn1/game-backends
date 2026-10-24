<?php
namespace App\Controllers\Agentadmin\Agent;

use App\Controllers\Agentadmin\BaseController;
use App\Models\AgentChatModel;
use App\Models\AgentRedModel;
use App\Models\AgentUserModel;
use App\Models\InterfaceModel;
use App\Module\BjuiGridModule;

class ChatRecordController extends BaseController
{
    public function actionInitPageData($data){
        $autoReply= AgentUserModel::where("id",$this->session->get("agentId"))->value("auto_reply");
        return array("autoReply"=>$autoReply);
    }

    public function actionList($data){
        $params = BjuiGridModule::searchFormatterParam($data);
        $params['pageSize']= isset($data['pageSize'])?$data['pageSize']:'';
        $params['pageCurrent'] = isset($data['pageCurrent'])?$data['pageCurrent']:'';

        $list = AgentChatModel::where("agent_id",$this->session->get("agentId"))->select();
        $list = BjuiGridModule::searchWhere($params,$list);
        return $list;
    }
    
    public function actionEdit($data){
        return $data;
    }

    public function actionDel($data){
        if(isset($data['id'])){
            $res = AgentChatModel::where('agent_id',$this->session->get("agentId"))->where('id',$data['id'])->delete();
            if($res){
                return $this->retMessage('ok','操作成功');
            }
        }
        return $this->retMessage('error','操作失败');
    }

    public function actionReplyOpt($data){
        if(isset($data['id'])&&isset($data['reply'])){
            $res = AgentChatModel::where('agent_id',$this->session->get("agentId"))->where('id',$data['id'])->update(['reply'=>$data['reply']]);
            if($res){
                $playerId=AgentChatModel::where('id',$data['id'])->value("player_id");
                if(!AgentRedModel::where('red_agent_id',$this->session->get("agentId"))->where('red_player_id',$playerId)->count()){
                    AgentRedModel::insert(['red_agent_id'=>$this->session->get("agentId"),'red_player_id'=>$playerId]);
                }
                InterfaceModel::agentReply($playerId,$this->session->get("agentId"));
                return $this->retMessage('ok','操作成功');
            }
        }
        return $this->retMessage('error','操作失败');
    }
    public function actionAutoOpt($data){
        if(isset($data['autoReply'])){
            $res = AgentUserModel::where('id',$this->session->get("agentId"))->update(['auto_reply'=>$data['autoReply']]);
            if($res){
                return $this->retMessage('ok','操作成功');
            }
        }
        return $this->retMessage('error','操作失败');
    }
}
