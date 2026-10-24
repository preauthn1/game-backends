<?php

namespace App\Http\Controllers\Member;

use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
use App\Services\MemberService;
use Exception;
use Validator;

class AccountController extends Controller
{
    /**
     * 會員管理 - 會員錢包(account)
     *
     * @bodyParam username string 會員帳號 Example: abc
     * @bodyParam real_name string 會員姓名 Example: abc
     * @bodyParam account_no string 銀行卡 Example: 123456
     * @bodyParam crypto_addr string 加密錢包地址(USDT地址) Example: 0x123456
     * @bodyParam page integer required 第幾頁 Example: 1
     * @bodyParam per_page integer required 每頁筆數 Example: 30
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *        "list": [
     *              {
     *                  "member_id": 3,
     *                  "username": "leahMiner03",
     *                  "real_name": "LeahMinerThree",
     *                  "account_bank_id": 4,
     *                  "account_name": "LeahMinerThree",
     *                  "account_no": "15454578451634803",
     *                  "account_crypto_id": 4,
     *                  "crypto_addr": "0x12345678901234567890"
     *              }
     *          ],
     *          "pagination": {
     *              "per_page": "10",
     *              "total": 1,
     *              "pages": 1,
     *              "previous": "",
     *              "current": "1",
     *              "next": ""
     *           }
     *    }
     * }
     */
    public function getList(Request $request)
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'username' => 'string|between:4,20',
                'real_name' => 'string|between:4,20',
                'account_no' => 'string|between:4,100',
                'crypto_addr' => 'string|between:4,100',
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
            list($total, $result) = $service->getAccountList($request);

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
