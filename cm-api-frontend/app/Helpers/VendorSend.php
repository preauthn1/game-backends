<?php

namespace App\Helpers;

use Log;
use Exception;

class VendorSend
{

    public $responseHeader;
    private $settings = array(
        'json_post' => false, //PostField 是否使用json_encode
        'timeout' => 30 // 訪問超時 (秒)

    );
    /**
     * VendorSend constructor.
     * @param array $settings
     */
    public function __construct($settings=[])
    {
        $this->settings = array_merge($this->settings, $settings);
    }

    /**
     * 傳送資料至其他服務
     *
     * @param string $url
     * @param string $method GET/POST/PUT/DELETE
     * @param array $headers Header
     * @param array $cookies Cookie
     * @param array $body    Body
     *
     * @return object
     */

    public function send($action, $url, $method, $body = [], $headers = array(), $cookies = array())
    {
        switch (strtoupper($method)) {
            case 'POST':
                $result = $this->post($action, $url, $body, $headers, $cookies);
                break;
            case 'PUT':
                $result = $this->put($action, $url, $body, $headers, $cookies);
                break;
            case 'DELETE':
                $result = $this->delete($url, null, $headers, $cookies);
                break;
            default:
                if ($body == []) {
                    $path = $url;
                } else {
                    $path = $url . "?" . http_build_query($body);
                };
                $result = $this->get($action, $path, $body, $headers, $cookies);
                break;
        }
        return $result;
    }

    private function post($action, $url, $data, $header = array(), $cookies = array())
    {
        $httpcode = 0;
        try {
            if ($this->settings['json_post'] === true) {
                $postFields = json_encode($data);
            } elseif ($this->settings['json_post'] == 'array') {
                $postFields = $data;
            } else {
                $postFields = http_build_query($data);
            }

            $ch = curl_init();
            curl_setopt($ch, CURLOPT_URL, $url);
            curl_setopt($ch, CURLOPT_POST, true);
            curl_setopt($ch, CURLOPT_POSTFIELDS, $postFields);
            curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
            curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);
            curl_setopt($ch, CURLOPT_TIMEOUT, $this->settings['timeout']);
            curl_setopt($ch, CURLOPT_HTTPHEADER, $header);
            curl_setopt($ch, CURLOPT_HEADER, 1);
            curl_setopt($ch, CURLOPT_IPRESOLVE, CURL_IPRESOLVE_V4);
            if($cookies) {
                $ss = [];
                foreach($cookies as $key=>$value) {
                    $ss[] = sprintf("%s=%s", $key, $value);
                }
                $cookie_string = implode("; ", $ss);
                curl_setopt($ch, CURLOPT_COOKIE, $cookie_string);
            }

            $response = curl_exec($ch);
            if($response === false) {
                $error_string = curl_error($ch);
                throw new Exception($error_string);
            }

            $header_size = curl_getinfo($ch, CURLINFO_HEADER_SIZE);
            $res_header = substr($response, 0, $header_size);
            $this->responseHeader = $res_header;

            $result = substr($response, $header_size);

            $result_log = $this->checkstrlen($result);
            $httpcode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
            $params = array($data, $postFields);
            $this->newlog($action, $url, 'POST', $params, $httpcode, $header, $result_log, $res_header);
            curl_close($ch);

            return $result;
        } catch (Exception $exception) {
            $this->errorLog($action, $url, 'POST', $postFields, $httpcode, $header, $exception->getMessage());
        }
    }

    private function put($action, $url, $data, $header = array(), $cookies = array())
    {
        $httpcode = 0;
        try {
            if ($this->settings['json_post'] === true) {
                $postFields = json_encode($data);
            } else {
                $postFields = $data;
            }

            $ch = curl_init();
            curl_setopt($ch, CURLOPT_URL, $url);
            curl_setopt($ch, CURLOPT_POSTFIELDS, $postFields);
            curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
            curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);
            curl_setopt($ch, CURLOPT_CUSTOMREQUEST, 'PUT');
            curl_setopt($ch, CURLOPT_TIMEOUT, $this->settings['timeout']);
            curl_setopt($ch, CURLOPT_HTTPHEADER, $header);
            $result = curl_exec($ch);
            if ($result === false) {
                $error_string = curl_error($ch);
                throw new Exception($error_string);
            }
            $result_log = $this->checkstrlen($result);
            $httpcode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
            $params = array($data, $postFields);
            $this->newlog($action, $url, 'PUT', $params, $httpcode, $header, $result_log);
            curl_close($ch);

            return $result;
        } catch (Exception $exception) {
            $this->errorLog($action, $url, 'PUT', $postFields, $httpcode, $header, $exception->getMessage());
        }
    }

    private function get($action, $url, $data, $header = array(), $cookies = array())
    {
        $httpcode = 0;

        try {
            $ch = curl_init();
            curl_setopt($ch, CURLOPT_URL, $url);
            curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
            curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);
            curl_setopt($ch, CURLOPT_TIMEOUT, $this->settings['timeout']);
            curl_setopt($ch, CURLOPT_HTTPHEADER, $header);
            $result = curl_exec($ch);
            if($result === false) {
                $error_string = curl_error($ch);
                throw new Exception($error_string);
            }
            $result_log = $this->checkstrlen($result);
            $httpcode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
            $this->newlog($action, $url, 'GET', $data, $httpcode, $header, $result_log);
            curl_close($ch);

            return $result;

        } catch (Exception $exception) {
            $this->errorLog($action, $url, 'GET', $data, $httpcode, $header, $exception->getMessage());
        }
    }

    private function checkstrlen($result)
    {
        //如果字超過1024個，把它壓縮
        if (strlen($result) > 1024) {
            return base64_encode(gzcompress($result));
        } else {
            return json_decode($result, true);
        }
    }

    private function newlog($action, $url, $method, $parameters, $httpcode, $header, $result, $res_header = null)
    {
        $context = [
            'request' => [
                'url' => $url,
                'method' => $method,
                'header' => $header,
                'parameters' => $parameters,
            ],
            'response' => [
                'header' => $res_header,
                'statusCode' => $httpcode,
                'body' => $result,
            ]
        ];

        try {
            $message = $action;
            Log::info($message, $context);
        } catch (Exception $e) {
            Log::error($e->getMessage(), $context);
        }
    }

    private function errorLog($action, $url, $method, $parameters, $httpcode, $header, $result)
    {
        $context = [
            'request' => [
                'url' => $url,
                'method' => $method,
                'header' => $header,
                'parameters' => $parameters,
            ],
            'response' => [
                'statusCode' => $httpcode,
                'body' => $result,
            ]
        ];
        Log::error($action, $context);
    }
}