<?php
namespace App\Controllers;

use App\Libraries\MessageHelper;
use App\Libraries\Procedure;
use App\Libraries\Url;
use App\Models\GameServerListModel;
use App\Models\MenuModel;
use App\Models\SettingModel;
use App\Models\UserGroupModel;
use App\Models\UserModel;
use EasyWeChat\Payment\Order;
use Orx0r\Slim\Controller\Controller;
use Illuminate\Database\Capsule\Manager as DB;

class BaseController extends Controller
{
    protected $menuData       = [];
    protected $commonViewArgs = [];
    protected $menuAllData    = [];
    protected $thisMenuId     = [];
    protected $allMenu        = [];
    protected $setting        = [];
    protected $userid         = '';
    protected $group          = [];
    protected $user           = [];
    protected $users           = [];

    protected function baseAction()
    {
        //$this->group   = $this->getGroup();
        $thisViewArgs = [
            'routes'  	 => $this->router->getRouteNames(),
            'groupid'        => $this->session->get("groupid"),
        ];

        foreach ($thisViewArgs as $key => $value) {
            $this->container['view']['twig']->addGlobal($key, $value);
        }
    }

    protected function uploadImage($name, $path, $backAction, $width, $height = 0)
    {
        $upload         = $this->request->getUploadedFiles();
        $image          = $upload[$name];
        $uploadFileName = '';
        if ($image->getError() === UPLOAD_ERR_OK) {
            $uploadFileType = $image->getClientMediaType();
            if (!in_array($uploadFileType, ['image/png', 'image/jpeg', 'image/gif'])) {
                return $this->massage('上传的文件类型有误', $backAction, 3);
            }
            $uploadFileName = $image->getClientFilename();
            $fileExt        = pathinfo($uploadFileName, PATHINFO_EXTENSION);
            $uploadFileName = md5($uploadFileName . SYS_TIME) . '.' . $fileExt;
            $image->moveTo($path . $uploadFileName);
            $this->imageToThumb($path . $uploadFileName, $path . $uploadFileName, $width, $height);
        } else {
            if ($image->getError() != 4) {
                var_dump($image->getError());exit;
            }
        }
        return $uploadFileName;
    }

    protected function imageToThumb($srcImg, $dstImg, $width = 100, $height = 100, $cut = 0, $proportion = 0)
    {
        if (!is_file($srcImg)) {
            return false;
        }
        $fileExt   = strtolower(pathinfo($dstImg, PATHINFO_EXTENSION));
        $outFunc   = 'image' . ($fileExt == 'jpg' ? 'jpeg' : $fileExt);
        $srcInfo   = getimagesize($srcImg);
        $srcW      = $srcInfo[0];
        $srcH      = $srcInfo[1];
        $type      = strtolower(substr(image_type_to_extension($srcInfo[2]), 1));
        $createFun = 'imagecreatefrom' . ($type == 'jpg' ? 'jpeg' : $type);

        $dstH = $height;
        $dstW = $width;
        $x    = $y    = 0;
        if (($width > $srcW && $height > $srcH) || ($height > $srcH && $width == 0) || ($width > $srcW && $height == 0)) {
            $proportion = 1;
        }
        if ($width > $srcW) {
            $dstW = $width = $srcW;
        }
        if ($height > $srcH) {
            $dstH = $height = $srcH;
        }

        if (!$width && !$height && !$proportion) {
            return false;
        }
        if (!$proportion) {
            if ($cut == 0) {
                if ($dstW && $dstH) {
                    if ($dstW / $srcW > $dstH / $srcH) {
                        $dstW = $srcW * ($dstH / $srcH);
                        $x    = 0 - ($dstW - $width) / 2;
                    } else {
                        $dstH = $srcH * ($dstW / $srcW);
                        $y    = 0 - ($dstH - $height) / 2;
                    }
                } else if ($dstW xor $dstH) {
                    if ($dstW && !$dstH) //有宽无高
                    {
                        $propor = $dstW / $srcW;
                        $height = $dstH = $srcH * $propor;
                    } else if (!$dstW && $dstH) //有高无宽
                    {
                        $propor = $dstH / $srcH;
                        $width  = $dstW  = $srcW * $propor;
                    }
                }
            } else {
                if (!$dstH) //裁剪时无高
                {
                    $height = $dstH = $dstW;
                }
                if (!$dstW) //裁剪时无宽
                {
                    $width = $dstW = $dstH;
                }
                $propor = min(max($dstW / $srcW, $dstH / $srcH), 1);
                $dstW   = (int) round($srcW * $propor);
                $dstH   = (int) round($srcH * $propor);
                $x      = ($width - $dstW) / 2;
                $y      = ($height - $dstH) / 2;
            }
        } else {
            $proportion = min($proportion, 1);
            $height     = $dstH     = $srcH * $proportion;
            $width      = $dstW      = $srcW * $proportion;
        }
        $src   = $createFun($srcImg);
        $dst   = imagecreatetruecolor($width ? $width : $dstW, $height ? $height : $dstH);
        $white = imagecolorallocate($dst, 255, 255, 255);
        imagefill($dst, 0, 0, $white);

        if (function_exists('imagecopyresampled')) {
            imagecopyresampled($dst, $src, $x, $y, 0, 0, $dstW, $dstH, $srcW, $srcH);
        } else {
            imagecopyresized($dst, $src, $x, $y, 0, 0, $dstW, $dstH, $srcW, $srcH);
        }
        $outFunc($dst, $dstImg);
        imagedestroy($dst);
        imagedestroy($src);
        return true;

    }

