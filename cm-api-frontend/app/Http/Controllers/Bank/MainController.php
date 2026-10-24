<?php

namespace App\Http\Controllers\Bank;

use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
use App\Services\BankService;
use Exception;

class MainController extends Controller
{
    /**
     * 銀行選單
     *
     * @bodyParam type integer required 類別; 1:銀行, 2:電話, 3:加密貨幣 Example: 1
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": "會員系統編號,ex:1"
     * }
     */
    public function getDropdown(Request $request)
    {
        try {
            $service = new BankService();
            $list = $service->getWithdrawalDropdown($request->CASH_FLOW_GROUP, $request->type);
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'data' => $list
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
