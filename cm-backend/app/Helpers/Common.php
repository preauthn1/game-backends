<?php

namespace App\Helpers;

use App\Models\Language;
use Log;

class Common
{
    /* 是否 非空字串、非NULL、非0值 */
    public static function IsNotNullOrEmptyStringNotZeroValue($str)
    {
        return !Common::IsNullOrEmptyString($str) && $str != "0";
    }

    /* 是否 [非空字串]、[非NULL] */
    public static function IsNotNullOrEmptyString($str)
    {
        return !Common::IsNullOrEmptyString($str);
    }

    /* 是否 [空字串] 或 [NULL] */
    public static function IsNullOrEmptyString($str)
    {
        return (!isset($str) || trim($str) === '');
    }

    // 判斷是否為行動裝置
    public static function checkIsMobile()
    {
        if (isset($_SERVER['HTTP_USER_AGENT'])) {
            //Detect special conditions devices
            $iPod = stripos($_SERVER['HTTP_USER_AGENT'], "iPod");
            $iPhone = stripos($_SERVER['HTTP_USER_AGENT'], "iPhone");
            $iPad = stripos($_SERVER['HTTP_USER_AGENT'], "iPad");
            if (stripos($_SERVER['HTTP_USER_AGENT'], "Android") && stripos($_SERVER['HTTP_USER_AGENT'], "mobile")) {
                $Android = true;
                $AndroidTablet = false;
            } else if (stripos($_SERVER['HTTP_USER_AGENT'], "Android")) {
                $Android = false;
                $AndroidTablet = true;
            } else {
                $Android = false;
                $AndroidTablet = false;
            }
            $webOS = stripos($_SERVER['HTTP_USER_AGENT'], "webOS");
            $BlackBerry = stripos($_SERVER['HTTP_USER_AGENT'], "BlackBerry");
            $RimTablet = stripos($_SERVER['HTTP_USER_AGENT'], "RIM Tablet");
            //do something with this information
            if ($iPad || $AndroidTablet || $RimTablet) {
                return 2;
            } else if ($iPod || $iPhone || $Android || $webOS || $BlackBerry) {
                return 1;
            } else {
                return 0;
            }
        } else {
            return 0;
        }
    }

    //獲取語系ID
    public static function getLanguageId($code)
    {
        $row = Language::where('code', $code)->where('status', 1)->first();
        return is_object($row) ? $row->id : 1;
    }

    //紀錄日誌
    public static function log($userId, $content, $action)
    {
        // 刪除PASSWORD相關的KEY
        if (is_array($content)) {
            $content = collect($content)->map(function ($value, $key) {
                if (is_array($value) && isset($value['password'])) {
                    $value['password'] = '******';
                }
                return $value;
            })->all();
        }
        $ip = Common::getIp();
        $context = [
            'userID' => $userId,
            'content' => $content,
            'action' => $action,
            'ip' => $ip
        ];
        try {
            $action = is_array($context['action']) ? json_encode($action) : $action;

            $message = sprintf("user[%d] access %s from %s",
                $context['userID'],
                $action,
                $context['ip']);
            Log::info($message, $context);
        } catch (Exception $e) {
            Log::error($e->getMessage(), $context);
        }
    }

    public static function getUserIp()
    {
        //獲取IP蹤跡
        $traceIps = Common::getIp();
        //切割IP蹤跡
        $traceIpsList = explode(", ", $traceIps);

        return $traceIpsList[0];
    }

    //獲取IP
    public static function getIp()
    {
        if (isset($_SERVER["HTTP_CF_CONNECTING_IP"])) {
            $ip = $_SERVER["HTTP_CF_CONNECTING_IP"];
        } else if (!empty(request()->header('V-HTTP-CLIENT-IP'))) {
            $ip = request()->header('V-HTTP-CLIENT-IP');
        } elseif (!empty($_SERVER["HTTP_CLIENT_IP"])) {
            $ip = $_SERVER["HTTP_CLIENT_IP"];
        } elseif (!empty($_SERVER["HTTP_X_FORWARDED_FOR"])) {
            $ip = $_SERVER["HTTP_X_FORWARDED_FOR"];
        } else {
            $ip = $_SERVER["REMOTE_ADDR"];
        }
        return $ip;
    }

    //取得訂單號碼
    public static function getOrderNo($prefix = 'A', $rand_num = 5, $datetype = 'ymdHi')
    {
        return $prefix . date($datetype) . str_pad(rand(0, 99999), $rand_num, '0', STR_PAD_LEFT);
    }

    public static function image2Base64($request, $image_name = 'image')
    {
        // 获取上传的图像文件
        $image = $request->file($image_name);

        // 将图像文件转换为二进制数据
        $imageContents = file_get_contents($image->getPathname());

        // 将二进制数据转换为 base64 编码字符串
        return base64_encode($imageContents);
    }

    public static function getTransactionNo($prefix = '')
    {
        return $prefix . date('ymdHis') . str_pad(rand(0, 99999), 5, '0', STR_PAD_LEFT);
    }
}