    protected function getSetting()
    {
        $systemSettings = SettingModel::get()->toArray();
        $return = [];
        foreach ($systemSettings as $value) {
            $return[$value['key']] = $value;
        }
        return $return;
    }

    protected function getGroup()
    {
        $group  = UserGroupModel::get()->toArray();
        $return = [];
        foreach ($group as $value) {
            $return[$value['id']] = $value;
        }
        return $return;
    }

    protected function getUser()
    {
        $data = UserModel::get()->toArray();
        return $data;
    }

    protected function checkLogin()
    {
        if (!isset($_SESSION['userid']) || !$_SESSION['userid'] || $this->session->get('userid') != $_SESSION['userid']) {
            MessageHelper::showMessage($this->container, '请登录', $this->router->pathFor('adminLogin'), 3);
        } else {
            $this->checkPriv();
            $thisViewArgs = [
                'username' => $this->session->get('username'),
                'userid'   => $this->session->get('userid'),
            ];
            foreach ($thisViewArgs as $key => $value) {
                $this->view['twig']->addGlobal($key, $value);
            }
        }
    }

    protected function checkUserLogin()
    {
        if (!isset($_SESSION['usersid']) || !$_SESSION['usersid'] || $this->session->get('usersid') != $_SESSION['usersid']||!$this->session->get('users')) {
            MessageHelper::showMessage($this->container, '请登录', $this->router->pathFor('userLogin'), 3, SELF_URL);
        } else {
            $data        = $this->container['db']->table('users')->where('id', $this->session->get('usersid'))->first();
            if($data->status!=1){
                MessageHelper::showMessage($this->container, '用户被禁用', $this->router->pathFor('userLogin'), 3, SELF_URL);
            }
            $this->users = $data;
            $this->usergroupid = isset($data->groupid) ? $data->groupid : '';
            $thisViewArgs = [
                'username' => $this->session->get('usersname'),
                'userid'   => $this->session->get('usersid'),
                'money'   => $data->money
            ];
            foreach ($thisViewArgs as $key => $value) {
                $this->view['twig']->addGlobal($key, $value);
            }
        }
    }

