<?php
	
namespace App\Libraries;
	
class Url
{

   public static function vpost($url, $data="", $timeOut=10) { // 模拟提交数据函数
        $curl = curl_init(); // 启动一个CURL会话
        curl_setopt($curl, CURLOPT_URL, $url); // 要访问的地址
        curl_setopt($curl, CURLOPT_SSL_VERIFYPEER, 0); // 对认证证书来源的检查
        //curl_setopt($curl, CURLOPT_SSL_VERIFYHOST, 1); // 从证书中检查SSL加密算法是否存在
        //curl_setopt($curl, CURLOPT_FOLLOWLOCATION, 1); // 使用自动跳转
        //curl_setopt($curl, CURLOPT_AUTOREFERER, 1); // 自动设置Referer
        curl_setopt($curl, CURLOPT_POST, 1); // 发送一个常规的Post请求
        curl_setopt($curl, CURLOPT_POSTFIELDS, $data); // Post提交的数据包
        curl_setopt($curl, CURLOPT_TIMEOUT, $timeOut); // 设置超时限制防止死循环
        curl_setopt($curl, CURLOPT_HEADER, 0); // 显示返回的Header区域内容
        curl_setopt($curl, CURLOPT_RETURNTRANSFER, 1); // 获取的信息以文件流的形式返回

        $tmpInfo = curl_exec($curl); // 执行操作

        if (curl_errno($curl)) {
            echo 'error:'.curl_error($curl);//捕抓异常
            curl_close($curl); // 关闭CURL会话
            return FALSE;
        }
        curl_close($curl); // 关闭CURL会话
        return $tmpInfo; // 返回数据
    }
    public static function vpost2($url,$data="",$timeOut=10){
        $ch = curl_init();
        curl_setopt($ch, CURLOPT_URL, $url);
        curl_setopt($ch,CURLOPT_SSL_VERIFYPEER,false);
        curl_setopt($ch,CURLOPT_SSL_VERIFYHOST,false);
        curl_setopt($ch, CURLOPT_HEADER,false);
        curl_setopt($ch, CURLOPT_RETURNTRANSFER,1);
        curl_setopt($ch, CURLOPT_POST,1);
        curl_setopt($ch, CURLOPT_POSTFIELDS, $data);
        curl_setopt($ch, CURLOPT_TIMEOUT, $timeOut);
        $output = curl_exec($ch);
        curl_close($ch);
        return $output;
    }
    public static function curlPost($sUrl,$aPostData,$timeOut = 10) {
		$ch = curl_init();
		curl_setopt($ch, CURLOPT_URL, $sUrl);
		curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);
		curl_setopt($ch, CURLOPT_SSL_VERIFYHOST, false);
		curl_setopt($ch, CURLOPT_POST, true);
		curl_setopt($ch, CURLOPT_HEADER, false);
		curl_setopt($ch, CURLOPT_POSTFIELDS, http_build_query($aPostData));
		curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
		curl_setopt($ch, CURLOPT_TIMEOUT, (int)$timeOut);
		curl_setopt($ch, CURLOPT_USERAGENT,'Mozilla/5.0 (Windows NT 6.1) AppleWebKit/536.11 (KHTML, like Gecko) Chrome/20.0.1132.47 Safari/536.11');
		$response = curl_exec($ch);
        if (curl_errno($ch)) {
            echo 'error:'.curl_error($ch);
            curl_close($ch);
            return FALSE;
        }
        $info = curl_getinfo($ch);
		if($info['http_code']==302){
            $response=$info['redirect_url'];
		}
        curl_close($ch);
        return $response;
	}
    public static function vpostXML($url, $data="") {
        $header[] = "Content-type: text/xml";
        $curl = curl_init();
        curl_setopt($curl, CURLOPT_URL, $url);
        curl_setopt($curl, CURLOPT_SSL_VERIFYPEER, false);
        curl_setopt($curl, CURLOPT_POST, 1);
        curl_setopt($curl, CURLOPT_HEADER, false);
        curl_setopt($curl, CURLOPT_POSTFIELDS, $data);
        curl_setopt($curl, CURLOPT_TIMEOUT, 3);
        curl_setopt($curl, CURLOPT_HTTPHEADER, $header);
        curl_setopt($curl, CURLOPT_RETURNTRANSFER, 1);
        $tmpInfo = curl_exec($curl);
        if (curl_errno($curl)) {
            echo 'error:'.curl_error($curl);
            curl_close($curl);
            return FALSE;
        }
        curl_close($curl);
        return $tmpInfo;
    }

    public static function vpostJson($url, $data="") {
        $data_string = json_encode($data);
        $header[] = "Content-type: application/json";
        $curl = curl_init();
        curl_setopt($curl, CURLOPT_URL, $url);
        curl_setopt($curl, CURLOPT_SSL_VERIFYPEER, false);
        curl_setopt($curl, CURLOPT_POST, 1);
        curl_setopt($curl, CURLOPT_HEADER, false);
        curl_setopt($curl, CURLOPT_POSTFIELDS, $data_string);
        curl_setopt($curl, CURLOPT_TIMEOUT, 3);
        curl_setopt($curl, CURLOPT_HTTPHEADER, $header);
        curl_setopt($curl, CURLOPT_RETURNTRANSFER, 1);
        $tmpInfo = curl_exec($curl);
        if (curl_errno($curl)) {
            echo 'error:'.curl_error($curl);
            curl_close($curl);
            return FALSE;
        }
        curl_close($curl);
        return $tmpInfo;
    }

	public static function getIPLoc_QQ($queryIP) {
		$url = 'http://example.invalid/cgi-bin/searchip?searchip1='.$queryIP;
		$ch = curl_init($url);
		curl_setopt($ch, CURLOPT_ENCODING ,'gb2312');
		curl_setopt($ch, CURLOPT_TIMEOUT, 10);
		curl_setopt($ch, CURLOPT_RETURNTRANSFER, true) ; //获取数据返回
		$result = curl_exec($ch);
		$result = mb_convert_encoding($result, "utf-8", "gb2312"); //编码转换，否则乱码
		curl_close($ch);
		preg_match("@<span>(.*)</span></p>@iU", $result, $ipArray);
		$loc = $ipArray[1];
		return $loc;
	}

	public static function getIPLoc_sina($queryIP) {
		$url = 'http://example.invalid/iplookup/iplookup.php?format=json&ip='.$queryIP; 
		$ch = curl_init($url); 
		//curl_setopt($ch,CURLOPT_ENCODING ,'utf8'); 
		curl_setopt($ch, CURLOPT_TIMEOUT, 10); 
		curl_setopt($ch, CURLOPT_RETURNTRANSFER, true) ; // 获取数据返回 
		$location = curl_exec($ch); 
		$location = json_decode($location); 
		curl_close($ch); 
			                         
		$loc = ""; 
		if ($location===FALSE) return ""; 
		//if (!empty($location->desc)) { 
// 			$loc = $location->country.$location->province.$location->city.$location->district.$location->isp; 
		//} else { 
		//	$loc = $location->desc; 
		//} 
		$loc=$location->city;
		return $loc; 
	}

	public static function getIPLoc_ip138($ip) {
		$source=file_get_contents('http://example.invalid/ips.asp?ip='.$ip.'&action=2'); 
		//正则匹配 
		preg_match_all("/<li>(.*)<\/li>/isU",$source,$result); 
		return ($result[1]);
	}
	public static function vget($url,$timeOut=10) {
		$ch=curl_init();
		curl_setopt($ch,CURLOPT_URL,$url);
        curl_setopt($ch,CURLOPT_RETURNTRANSFER,1);
		curl_setopt($ch,CURLOPT_HEADER,0);
        curl_setopt($ch, CURLOPT_TIMEOUT, $timeOut);
        curl_setopt($ch, CURLOPT_USERAGENT,'Mozilla/5.0 (Windows NT 6.1) AppleWebKit/536.11 (KHTML, like Gecko) Chrome/20.0.1132.47 Safari/536.11');
		$content = curl_exec($ch);
		curl_close($ch);
		return $content;
	}

    public static function postContent($url, $post = null)
    {
        $context = array();

        if (is_array($post))
        {
            ksort($post);

            $context['http'] = array
            (

                'timeout'=>60,
                'method' => 'POST',
                'header' => "Content-type: application/x-www-form-urlencoded ",
                'content' => http_build_query($post, '', '&'),
            );
        }

        return file_get_contents($url, false, stream_context_create($context));
    }

    //国王特殊化请求
    public static function gwGequest($url,$parameters,$certNo,$md5Key) {
        $signMethod = 'MD5';

        $httpHeaders = array();
        $headers = array();
        $headers['x-oapi-pv'] = '0.0.1';
        $headers['x-oapi-sdkv'] = '0.0.1/php';
        $headers['x-oapi-sk'] = $certNo;
        $headers['x-oapi-sm'] = $signMethod;

        $ch = curl_init($url);
        //curl_setopt($ch, CURLOPT_PROXY, '0.0.0.0:7777');
        curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, FALSE);     
        curl_setopt($ch, CURLOPT_SSL_VERIFYHOST, FALSE); 
        curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
        curl_setopt($ch, CURLOPT_HEADER, true);

        $body = NULL;
        $body = json_encode($parameters);
        $headers['x-oapi-sign'] = self::md5_sign($url, $headers, $body, $md5Key);
        array_push($httpHeaders, "Content-Type:application/json;charset=utf-8");
        foreach ($headers as $key => $value) {
            array_push($httpHeaders, $key . ':' . $value);
        }
        curl_setopt($ch, CURLOPT_HTTPHEADER, $httpHeaders);
        curl_setopt($ch, CURLOPT_POSTFIELDS, $body);
        $response = curl_exec($ch);
        if (curl_errno($ch)) {
            echo 'HTTP Request Error: ' . curl_error($ch);
            return;
        }

        $parsedResponse = self::parse_http_response($ch, $response);
        $httpStatus = $parsedResponse["status"];
        $headers = $parsedResponse["headers"];
        $body = $parsedResponse["body"];

        #print_r($parsedResponse);die;

        curl_close($ch);
        $msg = NULL;
        if(array_key_exists('x-oapi-msg', $headers)) {
            $msg = $headers['x-oapi-msg'];
        }

        $result = array(
            "errorCode" => $headers['x-oapi-error-code'],
            "msg" => $msg,
            "data" => $body
        );

        return $result;
    }

    private static function parse_http_response($ch, $response) {
        $headers = array();
        $header_size = curl_getinfo($ch, CURLINFO_HEADER_SIZE);
        $header_text = substr($response, 0, $header_size);
        $body = substr($response, $header_size);
        #printf("header_text: %s\n", $header_text);
        $status = 0;
        foreach (explode("\r\n", $header_text) as $i => $line) {
            if(!$line) {
                continue;
            }
            if ($i === 0) {
                list ($protocol, $status) = explode(' ', $line);
                $status = $status;
            } else {
                list ($key, $value) = explode(': ', $line);
                $headers[strtolower($key)] = urldecode($value);
            }
        }
        return array("status" => $status, "headers" => $headers, "body" => $body);
    }

    /**
     * MD5签名
     * @param $url
     * @param $headers
     * @param $body
     * @return
     *              签名值
     *
     */
    private static function md5_sign($url, $headers, $body, $md5Key) {
        ksort($headers);
        $str = $url;
        foreach ($headers as $key => $value) {
            if(strpos($key, 'x-oapi-') === 0) {
                $str = $str . "&" . $key . "=" . $value ;
            }
        }

        if(isset($body)) {
            $str = $str . "&" . $body;
        }

        $str = $str . "&" . $md5Key;
        return strtoupper(md5($str));
    }

}

?>
