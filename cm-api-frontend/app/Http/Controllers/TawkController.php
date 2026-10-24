<?php

namespace App\Http\Controllers;

use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
use Exception;
use App\Services\MemberService;
use Log;

class TawkController extends Controller
{
    private $apiKey;

    public function __construct()
    {
        $this->apiKey = [REDACTED]'TAWK_API_KEY');
    }

    public function getHash(Request $request)
    {
        try {
            $memberService = new MemberService();
            $member = $memberService->find($request->ID);
            
            // mobile 為空顯示unknown，不為空則需要隱碼，前面3碼後面3碼明碼
            $mobile = $member->mobile ? substr($member->mobile, 0, 3) . '****' . substr($member->mobile, -3) : 'unknown';
            // username-mobile 作為 message，API key 作為 secret key
            $userId = $member->username . '@' . $mobile;
            // $userId = strval($request->ID);

            // 使用 HMAC SHA256 生成 hash
            $hash = hash_hmac('sha256', $userId, $this->apiKey, true);
            $hash = bin2hex($hash);
            Log::info('tawk hash', ['userId' => $userId, 'hash' => $hash, 'apiKey' => $this->apiKey]);
            $tawkUser = [
                'name' => $member->username,
                'hash' => $hash,
                'userId' => $userId
            ];

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => $tawkUser
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
    public function getSSLogin(Request $request)
    {
        try {
            $memberService = new MemberService();
            $member = $memberService->find($request->ID);
            
            // mobile 為空顯示unknown，不為空則需要隱碼，前面3碼後面3碼明碼
            $mobile = $member->mobile ? substr($member->mobile, 0, 3) . '****' . substr($member->mobile, -3) : 'unknown';
            // username-mobile 作為 message，API key 作為 secret key
            $userId = md5($member->username . '@' . $mobile . '#' . $this->apiKey);
            // $userId = strval($request->ID);
            
            $ssUser = [
                'user_id' => $userId,
                'user_name' => $member->username,
                'phone' => $mobile,
            ];

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => $ssUser
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