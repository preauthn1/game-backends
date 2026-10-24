<?php

namespace App\Http\Controllers\Tools;

use App\Http\Controllers\Controller;
use Illuminate\Http\Request;
use App\AES;
use Exception;

class AesController extends Controller
{
    //將傳入的 params & key 解密
    public function Decrypt(Request $request)
    {
        $API_AES_KEY = $this-> getQueryStringOrEnv($request , "API_AES_KEY");
        $API_IV = $this->getQueryStringOrEnv($request , "API_IV");

        $json_content = $request->getContent();
        $myObject = json_decode($json_content, true);

        $paramsValue = $myObject["params"];

        $params = str_replace(' ', '+', $paramsValue);
        $params = str_replace('\\', '', $params);

        // 5.解密param
        $aes = new AES($API_AES_KEY, $API_IV);
        $decryptResult = json_decode($aes->decrypt($params), true);

        return response()->json([
            'code' => 1000,
            'message' => __('error_message.1000'),
            'data' => $decryptResult
        ]);
    }

    //產生加 密後的MD5
    public function Encrypt(Request $request)
    {
        $API_AES_KEY = $this-> getQueryStringOrEnv($request , "API_AES_KEY");
        $API_IV = $this->getQueryStringOrEnv($request , "API_IV");
        $API_MD5_KEY = $this->getQueryStringOrEnv($request , "API_MD5_KEY");

        $json_content = ($request->all() == null)
            ? $request->getContent()
            : json_encode($request->all());

        //解決value=0時轉換成null
        $json_content = json_decode($json_content, true); // 將 JSON 字符串轉換為數組
        $json_content = array_map(function ($item) {
            return is_null($item) ? 0 : $item;
        }, $json_content);
        $json_content = json_encode($json_content);

        $aes = new AES($API_AES_KEY, $API_IV);
        $encryptParams = $aes->encrypt($json_content);
        $key = md5($encryptParams . $API_MD5_KEY );

        return response()->json([
            'code' => 1000,
            'message' => __('error_message.1000'),
            'data' => [
                'key' =>  $key,
                'params' => $encryptParams
            ]
        ]);
    }

    private function getQueryStringOrEnv(Request $request , $key)
    {
        $targetValue = $request->query($key);
        $targetValue =  isset($targetValue) ? $targetValue : env($key);
        return $targetValue;
    }
}
