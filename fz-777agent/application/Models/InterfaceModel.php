<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use App\Libraries\Url;

class InterfaceModel extends Model
{
    protected $table = 'setting';
    protected static $key = 'jfeuwpwejpoweietpepogn';

    private static function getValue($url,$str=""){
        $sign=md5($str."&key=".self::$key);
        $setting=self::pluck("value","key");
        $url=$setting['interface']."/$url?$str&sign=$sign";
        $result = Url::vget($url);
        $res=json_decode($result);
        if (!empty($res)&&$res->result==0) {
            return $res;
        }
        return false;
    }
    private static function setValue($url,$str=""){
        $sign=md5($str."&key=".self::$key);
        $setting=self::pluck("value","key");
        $url=$setting['interface']."/$url?$str&sign=$sign";
        $result = Url::vget($url);
        $res=json_decode($result);
        if (!empty($res)&&$res->result==0) {
            return true;
        }
        return false;
    }

    //节点接口
    public static function node($str){
        $setting=self::pluck("value","key");
        $ip = Url::vget($setting['nodeurl'] . "/?" . $str);
        if ($ip) {
            $status = @fsockopen($ip, 18888, $errno, $errstr, 5);
            if ($status) {
                return $ip;
            }
        }
        //灾备
        $ips=explode(',',$setting['ips']);
        $num=rand(0,count($ips)-1);
        return $ips[$num];
    }

}
