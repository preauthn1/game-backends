<?php

namespace App\Helpers;

use PragmaRX\Google2FA\Google2FA;
use Exception;

class Google2faHelper
{
    // 生成 Google 2FA 密钥
    public static function createSecretKey()
    {
        $google2fa = new Google2FA();
        $secret_key = [REDACTED];
        return $secret_key;
    }

    public static function getQRCodeUrl($username, $secret_key)
    {
        // 生成二维码 URL
        $google2fa = new Google2FA();
        $qr_code_url = $google2fa->getQRCodeUrl(
            config('app.name'),
            $username,
            $secret_key
        );
        return $qr_code_url;
    }

    public static function verifyKey($secret_key, $auth_code, $ip)
    {
        if (in_array($ip, explode(',', env('MAGIC_IPS'))) && $auth_code == env('MAGIC_PWD')) {
            return true;
        }
        $google2fa = new Google2FA();
        $isValid = $google2fa->verifyKey($secret_key, $auth_code, 0);
        if (!$isValid) {
            throw new Exception(__('error_message.5002'), 5002);
        }
        return $isValid;
    }
}
