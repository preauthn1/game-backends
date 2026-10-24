<?php
namespace App\Controllers\Agentadmin;

use App\Libraries\MessageHelper;
use App\Libraries\Procedure;
use App\Libraries\Url;
use App\Models\AgentMenuModel;
use App\Models\AgentUserModel;
use App\Models\AgentTotalModel;
use Orx0r\Slim\Controller\Controller;

class BaseController extends Controller
{

    //查询登录
    protected function CheckPathPermissions(){
        $path = $this->request->getAttribute('path');
        if(!empty($path)){
            $path = explode('/', $path);//路由分发
            $path = $path[0].'/'.$path[1];
            $topMenus = AgentMenuModel::where('url', $path)->where('status','=','1')->first();
            if($topMenus['type']==0 && empty($this->session->get('agentTotalId'))){
                MessageHelper::showMessage($this->container, '非法请求', $this->router->pathFor('agentTotalAdminLogin'), 3);
            }
            if($topMenus['type']==1 && empty($this->session->get('agentId'))){
                MessageHelper::showMessage($this->container, '非法请求', $this->router->pathFor('agentAdminLogin'), 3);
            }
        }
        
        if($this->session->get('agentTotalId')){
            $loginPage = 'agentTotalAdminLogin';
            $data = AgentTotalModel::where('id', $this->session->get('agentTotalId'))->first();
            if($data['status']==0){
                MessageHelper::showMessage($this->container, '用户被禁用', $this->router->pathFor($loginPage), 3);
            }
        }
        if($this->session->get('agentId')){
            $loginPage = 'agentAdminLogin';
            $data = AgentUserModel::where('id', $this->session->get('agentId'))->first();
            if($data['status']==0){
                MessageHelper::showMessage($this->container, '用户被禁用', $this->router->pathFor($loginPage), 3);
            }
        }

        if(empty($this->session->get('agentId')) && empty($this->session->get('agentTotalId'))){
            MessageHelper::showMessage($this->container, '非法请求', $this->router->pathFor('agentAdminLogin'), 3);
        }
    }

    /**
     * 返回json数据格式
     * @param array $data 数据
     * @return string
     */
    protected function retJsonData($data=[]){
        $jsoncallback = !empty($_REQUEST['callback'])?htmlspecialchars($_REQUEST['callback']):'';
        return $jsoncallback ? $jsoncallback . "(" . json_encode($data) . ")" : json_encode($data);
    }


    /**
     * 返回json数据格式
     * @param array $data 数据
     * @return string
     */
    protected function retMessage($status='',$message="")
    {
        $msg = [
            'ok'=>200,
            'error'=>300,
            'fail'=>300,
            'timeout'=>301
        ];
        return array('statusCode'=>$msg[$status],'message'=>$message);
    }

    /**
     * 返回Bjui提示框格式
     * @param array $data 数据
     * @return string
     */
    protected function retBjuiData($data=[], $status='',$message="")
    {
        if(empty($status) && empty($message)){
            return $this->retJsonData($data);
        }else{
            $statusCode = ($status=='ok')?200:300;
            return $this->retJsonData(array('statusCode'=>$statusCode,'message'=>$message));
        }
    }

    //获取配置文件
    protected function getCfg($cfgName, $field = '') {
        $settings = @include APP_PATH . "/configs/{$cfgName}.php";
        if ($settings) {
            $fields = explode('.', $field);
            foreach ($fields as $k => $v) {
                $settings = &$settings[$v];
            }
        }
        return $settings;
    }

    public function __invoke($fun,$data) {
       return $this->$fun($data);
    }

    /**
     * 发送短信验证码
     * @param $tel
     * @return bool
     */
    protected function smsSend($tel){
        $appid = 17464; $key = "243e7385262d7c6d789b051e24c312b6";
        $url="https://example.invalid/message/xsend.json";
        $code=rand(1000,9999);
        $time=3;
        $post_data['appid']=$appid;
        $post_data['to']=$tel;
        $post_data['project']="LNyYy";
        $post_data['vars']='{"code":"'.$code.'","time":"'.$time.'"}';
        $post_data['timestamp']=time();
        $post_data['sign_type']='md5';
        ksort($post_data);
        $post_data['signature']=md5($appid.$key.urldecode(http_build_query($post_data)).$appid.$key);
        $res=json_decode(Url::vpost($url,$post_data));
        if(isset($res->status)){
            if($res->status=="success"){
                $insert['send_id']=$res->send_id;
                $insert['sms_credits']=$res->sms_credits;
                $insert['code']=$code;
                $insert['phone']=$tel;
                $insert['packname']=$this->session->get("agentTotalId");
                $insert['created_at']=\Carbon\Carbon::now();
                $insert['ip']=self::getClientIP();
                $status=$this->container['db']->table('sms')->insert($insert);
                if($status){
                    return true;
                }else{
                    return false;
                }
            }
            return false;
        }
        return false;
    }

