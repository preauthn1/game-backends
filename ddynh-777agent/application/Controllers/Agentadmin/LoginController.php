<?php
namespace App\Controllers\Agentadmin;

use App\Models\AgentLoginTotalModel;
use App\Models\AgentLoginUserModel;
use App\Module\LoginModule;
use App\Models\AgentUserModel;
use App\Models\AgentTotalModel;
use App\Libraries\GoogleAuthenticator;

class LoginController extends BaseController
{
    private $cookieExpire = SYS_TIME + 86400 * 30;

    //代理商登录
    public function actionAgentAdminLogin()
    {
        $ga = new GoogleAuthenticator();
        
        if ($this->request->isPost()) {
            $loginData = $this->request->getParsedBody();

			if (!empty($loginData['gotp_seckey']) && $loginData['gotp_seckey'] != "") {
				$loginData = $this->request->getParsedBody();
				//echo $loginData['gotp_seckey'];
				AgentUserModel::where('id',$loginData['logininfo_id'])->update(array('secretkey'=>$loginData['gotp_seckey']));
				$thisViewArgs = [
					'title'       => '登陆 - 代理商后台管理中心',
					'page_header' => '登陆',
				];
				return $this->render('agentadmin/login.html', $thisViewArgs);
			}

            $loginType = LoginModule::getLoginType($loginData['username']);
            $checkInputLoginName = LoginModule::checkInput('username', $loginData['username']);
            $checkInputPassword  = [REDACTED]'password', $loginData['password']);
            $checkInputGotp  = $loginData['gotp'];

            if ($checkInputLoginName && $checkInputPassword) {
                $loginType = 'id';
                $loginInfo = AgentUserModel::where($loginType, '=', $loginData['username'])->first();
                if(!empty($loginInfo['session_id']) && $loginInfo['session_id']!=session_id()){//限制一台设备登录,灰度测试
                    $sessContent = $this->getSessionContent($loginInfo['session_id']);
                    if(!empty($sessContent)){
                        return $this->massage('登录失败,此账号已经在其他设备登录！', $this->router->pathFor('agentAdminLogin'), 3);
                    }
                }
                if (isset($loginInfo)) {
                    $loginInfo = $loginInfo->toArray();
                    $loginData['password'] = LoginModule::getPasswordEncrypt($loginData['password'], $loginInfo['encrypt']);
                    $checkLoginByInput = AgentUserModel::where($loginType, '=', $loginData['username'])->where('password', $loginData['password'])->first();
                    if (!isset($checkLoginByInput)) {
                        self::loginLogUser("密码错误",$loginInfo['id']);
                        return $this->massage('用户名或密码错误或验证码錯誤', $this->router->pathFor('agentAdminLogin'), 3);
                    } else if ($loginInfo["secretkey"] === "0") { //需要註冊谷歌驗證器
                        $secretkey = $ga->createSecret();
                        //AgentUserModel::where('id',$loginInfo['id'])->update(array('secretkey'=>$secretkey));
                        $thisViewArgs = [
                            'title'       => '登陆 - 代理商后台管理中心',
                            'page_header' => '登陆',
                            'gotp_qrcode' => '1',
							'gotp_seckey' => $secretkey,
							'logininfo_id' => $loginInfo['id'],
                            'gotp_qrcodeurl' => $ga->getQRCodeGoogleUrl('FZAgent', $secretkey)
                        ];
                        return $this->render('agentadmin/login.html', $thisViewArgs);
                    } else { //己經有註冊谷歌驗證器
                        $checkGotp = $ga->verifyCode($loginInfo["secretkey"], $checkInputGotp, 2);
                        if (!$checkGotp) {
                            self::loginLogUser("谷歌验证码错误",$loginInfo['id']);
                            return $this->massage('用户名或密码错误或验证码錯誤', $this->router->pathFor('agentAdminLogin'), 3);
                        }
                        $checkLoginByInput = $checkLoginByInput->toArray();
                        if (isset($loginData['rememberMe'])) {
                            $cookieExpire = SYS_TIME + 86400 * 30;
                        } else {
                            $cookieExpire = 0;
                        }
                        self::loginLogUser("登录成功",$loginInfo['id']);
//                        $this->cookie->set('agentId', $checkLoginByInput['id'], $cookieExpire);
                        $this->session->set('agentId', $checkLoginByInput['id']);
                        $this->session->set('agentName', $checkLoginByInput['name']);
                        $this->session->set('agentInfo', $loginInfo);

//                        $this->cookie->set('agentTotalId', '');
//                        $this->cookie->set('agentTotalName', '');
                        $this->session->set('agentTotalId', '');
                        $this->session->set('agentTotalName', '');
                        $this->session->set('agentTotalInfo', '');
                        AgentUserModel::where('id',$loginInfo['id'])->update(array('session_id'=>session_id()));
                        return $this->massage('登陆成功', '/agentadmin/page/main', 0);
                    }
                } else {
                    return $this->massage('用户名密码错误', $this->router->pathFor('agentAdminLogin'), 3);
                }
            } else {
                return $this->massage('用户密码错误', $this->router->pathFor('agentAdminLogin'), 3);
            }
        } else {
            $thisViewArgs = [
                'title'       => '登陆 - 代理商后台管理中心',
                'page_header' => '登陆',
            ];
            return $this->render('agentadmin/login.html', $thisViewArgs);
        }
    }


     //代理商登出
    public function actionAgentAdminLogout()
    {
        self::loginLogUser("注销退出",$this->session->get('agentId'));
        session_destroy();
        return $this->massage('退出成功', $this->router->pathFor('agentAdminLogin'), 0);
    }



    //总代理登录入口
    public function actionAgentTotalAdminLogin(){
        if ($this->request->isPost()) {
            $loginData = $this->request->getParsedBody();


            if (!empty($loginData['opt']) && $loginData['opt']=='bandTelphone') {
                return $this->actionAgentTotalBandPhone($loginData);
            }else if (!empty($loginData['opt']) && $loginData['opt']=='telphoneLogin') {
                return $this->actionAgentTotalPhoneLogin($loginData);
            }
			else if (!empty($loginData['gotp_seckey']) && $loginData['gotp_seckey'] != "") {
				//echo $loginData['gotp_seckey'];
				AgentTotalModel::where('id',$loginData['logininfo_id'])->update(array('secretkey'=>$loginData['gotp_seckey']));

			}
			else {

                return $this->actionAgentTotalLogin();
            }
        }

        $thisViewArgs = [
            'title'       => '登陆 - 代理商后台管理中心',
            'page_header' => '登陆',
        ];
        return $this->render('agentadmin/agenttotal_login.html', $thisViewArgs);
    }

    protected function actionAgentTotalBandPhone($loginData){
        $checkLoginByInput = AgentTotalModel::where('username', '=', $loginData['username'])->where('password', $loginData['password'])->first();
        if(empty($checkLoginByInput)){
             return $this->retBjuiData([],'error','账号或者密码不对');
        }
        if(!empty($checkLoginByInput['telphone'])){
            return $this->retBjuiData([],'error','此账号已被其他手机绑定');
        }
        $checkLoginByInput = AgentTotalModel::where('telphone', '=', $loginData['telphone'])->first();
        if(!empty($checkLoginByInput)){
            return $this->retBjuiData([],'error','此手机已被绑定');
        }
    }

    //总代理商账号密码登录
    public function actionAgentTotalLogin()
    {
        $ga = new GoogleAuthenticator();

        if ($this->request->isPost()) {
            $loginData = $this->request->getParsedBody();
            $loginType = LoginModule::getLoginType($loginData['username']);
            $checkInputLoginName = LoginModule::checkInput('username', $loginData['username']);
            $checkInputPassword  = [REDACTED]'password', $loginData['password']);
            $checkInputGotp  = $loginData['gotp'];
            if ($checkInputLoginName && $checkInputPassword) {
                $loginInfo = AgentTotalModel::where($loginType, '=', $loginData['username'])->first();
                if (isset($loginInfo)) {
                    $loginInfo = $loginInfo->toArray();
                    $loginData['password'] = LoginModule::getPasswordEncrypt($loginData['password'], $loginInfo['encrypt']);
                    $checkLoginByInput = AgentTotalModel::where($loginType, '=', $loginData['username'])->where('password', $loginData['password'])->first();
                    if (!isset($checkLoginByInput)) {
                        self::loginLogTotal("密码错误",$loginInfo['id']);
                        return $this->massage('用户名或密码错误', $this->router->pathFor('agentTotalAdminLogin'), 3);
                    } else if ($loginInfo["secretkey"] === "0") { //需要註冊谷歌驗證器
                        $secretkey = $ga->createSecret();
                        //AgentTotalModel::where('id',$loginInfo['id'])->update(array('secretkey'=>$secretkey));
                        $thisViewArgs = [
                            'title'       => '登陆 - 代理商后台管理中心',
                            'page_header' => '登陆',
                            'gotp_qrcode' => '1',
							'gotp_seckey' => $secretkey,
							'logininfo_id' => $loginInfo['id'],
                            'gotp_qrcodeurl' => $ga->getQRCodeGoogleUrl('FZTotalAgent', $secretkey)
                        ];
                        return $this->render('agentadmin/agenttotal_login.html', $thisViewArgs);
                    
                    } else {  //己經有註冊谷歌驗證器
                        $checkGotp = $ga->verifyCode($loginInfo["secretkey"], $checkInputGotp, 2);
                        if (!$checkGotp) {
                            self::loginLogUser("谷歌验证码错误",$loginInfo['id']);
                            return $this->massage('用户名或密码错误或验证码錯誤', $this->router->pathFor('agentTotalAdminLogin'), 3);
                        }
                        self::loginLogTotal("登录成功",$loginInfo['id']);
                        $checkLoginByInput = $checkLoginByInput->toArray();
                        $cookieExpire = isset($loginData['rememberMe']) ? SYS_TIME + 86400 * 30 : 0;

//                        $this->cookie->set('agentTotalId', $checkLoginByInput['id'], $cookieExpire);
//                        $this->cookie->set('agentTotalName', $checkLoginByInput['username'], $cookieExpire);
                        $this->session->set('agentTotalId', $checkLoginByInput['id']);
                        $this->session->set('agentTotalName', $checkLoginByInput['username']);
                        $this->session->set('agentTotalInfo', $loginInfo);

//                        $this->cookie->set('agentId', '');
                        $this->session->set('agentId', '');
                        $this->session->set('agentName', '');
                        $this->session->set('agentInfo', '');
        
                        return $this->massage('登陆成功', '/agentadmin/page/main', 0);
                    }
                } else {
                    return $this->massage('用户名密码错误', $this->router->pathFor('agentTotalAdminLogin'), 3);
                }
            } else {
                return $this->massage('用户密码错误', $this->router->pathFor('agentTotalAdminLogin'), 3);
            }
        }
    }

    //总代理商手机短信验证登录
    public function actionAgentTotalPhoneLogin()
    {
        if ($this->request->isPost()) {
            $loginData = $this->request->getParsedBody();
            $loginType = LoginModule::getLoginType($loginData['telphone']);
            if($loginType != 'telphone'){
                return $this->retBjuiData([],'error','手机号码格式不对');
            }

            $ret = $this->smsVerify($loginData['telphone'],$loginData['smsCode']);
            if(!$ret){
                $totalId=AgentTotalModel::where("telphone",$loginData['telphone'])->first();
                if($totalId){
                    self::loginLogTotal("手机验证码错误",$totalId->id);
                }
                return $this->retBjuiData([],'error','验证码已失效');
            }

            $loginInfo = AgentTotalModel::where($loginType, '=', $loginData['telphone'])->where('telphone_login','=','1')->first();
            if (empty($loginInfo)) {
                return $this->retBjuiData([],'error','登录异常');
            }
            $checkLoginByInput = $loginInfo->toArray();
            $cookieExpire = isset($loginData['rememberMe']) ? SYS_TIME + 86400 * 30 : 0;

            self::loginLogTotal("手机登录成功",$checkLoginByInput['id']);
//            $this->cookie->set('agentTotalId', $checkLoginByInput['id'], $cookieExpire);
//            $this->cookie->set('agentTotalName', $checkLoginByInput['username'], $cookieExpire);
            $this->session->set('agentTotalId', $checkLoginByInput['id']);
            $this->session->set('agentTotalName', $checkLoginByInput['username']);
            $this->session->set('agentTotalInfo', $loginInfo);

//            $this->cookie->set('agentId', '');
            $this->session->set('agentId', '');
            $this->session->set('agentName', '');
            $this->session->set('agentInfo', '');
            return $this->retBjuiData([],'ok','登录成功');
        }
    }

    //总代理商登出
    public function actionAgentTotalAdminLogout()
    {
        session_destroy();
        return $this->massage('退出成功', $this->router->pathFor('agentTotalAdminLogin'), 0);
    }

    private function actionGetSmsCode($telphone){
        $res = $this->retMessage('error','操作失败');
        $totalId=AgentTotalModel::where("telphone",$telphone)->first();
        if($totalId){
            $ret = $this->smsSend($telphone);
            if($ret){
                $res = $this->retMessage('ok','发送成功');
            }
        }
        return $this->retJsonData($res);
    }
    //代理登录日志
    private function loginLogUser($remark,$agent_id=0){
        if(empty($agent_id)){
            return false;
        }
        $log['remark']=$remark;
        $log['agent_id']=$agent_id;
        $log['created_at']=\Carbon\Carbon::now();
        $log['ip']=$this->getClientIP();
        $log['area']=$this->getIPCity($log['ip']);
        AgentLoginUserModel::insert($log);
    }
    //代理登录日志
    private function loginLogTotal($remark,$agent_id=0){
        if(empty($agent_id)){
            return false;
        }
        $log['remark']=$remark;
        $log['agent_id']=$agent_id;
        $log['created_at']=\Carbon\Carbon::now();
        $log['ip']=$this->getClientIP();
        $log['area']=$this->getIPCity($log['ip']);
        AgentLoginTotalModel::insert($log);
    }

    private function getSessionContent($sessionId,$expireTime=1800){
        $filename = '/tmp/sess_'.$sessionId;
        if (!file_exists($filename)) {
            return null;
        }
        $filestats = stat($filename);
        if((time() - $filestats['mtime']) >= $expireTime){
            unlink($filename);
            return null;
        }
        return file_get_contents($filename);
    }
}
