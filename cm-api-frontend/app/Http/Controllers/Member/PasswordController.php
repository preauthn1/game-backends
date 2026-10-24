<?php

namespace App\Http\Controllers\Member;

use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\Hash;
use Illuminate\Support\Facades\Validator;
use App\Http\Controllers\Controller;
use App\Services\MemberService;
use App\Services\SmsService;
use App\Http\Controllers\Member\MemberController;
use Exception;

class PasswordController extends Controller
{
    /**
     * 修改會員密碼(成功會將token設置過期)
     *
     * @bodyParam old_password string required 舊會員密碼
     * @bodyParam new_password string required 新會員密碼
     * @bodyParam new_password_confirmation string required 確認新會員密碼
     *
     */
    public function change(Request $request): \Illuminate\Http\JsonResponse
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'old_password' => 'required|string|between:6,12',
                'new_password' => 'required|string|between:6,12|confirmed'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }
            $memberService = new MemberService();
            $member = $memberService->find($request->ID);
            //驗證舊密碼
            if (!Hash::check($request->old_password, $member->password)) {
                throw new Exception(__('error_message.5004'), 5004);
            }
            $data = [
                'password' => Hash::make($request->new_password)
            ];
            unset($member->country_code);
            if (!$member->update($data)) {
                throw new Exception(__('error_message.2001'), 2001);
            }
            //設置token過期
            $memberController = new MemberController();
            $memberController->setTokenExpire($request->ID);
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

    /**
     * 創建資金密碼
     *
     * @bodyParam fund_password string required 新資金密碼
     * @bodyParam fund_password_confirmation string required 確認新資金密碼
     *
     */
    public function createFund(Request $request)
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'fund_password' => 'required|string|between:6,12|confirmed'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }
            $memberService = new MemberService();
            $member = $memberService->find($request->ID);
            //已設置過資金密碼
            if (!empty($member->fund_password ?? '')) {
                throw new Exception(__('error_message.1303'), 1303);
            }
            //新資金密碼與會員密碼相同
            if (Hash::check($request->fund_password, $member->password)) {
                throw new Exception(__('error_message.6002'), 6002);
            }
            $data = [
                'fund_password' => Hash::make($request->fund_password)
            ];
            unset($member->country_code);
            if (!$member->update($data)) {
                throw new Exception(__('error_message.2001'), 2001);
            }

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

    /**
     * 修改資金密碼
     *
     * @bodyParam old_fund_password string required 舊資金密碼
     * @bodyParam new_fund_password string required 新資金密碼
     * @bodyParam new_fund_password_confirmation string required 確認新資金密碼
     *
     */
    public function changeFund(Request $request)
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'old_fund_password' => 'required|string|between:6,12',
                'new_fund_password' => 'required|string|between:6,12|confirmed'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }
            $memberService = new MemberService();
            $member = $memberService->find($request->ID);
            //驗證舊資金密碼
            if (!Hash::check($request->old_fund_password, $member->fund_password)) {
                throw new Exception(__('error_message.5005'), 5005);
            }
            //新資金密碼與會員密碼相同
            if (Hash::check($request->new_fund_password, $member->password)) {
                throw new Exception(__('error_message.6002'), 6002);
            }
            $data = [
                'fund_password' => Hash::make($request->new_fund_password)
            ];
            unset($member->country_code);
            if (!$member->update($data)) {
                throw new Exception(__('error_message.2001'), 2001);
            }

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

    /**
     * 忘記資金密碼
     *
     * @bodyParam sms_code string required 手機驗證碼
     * @bodyParam new_fund_password string required 新資金密碼
     * @bodyParam new_fund_password_confirmation string required 確認新資金密碼
     *
     */
    public function resetFund(Request $request)
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'sms_code' => 'required|string|between:4,8',
                'new_fund_password' => 'required|string|between:6,12|confirmed'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }
            $memberService = new MemberService();
            $member = $memberService->find($request->ID);
            //miner todo: 驗證手機驗證碼
            $smsService = new SmsService();
            $validateToken = [REDACTED], '2', $request->sms_code);

            //新資金密碼與會員密碼相同
            if (Hash::check($request->new_fund_password, $member->password)) {
                throw new Exception(__('error_message.6002'), 6002);
            }
            $data = [
                'fund_password' => Hash::make($request->new_fund_password)
            ];

            DB::beginTransaction();

            //更新ValidateToken Status
            $smsService->updateValidateToken($validateToken->id);

            unset($member->country_code);
            if (!$member->update($data)) {
                throw new Exception(__('error_message.2001'), 2001);
            }

            DB::commit();
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
