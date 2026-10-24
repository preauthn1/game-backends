<?php

namespace App\Http\Controllers\Member;

use App\Http\Controllers\Controller;
use App\Services\MemberService;
use Illuminate\Http\Request;
use Exception;
use Illuminate\Support\Facades\Validator;
use Illuminate\Database\QueryException;

class EmailController extends Controller
{
    /**
     * 創建會員郵箱
     *
     * @bodyParam email string required 郵箱
     *
     */
    public function createEmail(Request $request)
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'email' => 'required|email'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }
            $memberService = new MemberService();
            $member = $memberService->find($request->ID);
            //已設置過郵箱
            if (!empty($member->email ?? '')) {
                throw new Exception(__('error_message.1301'), 1301);
            }
            $data = [
                'email' => $request->email
            ];
            unset($member->country_code);
            if ($member->update($data)) {
                return response()->json([
                    'code' => 1000,
                    'message' => __('error_message.1000')
                ]);
            } else {
                throw new Exception(__('error_message.2001'), 2001);
            }
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
