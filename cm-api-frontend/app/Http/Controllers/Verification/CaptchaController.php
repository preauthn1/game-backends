<?php

namespace App\Http\Controllers\Verification;

use App\Http\Controllers\Controller;

class CaptchaController extends Controller
{
    /**
     * 獲取圖形驗證碼
     *
     * @response {
     *     "code": 1000,
     *     "message": "Success",
     *     "data": {
     *         "key": "$2y$12$0/LCUWT3QHPIjeVyjzgNSuZN4QHcz5k8K2fu7VxLND.SPH1k6ag8C",
     *         "image_url": "data:image/png;base64,iVBORw0KGgo....RK5CYII=",
     *     }
     * }
     *
     */
    public function getCaptcha()
    {
        $captcha = app('captcha')->create('default', true);
        return response()->json([
            'code' => 1000,
            'message' => __('error_message.1000'),
            'data' => [
                'key' => $captcha['key'],
                'image_url' => $captcha['img'],
            ],
        ]);
    }

    /**
     * 驗證圖形驗證碼
     *
     * @bodyParam key string required 驗證碼key
     * @bodyParam code string required 驗證碼
     *
     */
    public function checkCaptcha($key, $code): bool
    {
        if ('' == $key || '' == $code) {
            return false;
        }
        if (!captcha_api_check($code, $key)) {
            return false;
        }
        return true;
    }

}
