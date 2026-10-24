<?php
namespace App\Controllers;

use App\Models\GameServerListModel;
use App\Models\PerGameDetailLogModel;
use App\Models\PerGameLogModel;
use App\Models\UsersModel;
use PHPImageWorkshop\ImageWorkshop;
use App\Libraries\Url;
use App\Libraries\Qr;
use EasyWeChat\Message\Image;

class IndexController extends BaseController
{

    public function actionIndex()
    {
        exit('');
    }

    public function actionWelcome()
    {
        exit('');
    }

    public function actionWechat()
    {
//         $message = $this->container['wechat']->server->getMessage();
        $this->container['wechat']->server->setMessageHandler(function ($message) {
            // $message->FromUserName // 用户的 openid
            // $message->MsgType // 消息类型：event, text....
            switch ($message->MsgType) {
                case 'event':
                    if($message->Event=="subscribe"){
                        $userData       = $this->container['db']->table('wechat')->where('openid', $message->FromUserName)->first();
                        $subinfo=$this->container['wechat']->user->get($message->FromUserName);
                        $wechatData=array(
                            'subscribe'=>$subinfo->subscribe,'nickname'=>$subinfo->nickname,'sex'=>$subinfo->sex,
                            'city'=>$subinfo->city,'country'=>$subinfo->country,'province'=>$subinfo->province,
                            'headimgurl'=>$subinfo->headimgurl,'subscribe_time'=>$subinfo->subscribe_time
                        );
                        if($userData){
                            $this->container['db']->table('wechat')->where('openid', $subinfo->openid)->update($wechatData);
                        }else{
                            $wechatData['openid']=$subinfo->openid;
                            $wechatData['created_at']=\Carbon\Carbon::now();
                            $eventkey=(!empty($message->EventKey))?str_replace("qrscene_","",$message->EventKey):0;
                            if($eventkey){//关注前扫描
                            	$wechatData['scene_id'] = $eventkey;
                            	$wechatInfo  = $this->container['db']->table('users')->where('wechat_openid', $subinfo->openid)->first();
                            	if($wechatInfo){
                            		$this->container['db']->table('users')->where('wechat_openid', $subinfo->openid)->update(array('inviteuid'=>$eventkey));
                            	}
                            }
                            $this->container['db']->table('wechat')->insert($wechatData);
                        }
                        return '文本消息';
                    }else if($message->Event=="unsubscribe"){
                        $this->container['db']->table('wechat')->where('openid', $message->FromUserName)->update(array('subscribe'=>0));
                    }else if($message->Event=="SCAN"){//已关注的扫描暂不算数
                    	//return '扫描的ID是'.$message->EventKey.'<br/>ticket值是'.$message->Ticket;
                    }else if($message->Event=="CLICK"){
                        if($message->EventKey=="EXTEND"){
                            $userData       = $this->container['db']->table('users')->where('wechat_openid', $message->FromUserName)->first();
                            return '文本消息';
                        }else if($message->EventKey=="INFO"){
                        	return new Image(['media_id' => 'Bx8dQQ1DHzjFubjy1jwKzeFayfGxRLrCgE_9WOxseVM']);
                        }
                    }
                    return '';
                    break;
                /* case 'text':
                    return '收到文字消息';
                    break;
                case 'image':
                    return '收到图片消息';
                    break;
                case 'voice':
                    return '收到语音消息';
                    break;
                case 'video':
                    return '收到视频消息';
                    break;
                case 'location':
                    return '收到坐标消息';
                    break;
                case 'link':
                    return '收到链接消息';
                    break; */
                    // ... 其它消息
                default:
                    return '文本消息';
                    break;
            }
        });
        $this->container['wechat']->server->serve()->send();
    }

    public function actionLogin()
    {
        $this->baseAction();
        if ($this->request->isPost()) {
            return $this->massage('用户名或密码错误', SELF_URL, 3);
        } else {
            $thisViewArgs = [
                'title'       => '登陆 - 后台管理中心',
                'page_header' => '登陆',
            ];
            return $this->render('admin/login.html', $thisViewArgs);
        }
    }
}
