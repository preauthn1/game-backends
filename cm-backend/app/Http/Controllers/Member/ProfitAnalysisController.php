<?php

namespace App\Http\Controllers\Member;

use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
use App\Services\MemberService;
use Exception;
use Validator;

class ProfitAnalysisController extends Controller
{
    /**
     * 排序下拉選單
     */
    public function getOrderBy(Request $request)
    {
        try {
            $list = array();
            $list[] = array('id' => 1, 'name' => __('param.profit_analysis.orderBy.1'));
            $list[] = array('id' => 2, 'name' => __('param.profit_analysis.orderBy.2'));
            $list[] = array('id' => 3, 'name' => __('param.profit_analysis.orderBy.3'));
            $list[] = array('id' => 4, 'name' => __('param.profit_analysis.orderBy.4'));
            $list[] = array('id' => 5, 'name' => __('param.profit_analysis.orderBy.5'));
            $list[] = array('id' => 6, 'name' => __('param.profit_analysis.orderBy.6'));
            $list[] = array('id' => 7, 'name' => __('param.profit_analysis.orderBy.7'));
            $list[] = array('id' => 8, 'name' => __('param.profit_analysis.orderBy.8'));
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
     * 排序方式下拉選單
     */
    public function getSort(Request $request)
    {
        try {
            $list = array();
            $list[] = array('id' => 1, 'name' => __('param.profit_analysis.sort.1'));
            $list[] = array('id' => 2, 'name' => __('param.profit_analysis.sort.2'));
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
     * 盈利分析
     *
     * @bodyParam username string 會員名稱 Example: abc
     * @bodyParam uid integer UID Example: abc order_by
     * @bodyParam order_by integer 排序欄位 Example: 1;  總充值金額、總提現金額、總購買金額、總銷售返佣、總邀請獎勵、總兌換USDT金額、總兌換COIN、總折舊金額、總盈利
     * @bodyParam sort integer 排序方式 Example: 1
     * @bodyParam start_time datetime 帳號創建開始時間 Example: 2020-01-01 00:00
     * @bodyParam end_time datetime 帳號創建結束時間 Example: 2020-01-02 00:00
     * @bodyParam page integer required 第幾頁 Example: 1
     * @bodyParam per_page integer required 每頁筆數 Example: 30
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *      "list": [
     *          {
     *              "mm_id": 709,
     *              "member_id": 90,
     *              "username": "puffgff001",
     *              "name": "PS5 Pro",
     *              "rent_price": "111.0000",
     *              "parent_id": null,
     *              "parent_username": null,
     *              "status": 1,
     *              "start_time": "2024-10-02 00:00",
     *              "end_time": "2024-10-29 23:59",
     *              "cycle_days": 4,
     *              "statusTxt": "进行中",
     *          }
     *      ],
     *      "pagination": {
     *          "per_page": 10,
     *          "total": 4,
     *          "pages": 1,
     *          "previous": "",
     *          "current": 1,
     *          "next": ""
     *      }
     *    }
     * }
     */
    public function getList(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'uid' => 'sometimes|digits_between:0,10',
                'username' => 'sometimes|string|between:1,1000',
                'order_by' => 'sometimes|numeric|between:1,8',
                'sort' => 'required_with_all:order_by|numeric|between:1,2',
                'start_time' => 'required_without_all:uid,username|date_format:Y-m-d H:i',
                'end_time' => 'required_with:start_time|date_format:Y-m-d H:i|after_or_equal:start_time',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new MemberService();
            list($total, $result) = $service->getProfitAnalysisList($request);

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
