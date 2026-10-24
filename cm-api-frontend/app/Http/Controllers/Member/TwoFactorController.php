<?php

namespace App\Http\Controllers\Member;

use Illuminate\Http\Request;
use Illuminate\Database\QueryException;
use App\Http\Controllers\Controller;
use App\Services\MemberService;
use App\Helpers\Google2faHelper;
use App\Helpers\ImageHelper;
use App\Helpers\Common;
use Exception;
use Validator;

class TwoFactorController extends Controller
{
    /**
     * 取得google二階段認證
     *
     * @response {
     *   "code": 1000,
     *   "message": "SUCCESS",
     *   "data": {
     *     "secret": 142fadear6ew1sa, google2fa secret key
     *     "qr_code_url": 52, google2fa qr code,
     *   }
     * }
     */
    public function enable2fa(Request $request)
    {
        try {
            $service = new MemberService();
            $member = $service->find($request->ID);

            // 生成 Google 2FA 密钥
            $secret_key = [REDACTED];
            // 将密钥保存到数据库
            $service->updateGoogle2FaSecretKey($request->ID, $secret_key);
            // 生成二维码 URL
            $qr_code_url = Google2faHelper::getQRCodeUrl($member->username, $secret_key);
            $qr_code_image = ImageHelper::generateQrCode($qr_code_url);

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'secret' => $secret_key,
                    'qr_code_url' => $qr_code_url,
                    'qr_code_image' => $qr_code_image
                ]
            ]);
        } catch (QueryException $exception) {
            $info = $exception->errorInfo;
            return response()->json([
                'code' => $info[0],
                'message' => $info[2]
            ]);
        } catch (Exception $exception) {
            return response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
    }

    /**
     * 驗證google2FA
     *
     * @bodyParam gauth_code string required 驗證碼
     *
     * @response {
     *   "code": 1000,
     *   "message": "SUCCESS"
     * }
     */
    public function verify2fa(Request $request)
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'gauth_code' => 'required|digits:6'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            $ip = Common::getUserIp();

            $service = new MemberService();
            $member = $service->find($request->ID);
            Google2faHelper::verifyKey($member->google2fa_secret, $request->gauth_code, $ip);
            $service->set2faEnable($request->ID, $member->is_2fa_enable);
            
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000')
            ]);
        } catch (QueryException $exception) {
            $info = $exception->errorInfo;
            return response()->json([
                'code' => $info[0],
                'message' => $info[2]
            ]);
        } catch (Exception $exception) {
            return response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
    }
}
