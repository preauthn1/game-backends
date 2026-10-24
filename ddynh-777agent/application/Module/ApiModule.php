<?php
namespace App\Module;

/**
 *  API接口公共模块
 */
class ApiModule
{

    public static $salt = 'zhongleqp';

    /**
     * 获取token
     * @param array $params 数据
     * @return string
     */
    public static function getToken($params)
    {
        $str = '';
        if(is_array($params)){
            unset($params['token']);
            ksort($params);
            foreach ($params as $key => $value) {
                $str .= $value;
            }
        }else{
            $str = $params;
        }
        return md5(md5(self::$salt.$str));
    }

    /**
     * API返回json数据格式
     * @param array $data 数据
     * @param int $code 代码
     * @param string $message 信息
     * @return string
     */
    public static function retJson($data=[],$statusCode=200,$message=""){
        $jsoncallback = htmlspecialchars($_REQUEST['callback']);
        if(empty($message)){
            $message = self::retCode($statusCode);
        }
        $result=array(  
          'statusCode'=>$statusCode,
          'message'=>$message,  
          'data'=>$data   
        );
        //输出json  
        return $jsoncallback ? $jsoncallback . "(" . json_encode($result) . ")":json_encode($result);
        //exit;  
    }

    /**
     * API返回json数据格式
     * @param array $data 数据
     * @return string
     */
    public static function retJsonData($data=[]){
        $jsoncallback = !empty($_REQUEST['callback'])?htmlspecialchars($_REQUEST['callback']):'';
        //输出json  
        return $jsoncallback ? $jsoncallback . "(" . json_encode($data) . ")" : json_encode($data);  
        //exit;  
    }

    /**
     * API返回json数据格式
     * @param array $data 数据
     * @return string
     */
    public function retMessage($status='',$message="")
    {
        $statusCode = ($status=='ok')?200:300;
        return array('statusCode'=>$statusCode,'message'=>$message);
    }

    /**
     * API返回json数据格式
     * @param array $data 数据
     * @param int $code 代码
     * @param string $message 信息
     * @return string
     */
    public static function retCode($statusCode=200)
    {
        $message = array(
            '200'=>'success',
            '-1000'=>'不存在相关API',
            '-1001'=>'请求参数异常',
            '-1002'=>'访问过期',
            '-1003'=>'token验证不通过',
            '-200'=>'操作失败'
        );
        return isset($message[$statusCode])?$message[$statusCode]:'fail';
    }

    /**
     * 匹配验证日期格式
     * @param string $date 日期
     * @return boolean
     */
    public static function checkDateFormat($date)
    {
        //匹配日期格式
        if (preg_match ("/^([0-9]{4})[-]?([0-9]{2})[-]?([0-9]{2})$/", $date, $parts))
        {
            //检测是否为日期
            if(checkdate($parts[2],$parts[3],$parts[1])){
                return true;
            }else{
            
                return false;
            }
        }
        return false;
    }

    /**
     * 自定义sql方法
     * @param string $sql 自定义sql
     * @param array $cfg 数据库配置
     * @return boolean
     */
    public static function querySql($sql,$cfg=[])
    {
        if(empty($cfg)){
            $cfg = $this->settings['db'];
        }
        $conn = mysqli_connect($cfg['host'],$cfg['username'],$cfg['password']);
        
        if (!$conn) {  
            die('Could not connect: ' . mysqli_error($conn));  
        }
        mysqli_query($conn , "set names utf8");
        mysqli_select_db($conn, $cfg['database']);
        $row = mysqli_query($conn, $sql);
        while($data=mysqli_fetch_array($row,MYSQLI_ASSOC))
        {
            $result[] = $data;
        }
        mysqli_close($conn);
        return $result;
    }
}