    protected function getMenuId()
    {
        $uri = $this->getRoutes();
        $id  = $this->request->getAttribute('id');
        if ($id) {
            $first=stripos($uri['path'],'/' . $id);
            $last=strripos($uri['path'],'/' . $id);
            if($first==$last){
                $uri = str_replace('/' . $id, '', $uri['path']);
            }else{
                $uri = substr_replace($uri['path'],'',$first,$last-$first);
            }
            //$uri = substr($uri, 1); //cancel substr func fix index.php
        } else {
            //$uri = substr($uri['path'], 1); //cancel substr func fix index.php
            $uri = $uri['path'];
        }
        $uri = str_replace('index.php', '', $uri); //fix index.php
        $uri = substr($uri, 0 ,1)=='/'?substr($uri, 1):$uri;
//        $uri = preg_replace("/\/com\.\w+\.\w+\.?\w*/","",$uri); //特殊处理
        $data = MenuModel::where('url', $uri)->first();
        if (isset($data->id)) {
            return $data->id;
        } else {
            if(substr_count($uri,"/")>1){
                $uri=substr($uri, 0 ,strrpos($uri,"/"));
                $data = MenuModel::where('url', $uri)->first();
                if (isset($data->id)) {
                    return $data->id;
                } else {
                    if (is_numeric(substr($uri, strrpos($uri, '/')+1))) {
                        $uri=substr($uri, 0 ,strrpos($uri,"/"));
                        $data = MenuModel::where('url', $uri)->first();
                        if (isset($data->id)) {
                            return $data->id;
                        }
                    }
                }
            }
            return '';
        }
    }

    protected function checkPriv()
    {
        $allow      = false;
        $thisMenuId = $this->getMenuId();
        if(!$this->session->get('groupid')){
            MessageHelper::showMessage($this->container, '无权限操作', $this->router->pathFor('adminLogin'), 3);
        }
        $thisGroupPrivStr = $this->group[$this->session->get('groupid')]['priv'];
        $thisGroupPriv    = explode(',', $thisGroupPrivStr);

        if (in_array($thisMenuId, $thisGroupPriv)) {
            $allow = true;
        }
        
        if (!$allow && $this->session->get('groupid') != 1) {
            MessageHelper::showMessage($this->container, '无权限操作', $this->router->pathFor('adminLogin'), 3);
        }
        return $thisGroupPriv;
    }

    protected function commonData()
    {
        $thisGroupPriv = $this->checkPriv();
        if ($this->session->get('groupid') != 1 && !in_array($this->session->get("userid"), [47, 48])) {
            $this->menuAllData = MenuModel::whereIn('id', $thisGroupPriv)->get();
        } else {
            $this->menuAllData = MenuModel::orderBy('parent', 'asc')->orderBy('sort', 'asc')->get();
        }

        $this->allMenu = MenuModel::getMenu($this->menuAllData, 0);
        $menuData      = MenuModel::getMenu($this->menuAllData, 1, true); //生成树结构
        $sidebarData   = MenuModel::getMenu($this->menuAllData, 2);

        //$testData = MenuModel::getMenu($this->menuAllData, 1);
        //print_r($this->getRoutes());exit;
        //print_r($menuData);exit;
        $this->menuData       = $menuData;
        $this->commonViewArgs = [
            'main_name'    => '777管理面板',
            'menu_data'    => $this->menuData,
            'sidebar_data' => $sidebarData,
            'uri'          => $this->getRoutes(),
        ];
        //print_r($this->container['view']['viewArgs']);exit;
        //$this->container['view']['viewArgs'] = array_merge($this->container['view']['viewArgs'], $this->commonViewArgs);
        foreach ($this->commonViewArgs as $key => $value) {
            $this->container['view']['twig']->addGlobal($key, $value);
        }

    }

    /**
     * 输出调试日志到文件
     * @param string $content 内容
     * @param string $filename 文件名
     */
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

