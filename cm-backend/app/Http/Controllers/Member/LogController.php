<?php

namespace App\Http\Controllers\Member;

use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
use App\Services\MemberLogService;
use Exception;
use Validator;

class LogController extends Controller
{
    /**
     * 會員管理 - 登入IP日誌
     *
     * @bodyParam member_id integer 會員ID Example: 1
     * @bodyParam username string 會員ID Example: abc
     * @bodyParam login_ip string 登入IP Example: 0.0.0.0
     * @bodyParam start_time datetime 開始時間 Example: 2020-01-01 00:00
     * @bodyParam end_time datetime 結束時間 Example: 2020-01-02 00:00
     * @bodyParam page integer required 第幾頁 Example: 1
     * @bodyParam per_page integer required 每頁筆數 Example: 30
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *        "id": 1
     *    }
     * }
     */
    public function getLoginList(Request $request)
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'member_id' => 'required|digits_between:0,10',
                'username' => 'string|between:2,20',
                'start_time' => 'date_format:Y-m-d H:i',
                'end_time' => 'date_format:Y-m-d H:i|after_or_equal:start_time',
                'page' => 'required|integer|between:1,1000',
                'per_page' => 'required|integer|between:1,10000'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            $service = new MemberLogService();
            list($total, $result) = $service->getIpLogList($request);

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $result,
                    'pagination' => $this->getPagination($total, $request->per_page, $request->page)
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
     * 會員管理 - 操作日誌
     *
     * @bodyParam member_id integer 會員ID Example: 1
     * @bodyParam username string 會員ID Example: abc
     * @bodyParam start_time datetime 開始時間 Example: 2020-01-01 00:00
     * @bodyParam end_time datetime 結束時間 Example: 2020-01-02 00:00
     * @bodyParam page integer required 第幾頁 Example: 1
     * @bodyParam per_page integer required 每頁筆數 Example: 30
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *        "id": 1
     *    }
     * }
     */
    public function getOperationList(Request $request)
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'member_id' => 'required|digits_between:0,10',
                'username' => 'string|between:2,20',
                'start_time' => 'date_format:Y-m-d H:i',
                'end_time' => 'date_format:Y-m-d H:i|after_or_equal:start_time',
                'page' => 'required|integer|between:1,1000',
                'per_page' => 'required|integer|between:1,10000'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            $service = new MemberService();
            list($total, $result) = $service->getList($request);

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $result,
                    'pagination' => $this->getPagination($total, $request->per_page, $request->page)
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

}
