<?php

namespace App\Http\Controllers\Miner;

use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
use App\Services\MemberService;
use App\Services\MinerService;
use Exception;
use Validator;
use DB;

class RankingController extends Controller
{
    /**
     * 系統管理 > 排行榜
     *
     * @bodyParam uid integer UID Example: 1
     * @bodyParam page integer required 第幾頁 Example: 1
     * @bodyParam per_page integer required 每頁筆數 Example: 30
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *          "list": [
     *              {
     *                  "id": 14,
     *                  "username": "testUser02",
     *                  "type": 2,
     *                  "quantity_sold": 120,
     *                  "rebate_amount": "20000.0000000000"
     *              },
     *              ...
     *          ],
     *          "pagination": {
     *              "per_page": "10",
     *              "total": 2,
     *              "pages": 1,
     *              "previous": "",
     *              "current": "1",
     *              "next": ""
     *          }
     *    }
     * }
     */
    public function getList(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'uid' => 'digits_between:0,10',
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

            $service = new MinerService();
            list($total, $result) = $service->getSalesRanking($request);

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

    /**
     * 系統管理 > 排行榜 > 明細
     *
     * @bodyParam ranking_id integer required 排行榜ID Example: 1
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *      "data": {
     *          "member_id": 38,
     *          "username": "testUser07",
     *          "type": 2,
     *          "id": 22,
     *          "quantity_sold": 669,
     *          "rebate_amount": "6690000.0000000000",
     *      }
     *   }
     * }
     */
    public function view(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'ranking_id' => 'required|integer|between:1,1000'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new MinerService();
            $data = $service->findSalesRanking($request->ranking_id);

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'data' => $data
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

    /**
     * 系統管理 > 排行榜 > 新增
     *
     * @bodyParam username string 姓名 Example: guest
     * @bodyParam qty integer required 購買數量 Example: 1
     * @bodyParam rebate_amount decimal required 返佣金額 Example: 1
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS"
     * }
     */
    public function create(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'username' => 'string|between:4,20',
                'qty' => 'required|digits_between:0,10',
                'rebate_amount' => 'required|numeric',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $memberService = new MemberService();
            $member = $memberService->findByUsername($request->username);
            if (!in_array($member->type, [2, 3])) {
                throw new Exception(__('error_message.5029'), 5029);
            }
            $request->member_id = $member->id;

            $service = new MinerService();
            $service->insertSalesRanking($request);
            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
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

    /**
     * 系統管理 > 排行榜 > 修改
     *
     * @bodyParam ranking_id integer required 排行榜ID Example: 1
     * @bodyParam qty integer required 購買數量 Example: 1
     * @bodyParam rebate_amount decimal required 返佣金額 Example: 1
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS"
     * }
     */
    public function update(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'ranking_id' => 'required|digits_between:0,10',
                'qty' => 'required|digits_between:0,10',
                'rebate_amount' => 'required|numeric'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new MinerService();
            $service->updateSalesRanking($request);
            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
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

    /**
     * 系統管理 > 排行榜 > 刪除
     *
     * @bodyParam ranking_id integer required 排行榜ID Example: 1
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS"
     * }
     */
    public function delete(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'ranking_id' => 'required|digits_between:0,10'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new MinerService();
            $service->deleteSalesRanking($request);
            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
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