    protected function notify ($content = '', $uid = "1,110,112")
    {
//        Url::vpost("http://example.invalid:8088",array("type"=>"publish","to"=>$uid,"content"=>$content),2);
    }
    /**
     * 检查电话号码
     * @param int $tel
     * @return bool
     */
    protected function checkTel($tel)
    {
    	if (preg_match('/^1([0-9]{10})$/', $tel)) {
	    
	    	return true;
	    }
	    return false;
    }
    /**
     * 检查微信号
     * @param string $weinxin
     * @return bool
     */
    protected function checkWeiXin($weinxin)
    {
    	if (preg_match('/^[a-zA-Z0-9_-]{5,20}$/', $weinxin)) {
	    	return true;
	    }
	    return false;
    }
    /**
     * AJAX返回JSON串
     */
    protected function ajaxResult($str,$data=array()){
        if(!$str){
            $data=count($data)?$data:'';
            $body = new \Slim\Http\Body(fopen('php://temp', 'r+'));
            $body->write($data);
            return $this->response->withBody($body)->withStatus(500);
        }
    	$result['result'] = $str;
    	$result['data'] = $data;
    	exit(json_encode($result));
    }
    /**
     * 判断是否微信中
     * @return boolean
     */
    protected function is_weixin()
	{ 
    	if ( strpos($_SERVER['HTTP_USER_AGENT'], 'MicroMessenger') !== false ) {
        	return true;
    	}  
        return false;
	}
	/**
	 * 将 xml数据转换为数组格式。
	 * @param xml $xml
	 * @return array
	 */
	protected function xml_to_array($xml){
		$reg = "/<(\w+)[^>]*>([\\x00-\\xFF]*)<\\/\\1>/";
		if(preg_match_all($reg, $xml, $matches)){
			$count = count($matches[0]);
			for($i = 0; $i < $count; $i++){
			$subxml= $matches[2][$i];
			$key = $matches[1][$i];
				if(preg_match( $reg, $subxml )){
					$arr[$key] = $this->xml_to_array( $subxml );
				}else{
					$arr[$key] = $subxml;
				}
			}
		}
		return $arr;
	}
    //数组转XML
    protected function arrayToXml($arr)
    {
        $xml = "<xml>";
        foreach ($arr as $key=>$val)
        {
            if (is_numeric($val)){
                $xml.="<".$key.">".$val."</".$key.">";
            }else{
                $xml.="<".$key."><![CDATA[".$val."]]></".$key.">";
            }
        }
        $xml.="</xml>";
        return $xml;
    }

    //将XML转为array
    protected function xmlToArray($xml)
    {
        if(strpos($xml,"<xml")===false) return false;
        //禁止引用外部xml实体
        libxml_disable_entity_loader(true);
        $values = json_decode(json_encode(simplexml_load_string($xml, 'SimpleXMLElement', LIBXML_NOCDATA)), true);
        return $values;
    }

    protected function getClientIP(){
        if(getenv("HTTP_X_FORWARDED_FOR"))
            $ip = getenv("HTTP_X_FORWARDED_FOR");
        else if (getenv("REMOTE_ADDR"))
            $ip = getenv("REMOTE_ADDR");
        else if(getenv("HTTP_CLIENT_IP"))
            $ip = getenv("HTTP_CLIENT_IP");
        $ips=explode(",",$ip);
        $ip=trim($ips[0]);
        if(!filter_var($ip, FILTER_VALIDATE_IP, FILTER_FLAG_NO_PRIV_RANGE | FILTER_FLAG_NO_RES_RANGE)){
            $ip = getenv("REMOTE_ADDR");
        }
        return $ip;
    }
    protected function getIPFor(){
	    if(getenv("HTTP_X_FORWARDED_FOR"))
            $ip = getenv("HTTP_X_FORWARDED_FOR");
        else if (getenv("REMOTE_ADDR"))
            $ip = getenv("REMOTE_ADDR");
        else if(getenv("HTTP_CLIENT_IP"))
            $ip = getenv("HTTP_CLIENT_IP");
        $ips=explode(",",$ip);
        $ip=trim($ips[0]);
        if(!filter_var($ip, FILTER_VALIDATE_IP, FILTER_FLAG_NO_PRIV_RANGE | FILTER_FLAG_NO_RES_RANGE)){
            $ip = getenv("REMOTE_ADDR");
        }
        return $ip;
    }
	//数组转为jqgrid所需格式
	protected function arrToGrid($arr,$row,$page){
	    $rows=json_encode($arr);
	    $records=count($arr);
	    $total=ceil($records/$row);
	    return '{"total":'.$total.',"page":'.$page.',"records":'.$records.',"rows" :'.$rows.'}';
	}
	
