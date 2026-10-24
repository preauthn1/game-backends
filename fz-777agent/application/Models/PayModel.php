<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class PayModel extends Model
{
	const APPID="1480920117527616";//商户的应用ID
	const SECURE_KEY="rFO8PMzJ0gRQ5hYP0RtqTqsGOT7cX35b";//商户的秘钥
	private static  $secure_key="3zyzVB4rOyrWN0Z11dSdbgWEE1LCZykZ";//微信的秘钥
	const TIMEZONE="Asia/Shanghai";//时间时区
	const TRADE_TIME_OUT="3600";
	const FRONT_NOTIFY_URL="/front";
	const BACK_NOTIFY_URL="/back";
	const TRADE_URL="http://example.invalid";
	const QUERY_URL="http://example.invalid";
	const TRADE_FUNCODE="WP001";
	const QUERY_FUNCODE="MQ001";
	const NOTIFY_FUNCODE="N001";
	const FRONT_NOTIFY_FUNCODE="N002";
	const TRADE_TYPE="01";
	const TRADE_CURRENCYTYPE="156";
	const TRADE_CHARSET="UTF-8";
	const TRADE_DEVICE_TYPE="06";
	const TRADE_SIGN_TYPE="MD5";
	const TRADE_QSTRING_EQUAL="=";
	const TRADE_QSTRING_SPLIT="&";
	const TRADE_FUNCODE_KEY="funcode";
	const TRADE_DEVICETYPE_KEY="deviceType";
	const TRADE_SIGNTYPE_KEY="mhtSignType";
	const TRADE_SIGNATURE_KEY="mhtSignature";
	const SIGNATURE_KEY="signature";
	const SIGNTYPE_KEY="signType";
	const VERIFY_HTTPS_CERT=false;

	const WX_APPID="1481004952066690";//商户的应用ID
	const WX_TRADE_URL="https://example.invalid";
	const WX_QUERY_URL="https://example.invalid"; 
	const WX_TRADE_DEVICE_TYPE="0600";
	const TRADE_PAYCHANNELTYPE_KEY="13";
	/**
	 * 获取支付链接
	 * @return boolean|string
	 */
	public static function getPayUrl($payInfo){
		self::$secure_key=self::SECURE_KEY;
		$front_notify_url=APP_URL.self::FRONT_NOTIFY_URL;
        $back_notify_url=APP_URL.self::BACK_NOTIFY_URL;
        if(isset($payInfo["mhtOrderAmt"])&&isset($payInfo["mhtOrderName"])&&isset($payInfo["mhtOrderDetail"])){
        	if(!is_numeric($payInfo["mhtOrderAmt"])){return false;}
	        $req=array();
	        $req["mhtOrderName"]=$payInfo["mhtOrderName"];
            $req["mhtOrderAmt"]=$payInfo["mhtOrderAmt"];
            $req["mhtOrderDetail"]=$payInfo["mhtOrderDetail"];
            $req["funcode"]=self::TRADE_FUNCODE;
            $req["appId"]=self::APPID;//应用ID
            $req["mhtOrderNo"]=$payInfo['mhtOrderNo'];
            $req["mhtOrderType"]=self::TRADE_TYPE;
            $req["mhtCurrencyType"]=self::TRADE_CURRENCYTYPE;
            $req["mhtOrderTimeOut"]=self::TRADE_TIME_OUT;
            $req["mhtOrderStartTime"]=date("YmdHis");
            $req["notifyUrl"]=$back_notify_url;
            $req["frontNotifyUrl"]=$front_notify_url;
            $req["mhtCharset"]=self::TRADE_CHARSET;
            $req["deviceType"]=self::TRADE_DEVICE_TYPE;
            $req["mhtReserved"]="html";
            $req["mhtSignature"]=self::buildSign($req);
            $req["mhtSignType"]=self::TRADE_SIGN_TYPE;
	            
	            
            $req_str=self::trade($req);
            return self::TRADE_URL."?".$req_str;
        }else{
        	return false;
        }
    }
    /**
     * 微信支付链接
     * @return string|boolean
     */
    public static function getWxPayUrl($payInfo){
    	$front_notify_url=APP_URL.self::FRONT_NOTIFY_URL;
    	$back_notify_url=APP_URL.self::BACK_NOTIFY_URL;
    	if(isset($payInfo["mhtOrderAmt"])&&isset($payInfo["mhtOrderName"])&&isset($payInfo["mhtOrderDetail"])){
    		if(!is_numeric($payInfo["mhtOrderAmt"])){return false;}
            $req=array();
            $req["mhtOrderName"]=$payInfo["mhtOrderName"];
            $req["mhtOrderAmt"]=$payInfo["mhtOrderAmt"];
            $req["mhtOrderDetail"]=$payInfo["mhtOrderDetail"];
            $req["appId"]=self::WX_APPID;//应用ID
            $req["mhtOrderNo"]=$payInfo['mhtOrderNo'];
            $req["mhtOrderType"]=self::TRADE_TYPE;
            $req["mhtCurrencyType"]=self::TRADE_CURRENCYTYPE;
            $req["mhtOrderStartTime"]=date("YmdHis");
            $req["notifyUrl"]=$back_notify_url;
            $req["frontNotifyUrl"]=$front_notify_url;
            $req["mhtCharset"]=self::TRADE_CHARSET;
			$req["payChannelType"]=self::TRADE_PAYCHANNELTYPE_KEY;
            $req["mhtReserved"]="wx";
            $req["mhtSignature"]=self::buildSignature($req);
            $req["mhtSignType"]=self::TRADE_SIGN_TYPE;
            $req["funcode"]=self::TRADE_FUNCODE;
            $req["deviceType"]=self::WX_TRADE_DEVICE_TYPE;
            
            $req_str=self::trade($req);
            return self::WX_TRADE_URL."?".$req_str;
        }else{
        	return false;
        }
    }
    
    /**
     *
     * @author Jupiter
     *
     * 查询接口类
     * 通过订单号查询支付结果情况
     * 说明:以下代码只是为了方便商户测试而提供的样例代码，商户可以根据自己的需要，按照技术文档编写，并非一定要使用该代码。该代码仅供参考
     */
     public static function main() {
    	$req=array();
    	$req["funcode"]=self::QUERY_FUNCODE;
    	$req["appId"]=self::APPID;
    	$req["mhtOrderNo"]="";//商户欲查询交易订单号
    	$req["mhtCharset"]=self::TRADE_CHARSET;
    	$req["mhtSignature"]=self::buildSign($req);
    	$req["mhtSignType"]=self::TRADE_SIGN_TYPE;
    		 
    	$resp=array();
    	self::queryBuild($req, $resp);
    	print_r($resp);
    }
    
    public static function trade(Array $params) {
    	return self::createLinkString($params, false, true);
    }
    public static function queryBuild(Array $params,Array &$resp) {
    	$req_str=self::buildReq($params);
    	$resp_str=self::sendMessage($req_str, self::QUERY_URL);
    	return self::verifyResponse($resp_str, $resp);
    }
    
    public static function buildSign(Array $params) {
    	$filteredReq=self::paraFilter($params);
    	return self::buildSignature($filteredReq);
    }
    
    private static function buildReq(Array $params) {
    	return self::createLinkString($params, false, true);
    }
    
    public static function verifySignature($para){
    	$respSignature=$para[self::SIGNATURE_KEY];
    	$filteredReq=self::paraFilter($para);
    	$signature=self::buildSignature($filteredReq);
    	if ($respSignature!=""&&$respSignature==$signature) {
    		return TRUE;
    	}else {
    		return FALSE;
    	}
    }
    
    public static function verifyResponse($resp_str,&$resp){
    	if ($resp_str!="") {
    		parse_str($resp_str,$para);
    
    		$signIsValid=self::verifySignature($para);
    		$resp=$para;
    		if ($signIsValid) {
    			return TRUE;
    		}else{
    			return FALSE;
    		}
    	}
    }
    public static function paraFilter(Array $params) {
    	$result=array();
    	$flag=$params[self::TRADE_FUNCODE_KEY];
    	foreach($params as $key => $value){
    		if (($flag==self::TRADE_FUNCODE)&&!($key==self::TRADE_FUNCODE_KEY||$key==self::TRADE_DEVICETYPE_KEY
    				||$key==self::TRADE_SIGNTYPE_KEY||$key==self::TRADE_SIGNATURE_KEY)){
    			$result[$key]=$value;
    			continue;
    		}
    		if(($flag==self::NOTIFY_FUNCODE||$flag==self::FRONT_NOTIFY_FUNCODE)&&!($key==self::SIGNTYPE_KEY||$key==self::SIGNATURE_KEY)){
    			$result[$key]=$value;
    			continue;
    		}
    		if (($flag==self::QUERY_FUNCODE)&&!($key==self::TRADE_SIGNTYPE_KEY||$key==self::TRADE_SIGNATURE_KEY
    				||$key==self::SIGNTYPE_KEY||$key==self::SIGNATURE_KEY)) {
    					$result[$key]=$value;
    					continue;
    				}
    	}
    	return $result;
    }
    
    public static function buildSignature(Array $para){
    
    	$prestr=self::createLinkString($para, true, false);
    	$prestr.=self::TRADE_QSTRING_SPLIT.md5(self::$secure_key);
    	return md5($prestr);
    }
    public static function createLinkString(Array $para,$sort,$encode) {
    	$linkStr='';
    	if ($sort) {
    		$para=self::argSort($para);
    	}
    	foreach ($para as $key => $value){
    		if ($encode) {
    			$value=urlencode($value);
    		}
    		$linkStr.=$key.self::TRADE_QSTRING_EQUAL.$value.self::TRADE_QSTRING_SPLIT;
    	}
    	$linkStr=substr($linkStr, 0,count($linkStr)-2);
    	return $linkStr;
    }
    private static function argSort($para) {
    	ksort($para);
    	reset($para);
    	return $para;
    }
    /**
     * 发送信息
     *
     * @param type $req_content 请求字符串
     * @param type $url 请求地址
     * @return type 应答消息
     */
    static function sendMessage($req_content,$url) {
    	if(function_exists("curl_init")){
    		$curl=  curl_init();
    		$option=array(
    				CURLOPT_POST=>1,
    				CURLOPT_POSTFIELDS=>$req_content,
    				CURLOPT_URL=>$url,
    				CURLOPT_RETURNTRANSFER=>1,
    				CURLOPT_HEADER=>0,
    				CURLOPT_SSL_VERIFYPEER=>  self::VERIFY_HTTPS_CERT,
    				CURLOPT_SSL_VERIFYHOST=>  self::VERIFY_HTTPS_CERT
    		);
    		curl_setopt_array($curl, $option);
    		$resp_data=  curl_exec($curl);
    		if($resp_data==FALSE){
    			curl_close($curl);
    		}else{
    			curl_close($curl);
    			return $resp_data;
    		}
    	}
    }
}
