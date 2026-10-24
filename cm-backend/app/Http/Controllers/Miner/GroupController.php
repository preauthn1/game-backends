<?php

namespace App\Http\Controllers\Miner;

use App\Models\Member;
use Illuminate\Http\Request;
use Illuminate\Database\QueryException;
use App\Http\Controllers\Controller;
use App\Services\MinerGroupService;
use App\Services\MemberMinerService;
use App\Helpers\CloudFlareHelper;
use App\Helpers\Common;
use Exception;
use Validator;

class GroupController extends Controller
{
    /**
     * 團購訂單狀態下拉選單
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *        "id": 1
     *          "list": [
     *              {
     *                  "id": 0,
     *                  "name": "Created"
     *              },
     *              ...
     *          ]
     *    }
     * }
     */
    public function getStatusDropdown(Request $request)
    {
        try {
            $list = array();
            $list[] = array('id' => 0, 'name' => __('param.group_order.status.0'));
            $list[] = array('id' => 1, 'name' => __('param.group_order.status.1'));
            $list[] = array('id' => 2, 'name' => __('param.group_order.status.2'));
            $list[] = array('id' => 3, 'name' => __('param.group_order.status.3'));

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

    /**
     * 系統管理 - 團購訂單設置 - 列表
     *
     * @bodyParam username string 玩家名稱 Example: s19
     * @bodyParam member_type integer 玩家類型 Example: 1
     * @bodyParam name string 礦機名稱 Example: s19
     * @bodyParam start_time datetime 開始時間 Example: 2020-01-01 00:00
     * @bodyParam end_time datetime 結束時間 Example: 2020-01-02 00:00
     * @bodyParam page integer required 第幾頁 Example: 1
     * @bodyParam per_page integer required 每頁筆數 Example: 30
     */
    public function getWaitingList(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'username' => 'string|between:2,20',
                'name' => 'string|between:1,20',
                'member_type' => 'integer',
                'start_time' => 'date_format:Y-m-d H:i',
                'end_time' => 'date_format:Y-m-d H:i|after_or_equal:start_time',
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
            list($total, $result) = $service->getWaitingList($request);

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
     * 系統管理 - 待團購詳情
     *
     * @bodyParam mm_id int required 會員礦機ID
     */
    public function waitingview(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'mm_id' => 'required|integer'
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
            $data = $service->getWaiting($request->mm_id);

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
     * 系統管理 - 添加待團購
     *
     * @bodyParam username string 玩家名稱 Example: testUser01
     * @bodyParam minerName string 礦機名稱 Example: s19
     */
    public function createWaiting(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'username' => 'required|string|between:2,20',
                'minerName' => 'required|string|between:1,20',
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
            $service->createWaiting($request);

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
     * 系統管理 - 修改待團購
     *
     * @bodyParam mm_id int required 會員礦機ID
     * @bodyParam testUsername string 玩家名稱 Example: testUser01
     */
    public function updateWaiting(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'mm_id' => 'required|digits_between:1,10',
                'testUsername' => 'required|string|between:2,20',
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
            $service->updateWaiting($request);
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
     * 系統管理 - 取消待團購
     *
     * @bodyParam mm_id int required 會員礦機ID
     */
    public function cancelWaiting(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'mm_id' => 'required|digits_between:1,10',
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
            $service->cancelWaiting($request);
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
     * 系統管理 - 拼單管理
     *
     * @bodyParam name string 礦機名稱 Example: s19
     * @bodyParam status integer 礦機狀態 Example: 1
     * @bodyParam page integer required 第幾頁 Example: 1
     * @bodyParam per_page integer required 每頁筆數 Example: 30
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *      "list": [
     *          {
     *              "miner_id": 1,
     *              "name": "S19k",
     *              "image": "https://example.invalid/cdn-betmonsters-com-cdn-bucket/miner/S19K.png",
     *              "currency_id": 102,
     *              "currency_name": "BTC",
     *              "hash_rate": "0",
     *              "power_consumption": "0",
     *              "day_revenue": "3.0700",
     *              "depreciation_cycle": 14,
     *              "purchase_price": "1937.0000",
     *              "purchase_total": 20,
     *              "rent_price": "18.0000",
     *              "rent_total": 0,
     *              "promo": "{\"purchase\":{\"num\":2,\"days\":28},\"group\":{\"num\":1,\"days\":14}}",
     *              "status": 1,
     *              "info": "{\"mining_currency\":\"BTC\",\"hash_rate\":\"120\",\"power_consumption\":\"3250\",\"tag\":\"hot\",\"day_elec_fee\":\"8.17\",\"custody_fee\":\"1.5\",\"custody_fee_rate\":\"0.8\",\"tag_color\":\"#FF9900\"}",
     *              "created_at": "2024-08-07 11:44:24",
     *              "updated_at": "2024-09-08 15:37:40"
     *          }
     *      ],
     *      "pagination": {
     *          "per_page": "50",
     *          "total": 1,
     *          "pages": 1,
     *          "previous": "",
     *          "current": "1",
     *          "next": ""
     *      }
     *    }
     * }
     */
    public function getList(Request $request)
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'name' => 'string|between:1,20',
                //'status' => 'integer',
                //'start_time' => 'date_format:Y-m-d H:i',
                //'end_time' => 'date_format:Y-m-d H:i|after_or_equal:start_time',
                'page' => 'required|integer|between:1,1000',
                'per_page' => 'required|integer|between:1,10000'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            $service = new MinerGroupService();
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

    /**
     * 系統管理 - 拼單管理 - 明細
     *
     * @bodyParam miner_id integer required 礦機ID Example: 1
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *      "data": {
     *          "miner_id": 1
     *          "name": "S19k",
     *          "image": "https://example.invalid/cdn-betmonsters-com-cdn-bucket/miner/S19K.png",
     *          "currency_id": 102,
     *          "currency_name": "BTC"
     *          "hash_rate": "0",
     *          "power_consumption": "0",
     *          "day_revenue": "3.0700",
     *          "depreciation_cycle": 14,
     *          "group_price": "18.0000",
     *          "group_total": 0,
     *          "promo": {
     *              "purchase": {
     *                  "num": 2,
     *                  "days": 28
     *              },
     *              "group": {
     *                  "num": 1,
     *                  "days": 14
     *              }
     *          },
     *          "status": 1,
     *          "info": {
     *              "mining_currency": "BTC",
     *              "hash_rate": "120",
     *              "power_consumption": "3250",
     *              "tag": "hot",
     *              "day_elec_fee": "8.17",
     *              "custody_fee": "1.5",
     *              "custody_fee_rate": "0.8",
     *              "tag_color": "#FF9900"
     *          }
     *      }
     *    }
     * }
     */
    public function view(Request $request)
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'miner_id' => 'required|integer'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            $minerService = new MinerGroupService();
            $data = $minerService->get($request->miner_id);

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'data' => $data
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
     * 系統管理 - 拼單管理 - 編輯
     *
     * @bodyParam miner_id integer required 礦機ID Example: 1
     * @bodyParam group_price integer 拼單金額 Example: 6000
     * @bodyParam group_total integer required 總拼單數量 Example: 14
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS"
     * }
     */
    public function update(Request $request)
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'miner_id' => 'required|integer',
                //'group_price' => 'required',
                'group_total' => 'required',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            $minerService = new MinerGroupService();
            $data = $minerService->update($request);

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'data' => $data
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