	protected function makeOrderNO($uid)
	{
	    $yearCode = array('1', '2', '3', '4', '5', '6', '7', '8', '9', '10');
	    $orderSn  = $yearCode[intval(date('Y')) - 2016] .
	    strtoupper(date('m')) . date('d') .
	    substr(time(), -5) . substr(microtime(), 2, 5) . sprintf('%02d', rand(0, 99)) . substr($uid, 0, 1);
	    return $orderSn;
	}
	//按比例随机
    protected function get_rand($proArr) {
        $result = '';
        $proSum = array_sum($proArr);
        foreach ($proArr as $key => $proCur) {
            $randNum = mt_rand(1, $proSum);
            if ($randNum <= $proCur) {
                $result = $key;
                break;
            } else {
                $proSum -= $proCur;
            }
        }
        unset ($proArr);
        return $result;
    }

	//数据导出功能
	protected function exportData($list, $colData = [], $filename='') {
		try {
			if (!empty($colData)) {
				$title = '';
				foreach ($colData as $key => $value) {
					$title[$key] = $value['name'];
					foreach ($list as $k => $v) {
						$datas[$k][$key] = !empty($v[$key]) ? $v[$key] : '';
					}

				}
				unset($title['opt']);
				$titles[] = $title;
				$list = array_merge($titles, $datas);
			}
            $filename = !empty($filename)?$filename:time();
			echo $this->generateCsv($list,',','"',$filename);
		} catch (Exception $e) {
			echo 'export error!';
			exit;
		}
	}

	//数组生成CSV文件
	protected function generateCsv($data, $delimiter = ',', $enclosure = '"',$filename='') {
		header("Content-type: text/html; charset=utf-8");
		header('Content-type: application/vnd.ms-excel');
		header("Content-Disposition: attachment; filename=" . $filename . ".csv");
		header("Pragma: no-cache");
		$handle = fopen('php://temp', 'r+');
		fwrite($handle, chr(0xEF) . chr(0xBB) . chr(0xBF));
		foreach ($data as $line) {
			fputcsv($handle, $line, $delimiter, $enclosure);
		}
		rewind($handle);
		$contents = '';
		while (!feof($handle)) {
			$contents .= fread($handle, 8192);
		}
		fclose($handle);
		return $contents;
	}

	//获取配置文件
	protected function getCfg($cfgName, $field = '') {
		$settings = @include APP_PATH . "/configs/{$cfgName}.php";
		if ($settings) {
            if(empty($field)){
                return $settings;
            }
			$fields = explode('.', $field);
			foreach ($fields as $k => $v) {
				$settings = &$settings[$v];
			}
		}
		return $settings;
	}

