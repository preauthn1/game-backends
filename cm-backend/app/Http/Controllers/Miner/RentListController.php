<?php

namespace App\Http\Controllers\Miner;

use App\Http\Controllers\Controller;
use Illuminate\Http\Request;
use Illuminate\Database\QueryException;
use Exception;
use Illuminate\Support\Facades\Validator;
use App\Services\MemberMinerService;

class RentListController extends Controller
{
    /**
     * 租賃訂單狀態下拉選單
     */
    public function getStatusDropdown(Request $request)
    {
        try {
            //rent_order.status.
            $list = array();
            $list[] = array('id' => 0, 'name' => __('param.rent_order.status.0'));
            $list[] = array('id' => 1, 'name' => __('param.rent_order.status.1'));
            $list[] = array('id' => 2, 'name' => __('param.rent_order.status.2'));
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
     * 租賃列表
     *
     * @bodyParam username string 會員名稱 Example: abc
     * @bodyParam name string 礦機名稱 Example: s19
     * @bodyParam parent_username string 上級帳號 Example: username
     * @bodyParam status integer 訂單狀態 Example: 1
     * @bodyParam isExperience integer 是否體驗 0=否; 1=是; Example: 1
     * @bodyParam operator string 操作人員 Example: RD
     * @bodyParam c_start_time datetime 創建開始時間 Example: 2020-01-01 00:00
     * @bodyParam c_end_time datetime 創建結束時間 Example: 2020-01-02 00:00
     * @bodyParam s_start_time datetime 開始租賃開始時間 Example: 2020-01-01 00:00
     * @bodyParam s_end_time datetime 開始租賃結束時間 Example: 2020-01-02 00:00
     * @bodyParam e_start_time datetime 結束租賃開始時間 Example: 2020-01-01 00:00
     * @bodyParam e_end_time datetime 結束租賃結束時間 Example: 2020-01-02 00:00
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
                'isExperience' => 'sometimes|integer|in:0,1',
                'operator' => 'sometimes|string|between:2,20',
                'c_start_time' => 'sometimes|date_format:Y-m-d H:i',
                'c_end_time' => 'sometimes|date_format:Y-m-d H:i|after_or_equal:c_start_time',
                's_start_time' => 'sometimes|date_format:Y-m-d H:i',
                's_end_time' => 'sometimes|date_format:Y-m-d H:i|after_or_equal:s_start_time',
                'e_start_time' => 'sometimes|date_format:Y-m-d H:i',
                'e_end_time' => 'sometimes|date_format:Y-m-d H:i|after_or_equal:e_start_time',
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
            list($total, $result) = $service->getRentList($request);

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
