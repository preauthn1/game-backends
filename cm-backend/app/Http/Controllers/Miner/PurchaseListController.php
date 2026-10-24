<?php

namespace App\Http\Controllers\Miner;

use App\Http\Controllers\Controller;
use Illuminate\Http\Request;
use Illuminate\Database\QueryException;
use Exception;
use Illuminate\Support\Facades\Validator;
use App\Services\MemberMinerService;

class PurchaseListController extends Controller
{
    /**
     * 購買訂單狀態下拉選單
     */
    public function getStatusDropdown(Request $request)
    {
        try {
            //purchase_order.status.
            $list = array();
            $list[] = array('id' => 0, 'name' => __('param.purchase_order.status.0'));
            $list[] = array('id' => 1, 'name' => __('param.purchase_order.status.1'));
            $list[] = array('id' => 2, 'name' => __('param.purchase_order.status.2'));
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $list
                ]
            ]);

        } catch (Exception $exception) {
            return response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
    }

    /**
     * 購買列表
     *
     * @bodyParam username string 會員名稱 Example: abc
     * @bodyParam name string 礦機名稱 Example: s19
     * @bodyParam parent_username string 上級帳號 Example: username
     * @bodyParam status integer 訂單狀態 Example: 1
     * @bodyParam create_time_start datetime 創建時間開始 Example: 2020-01-01 00:00
     * @bodyParam create_time_end datetime 創建時間結束 Example: 2020-01-02 00:00
     * @bodyParam check_time_start datetime 確認時間開始 Example: 2020-01-01 00:00
     * @bodyParam check_time_end datetime 確認時間結束 Example: 2020-01-02 00:00
     * @bodyParam page integer required 第幾頁 Example: 1
     * @bodyParam per_page integer required 每頁筆數 Example: 30
     */
    public function getList(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'username' => 'sometimes|string|between:1,1000',
                'name' => 'sometimes|string|between:1,20',
                'parent_username' => 'sometimes|string|between:4,20',
                'status' => 'sometimes|numeric|between:0,2',
                'create_time_start' => 'sometimes|date_format:Y-m-d H:i',
                'create_time_end' => 'sometimes|date_format:Y-m-d H:i|after_or_equal:create_time_start',
                'check_time_start' => 'sometimes|date_format:Y-m-d H:i',
                'check_time_end' => 'sometimes|date_format:Y-m-d H:i|after_or_equal:check_time_start',
                'page' => 'required|integer|between:1,1000',
                'per_page' => 'required|integer|between:1,10000'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new MemberMinerService();
            list($total, $result) = $service->getPurchaseList($request);

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $result,
                    'pagination' => $this->getPagination($total, $request->per_page, $request->page)
                ]
            ]);
        } catch (QueryException|Exception $exception) {
            $isException = true;
            $response = response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
        $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, $isException ?? false);
        return $response;
    }
}