    // 过滤掉特殊字符
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
        if (preg_match("/\::1\b/i", $ip)) {
            return  'localhost';
        }
        $area=self::getIPAddress($ip);
        return empty($area[1])?$area[0]:$area[1];
    }
    //获取市
    protected function getIPCity($ip){
        if (preg_match("/\::1\b/i", $ip)) {
            return  'localhost';
        }
        $area=self::getIPAddress($ip);
        return empty($area[2])?(empty($area[1])?$area[0]:$area[1]):$area[2];
    }
    //获取省市
    protected function getIPProCity($ip){
        if (preg_match("/\::1\b/i", $ip)) {
            return  'localhost';
        }
        $area=self::getIPAddress($ip);
        return empty($area[1])?$area[0]:$area[1].$area[2];
    }
    //判断是否移动端
    protected function isMobile(){
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
    /**
     * 二维数组根据某个字段排序
     * @param array $array 要排序的数组
     * @param string $keys   要排序的键字段
     * @param string $sort  排序类型  SORT_ASC     SORT_DESC
     * @return array 排序后的数组
     */
    protected function arraySort($array, $keys, $sort = SORT_DESC) {
        $keysValue = [];
        foreach ($array as $k => $v) {
            $keysValue[$k] = $v[$keys];
        }
        array_multisort($keysValue, $sort, $array);
        return $array;
    }

    protected function ret($code = 200, $msg = '', $data = '')
    {
        $ret = [
            'code' => $code,
            'msg' => $msg,
            'data' => $data,
        ];
        return json_encode($ret);
    }

    public function updateUserGold($arr,$type){
        $data = [
            'intUserID' => ['value' => $arr[0], 'type' => 'int'],
            'sintSourceType' => ['value' => $arr[1], 'type' => 'int'],
            'lngUpdateAmount' => ['value' => $arr[2], 'type' => 'int'],
            'chvIPAddress' => ['value' => $arr[3], 'type' => 'varchar'],
            'chvMemo' => ['value' => $arr[4], 'type' => 'varchar'],
            'lngCurrentAmount' => ['value' => '$lngCurrentAmount', 'type' => 'int', 'output' => true],
            'chvErrMsg' => ['value' => '$chvErrMsg', 'type' => 'varchar', 'output' => true],
        ];
        $procedureName = $type=='1' ? 'PrPsWeb_UpdateUserBank': 'PrWeb_Pay_FinishedAgent';
        $ret = Procedure::getInstance("center")->query($procedureName, $data);
        return $ret == 1 ? true : false;
    }

    public function toggleForbidUser($toggle,$arr){
        if(!is_array($arr)){
            return false;
        }
        $procedureName = $toggle=='0' ? 'PrWeb_Game_UnForbidUser' : 'PrWeb_Game_ForbidUser';
        $data = [
            'intUserID' => ['value' => $arr[0], 'type' => 'int'],
            'chvMemo' => ['value' => $arr[1], 'type' => 'varchar'],
            'chvIPAddress' => ['value' => $arr[2], 'type' => 'varchar'],
            'chvErrMsg' => ['value' => '$chvErrMsg', 'type' => 'varchar', 'output' => true],
        ];
        $ret = Procedure::getInstance("center")->query($procedureName, $data);
        return $ret == 1 ? true : false;
    }
    public function resetBankPass($id){
        $data = [
            'intUserID' => ['value' => $id, 'type' => 'int'],
            'chvIPAddress' => ['value' => self::getClientIP(), 'type' => 'varchar'],
            'chvErrMsg' => ['value' => '$chvErrMsg', 'type' => 'varchar', 'output' => true],
        ];
        $procedureName = 'PrWeb_ResetBankPass';
        $ret = Procedure::getInstance("center")->query($procedureName, $data);
        return $ret == 1 ? true : false;
    }

    /**
     * 执行发货流程
     * @param $arr
     * @return bool
     */
    public function deliver($intOrderID, $sintSourceTypeID, $lngAmount, $chvIPAddress, $chvMemo) {
        $data = [
            'intOrderID' => ['value' => $intOrderID, 'type' => 'varchar'],
            'sintSourceTypeID' => ['value' => $sintSourceTypeID, 'type' => 'int'],
            'lngAmount' => ['value' => $lngAmount, 'type' => 'int'],
            'chvIPAddress' => ['value' => $chvIPAddress, 'type' => 'varchar'],
            'chvMemo' => ['value' => $chvMemo, 'type' => 'varchar'],
            'chvTransData' => ['value' => '在线支付', 'type' => 'varchar'],
            'chvErrMsg' => ['value' => '$chvErrMsg', 'type' => 'varchar', 'output' => true],
        ];
        $procedureName = 'PrWeb_Pay_FinishedOrder';
        $ret = Procedure::getInstance("center")->query($procedureName, $data);
        return $ret == 1 ? true : false;
        /*DB::connection('center')->statement('SET ANSI_NULLS, QUOTED_IDENTIFIER, CONCAT_NULL_YIELDS_NULL, ANSI_WARNINGS, ANSI_PADDING ON');
        $rtn = DB::connection("center")->select('DECLARE @res VARCHAR(128);exec PrWeb_Pay_FinishedOrder ?,?,?,?,?,?,?,?,?,@res OUTPUT;select @res as res',$arr);
        if(isset($rtn[0])&&strpos($rtn[0]->res , '成功') !==false){
            return true;
        }else{
            return false;
        }*/
    }

    /**
     * 发送系统私信
     * @param $arr
     * @return bool
     */
    public function sendSysMail($arr) {
        $data = [
            'intUserID' => ['value' => $arr[0], 'type' => 'int'],
            'chvTitle' => ['value' => $arr[1], 'type' => 'varchar'],
            'chvContent' => ['value' => $arr[2], 'type' => 'varchar'],
            'intSort' => ['value' => $arr[3], 'type' => 'int'],
            'intMsgID' => ['value' => '$intMsgID', 'type' => 'int', 'output' => true],
        ];
        $procedureName = 'PrPs_SendSysMail';
        $ret = Procedure::getInstance("center")->query($procedureName, $data);
        return $ret == 1 ? true : false;
    }
    //电报监控
    protected function telegramMonitor($noticeMsg){
        if(empty($noticeMsg)){
            return false;
        }
        Url::curlPost('http://example.invalid/telegram.php', array('noticeMsg' => urlencode($noticeMsg)));
    }
    //电报监控
    protected function telegramMonitor2($noticeMsg) {
        if (empty($noticeMsg)) {
            return false;
        }
        Url::curlPost('http://example.invalid/telegram2.php', array('noticeMsg' => urlencode($noticeMsg)));
    }

     /**
     * 获取游戏控制类型
     * @param $gameid
     * @return int
     */
    protected function _getGameControlType($gameid) {
        if (in_array($gameid, GameServerListModel::JINLIU_GAMEID)) {
            return 1;
        } elseif(in_array($gameid, GameServerListModel::BAIREN_GAMEID)) {
            return 2;
        }
    }
    /**
     * 请求server服务器数据
     * @param $url
     * @param $data
     * @return array|mixed
     */
    protected function _sendServer($url,$data) {
        ksort($data);
        $vcode = "";
        foreach ($data as $k => $v) {
            $vcode .= "{$k}{$v}";
        }
        $vcode .= "abc123";
        $data['vcode'] = md5($vcode);
        $retJson = Url::curlPost($url, $data);
        return $retJson ? json_decode($retJson, true) : [];
    }

    /**
     * 是否显示兑换状态
     * @param $player_id
     * @return bool
     */
    public function exchangeShow($player_id) {
        $data = [
            'intUserId' => ['value' => $player_id, 'type' => 'int'],
            'UserShowCode' => ['value' => '$UserShowCode', 'type' => 'int', 'output' => true],
        ];
        $procedureName = 'PrPs_User_CheckShowCode';
        $ret = Procedure::getInstance("center")->query($procedureName, $data);
        return $ret == 1 ? true : false;
    }

    /**
     * 账号加星星
     * @param $str
     * @return null|string|string[]
     */
    public function accountAddStar($str) {
        if (strpos($str, '@')) {
            $email_array = explode("@", $str);
            $prevfix = (strlen($email_array[0]) < 4) ? "" : substr($str, 0, 3);
            $count = 0;
            $str = preg_replace('/([\d\w+_-]{0,100})@/', '***@', $str, -1, $count);
            $rs = $prevfix . $str;
        } else {
            $pattern = '/(1[3458]{1}[0-9])[0-9]{4}([0-9]{4})/i';
            if (preg_match($pattern, $str)) {
                $rs = preg_replace($pattern, '$1****$2', $str);
            } else {
                $rs = substr($str, 0, 3) . "***" . substr($str, -1);
            }
        }
        return $rs;
    }
    //校验银行卡号
    protected function isBankNo($bankNo){
        if(strlen($bankNo)<10){
            return false;
        }
        $arr_no = str_split($bankNo);
        $last_n = $arr_no[count($arr_no)-1];
        krsort($arr_no);
        $i = 1;
        $total = 0;
        foreach ($arr_no as $n){
            if($i%2==0){
                $ix = $n*2;
                if($ix>=10){
                    $nx = 1 + ($ix % 10);
                    $total += $nx;
                }else{
                    $total += $ix;
                }
            }else{
                $total += $n;
            }
            $i++;
        }
        $total -= $last_n;
        $total *= 9;
        if($last_n == ($total%10)){
            return true;
        }else{
            return false;
        }
    }
    protected function isEmail($str){
        $check = "/\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*/";
        if(preg_match($check,$str)){
            return true;
        }else{
            return false;
        }
    }
    protected function isTel($str){
        if(preg_match("/^1\d{10}$/",$str)){
            return true;
        }else{
            return false;
        }
    }
    protected function isChinese($str){
        if(strpos($str,'·')){
            $str=str_replace("·",'',$str);
            if(preg_match('/^[\x7f-\xff]+$/', $str)){
                return true;
            }else{
                return false;
            }
        }else{
            if(preg_match('/^[\x7f-\xff]+$/', $str)){
                return true;
            }else{
                return false;
            }
        }
    }
}