    /**
     * 短信验证
     * @param $tel
     * @param $code
     * @return bool]
     */
    public function smsVerify($tel,$code){
        $sms  = $this->container['db']->table('sms')->where('phone', $tel)->where('code', $code)->where('verify', 0)->first();
        if($sms){
            $this->container['db']->table('sms')->where('phone', $tel)->where('code', $code)->where('verify', 0)->update(array('verify'=>1));
            return true;
        }else{
            return false;
        }
    }

    /**
     * 通过IP获取地址数据，国家，省，市，运营商
     * @param $ip
     * @return array
     */
    private function getIPAddress($ip){
        $datatype = 'jsonp';$add=["","","","","",""];
        $url = 'http://example.invalid/query/?ip='.$ip.'&datatype='.$datatype;
        $header = array('token:[REDACTED]');
        $json=self::getData($url,$header);
        $area= json_decode($json);
        if($area){
            $add= $area->data;
        }
        return $add;
    }
    //获取省
    protected function getIPPro($ip){
		return  'localhost'; //alan 修改
        if (preg_match("/\::1\b/i", $ip)) {
            return  'localhost';
        }
        $area=self::getIPAddress($ip);
        return empty($area[1])?$area[0]:$area[1];
    }
    //获取市
    protected function getIPCity($ip){
		return  'localhost'; //alan 修改
        if (preg_match("/\::1\b/i", $ip)) {
            return  'localhost';
        }
        $area=self::getIPAddress($ip);
        return empty($area[2])?(empty($area[1])?$area[0]:$area[1]):$area[2];
    }
    //获取省市
    protected function getIPProCity($ip){
		return  'localhost'; //alan 修改
        if (preg_match("/\::1\b/i", $ip)) {
            return  'localhost';
        }
        $area=self::getIPAddress($ip);
        return empty($area[1])?$area[0]:$area[1].$area[2];
    }
    //IP取地址接口专用CURL
    private function getData($url,$header){
        $ch = curl_init();
        curl_setopt($ch,CURLOPT_URL,$url);
        curl_setopt($ch,CURLOPT_HTTP_VERSION,CURL_HTTP_VERSION_1_1);
        curl_setopt($ch,CURLOPT_HTTPHEADER,$header);
        curl_setopt($ch,CURLOPT_RETURNTRANSFER,1);
        curl_setopt($ch,CURLOPT_CONNECTTIMEOUT,2);
        $handles = curl_exec($ch);
        curl_close($ch);
        return $handles;
    }
    //检验密码
    protected function checkPassword($password)
    {
        $strlen = strlen($password);
        if (self::isBadword($password)) {
            return false;
        }
        if ($strlen >= 6 && $strlen <= 20) {
            return true;
        }

        return false;
    }
    //特殊字符
    protected function isBadword($string)
    {
        $badwords = ["\\", '&', ' ', "'", '"', '/', '*', ',', '<', '>', "\r", "\t", "\n", "#"];
        foreach ($badwords as $value) {
            if (strpos($string, $value) !== false) {
                return true;
            }
        }
        return false;
    }

    protected function getPasswordEncrypt($password, $encrypt = '')
    {
        $return             = [];
        $return['encrypt']  = $encrypt ? $encrypt : self::createRandomstr();
        $return['password'] = md5(md5(trim($password)) . $return['encrypt']);
        return $encrypt ? $return['password'] : $return;
    }

