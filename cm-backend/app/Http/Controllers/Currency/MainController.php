<?php

namespace App\Http\Controllers\Currency;

use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
use App\Services\CurrencyService;
use Exception;
use Validator;

class MainController extends Controller
{
    /**
     * 幣別下拉選單
     *
     * @bodyParam type integer 類型 Example: 1, 1:充值, 2:提領, 3:挖礦
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *        "id": 1
     *    }
     * }
     */
    public function getDropdown(Request $request)
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'type' => 'digits_between:0,10'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            $service = new CurrencyService();
            $list = $service->getDropdown($request->type);
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $list
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