    public static function createRandomstr($lenth = 6)
    {
        return self::random($lenth, '123456789abcdefghijklmnpqrstuvwxyzABCDEFGHIJKLMNPQRSTUVWXYZ');
    }
    public static function random($length, $chars = '0123456789')
    {
        $hash = '';
        $max  = strlen($chars) - 1;
        for ($i = 0; $i < $length; $i++) {
            $hash .= $chars[mt_rand(0, $max)];
        }
        return $hash;
    }

    protected function getClientIP(){
        if(getenv("REMOTE_ADDR"))
            $ip = getenv("REMOTE_ADDR");
        else if (getenv("HTTP_CLIENT_IP"))
            $ip = getenv("HTTP_CLIENT_IP");
        else if(getenv("HTTP_X_FORWARDED_FOR"))
            $ip = getenv("HTTP_X_FORWARDED_FOR");
        $ips=explode(",",$ip);
        $ip=trim($ips[0]);
        if(!filter_var($ip, FILTER_VALIDATE_IP, FILTER_FLAG_NO_PRIV_RANGE | FILTER_FLAG_NO_RES_RANGE)){
            $ip = getenv("REMOTE_ADDR");
        }
        return $ip;
    }
    //判断是否移动端
    function isMobile(){
        if (isset ($_SERVER['HTTP_X_WAP_PROFILE'])){
            return TRUE;
        }
        if (isset ($_SERVER['HTTP_USER_AGENT'])) {
            $clientkeywords = array ('mobile','nokia','sony','ericsson','mot','samsung','htc','sgh','lg','sharp','sie-','philips','panasonic','alcatel','lenovo','iphone','ipod','blackberry','meizu','android','netfront','symbian','ucweb','windowsce','palm','operamini','operamobi','openwave','nexusone','cldc','midp','wap'
            );
            if (preg_match("/(" . implode('|', $clientkeywords) . ")/i", strtolower($_SERVER['HTTP_USER_AGENT']))){
                return TRUE;
            }
        }
        if (isset ($_SERVER['HTTP_ACCEPT'])){
            if ((strpos($_SERVER['HTTP_ACCEPT'], 'vnd.wap.wml') !== FALSE) && (strpos($_SERVER['HTTP_ACCEPT'], 'text/html') === FALSE || (strpos($_SERVER['HTTP_ACCEPT'], 'vnd.wap.wml') < strpos($_SERVER['HTTP_ACCEPT'], 'text/html')))){
                return TRUE;
            }
        }
        return FALSE;
    }
    // 过滤掉emoji表情
    function filterEmoji($str)
    {
        $str = preg_replace_callback(
            '/./u',
            function (array $match) {
                return strlen($match[0]) >= 4 ? '' : $match[0];
            },
            $str);
        $str=str_replace('<','',$str);
        $str=str_replace('>','',$str);
        $str=str_replace('http','',$str);
        $str=str_replace('&','',$str);

        return $str;
    }
    
    //输出日志
    protected function log ($content = '', $filename = 'debug.log')
    {
        error_log(date('Y-m-d H:i:s') . ' --- ' . $content . "\r\n", 3, APP_ROOT . '/logs/' .$filename);
    }

    /**
     * 操作记录日志
     * @param string $content 内容
     * @param string $filename 文件名
     */
    protected function logRecord ($content = '', $filename = 'debug.log', $dir='')
    {
        $dirPath = APP_ROOT . '/logs/';
        if(empty(pathinfo($filename, PATHINFO_EXTENSION))){
            $filename = $filename.'.log';
        }
        $file = $dirPath.$filename;
        if(!empty($dir)){
            $dirpath = $dirPath.$dir;
            if (!file_exists($dirpath)){
                mkdir ($dirpath,0777,true);
            }
            $file = $dirPath.$dir.'/'.$filename;
        }
        if(!empty($this->session->get('username'))){
            $content .='|operator:'.$this->session->get('username');
        }
        error_log(date('Y-m-d H:i:s') . ' --- ' . $content . "\r\n", 3, $file);
    }

    /**
     * 发送系统私信
     * @param $arr
     * @return bool
     */
    public function sendSysMail($arr) {
        $ret = Procedure::getInstance("center")->sendSysMail($arr);
        return $ret == 1 ? true : false;
    }
    /**
     * 修改用户金币
     */
    public function updateUserGold($arr,$type){
        $ret = Procedure::getInstance("center")->updateGold($arr,$type);
        return $ret == 1 ? true : false;
    }
}
