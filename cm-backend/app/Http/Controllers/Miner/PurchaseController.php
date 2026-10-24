<?php

namespace App\Http\Controllers\Miner;

use Illuminate\Http\Request;
use Illuminate\Database\QueryException;
use Illuminate\Support\Facades\DB;
use App\Http\Controllers\Controller;
use App\Services\MinerPurchaseService;
use App\Services\MinerRebateService;
use App\Services\MinerRewardService;
use App\Helpers\CloudFlareHelper;
use App\Helpers\Common;
use Exception;
use Illuminate\Support\Facades\Validator;

class PurchaseController extends Controller
{
    /**
     * 系統管理 - 礦機管理
     *
     * @bodyParam name string 礦機名稱 Example: s19
     * @bodyParam is_show integer 是否展示 Example: 1
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
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'name' => 'string|between:1,20',
                'is_show' => 'integer|in:0,1',
                'status' => 'integer|in:0,1',
                //'start_time' => 'date_format:Y-m-d H:i',
                //'end_time' => 'date_format:Y-m-d H:i|after_or_equal:start_time',
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

            $service = new MinerPurchaseService();
            list($total, $result) = $service->getList($request);

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $result,
                    'pagination' => $this->getPagination($total, $request->per_page, $request->page)
                ]
            ]);
        } catch (QueryException | Exception $exception) {
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
     * 系統管理 - 礦機管理 - 明細
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
     *          "purchase_price": "1937.0000",
     *          "purchase_total": 20,
     *          "rent_price": "18.0000",
     *          "rent_total": 0,
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
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'miner_id' => 'required|integer'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new MinerPurchaseService();
            $data = $service->get($request->miner_id);

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'data' => $data
                ]
            ]);
        } catch (QueryException | Exception $exception) {
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
     * 系統管理 - 礦機管理 - 新增
     *
     * @bodyParam name string required 礦機名稱 Example: S19K
     * @bodyParam image string required 礦機圖片 Example:
     * @bodyParam currency_id integer required 挖礦幣種 Example:1
     * @bodyParam hash_rate string required 哈希率TH/s Example:1
     * @bodyParam power_consumption string required 能量消化W Example:1
     * @bodyParam day_revenue decimal 每日收益 Example: 1.1234
     * @bodyParam depreciation_cycle integer required 折舊週期(天) Example: 14
     * @bodyParam purchase_price decimal required 售價 Example: 1234.5678
     * @bodyParam purchase_total integer required 總購買數量 Example: 14
     * @bodyParam rent_price decimal required 租賃價格 Example: 1234.5678
     * @bodyParam rent_total integer required 總租賃數量 Example: 14
     * @bodyParam group_price decimal required 團購價格 Example: 1234.5678
     * @bodyParam group_total integer required 團購數量 Example: 14
     * @bodyParam promo_purchase_num integer 購買贈送租賃數量 Example: 2
     * @bodyParam promo_purchase_days integer 購買贈送租賃週期(天) Example: 28
     * @bodyParam promo_group_num integer 團購贈送租賃優惠數量 Example: 1
     * @bodyParam promo_group_days integer 團購贈送租賃優惠週期(天) Example: 14
     * @bodyParam sort integer 順序 Example: 0
     * @bodyParam is_show integer required 是否展示 Example: 1
     * @bodyParam status integer required 狀態 Example: 1, 0:disable, 1:enable, 9:block
     * @bodyParam mining_currency string 挖礦幣種 Example: BTC
     * @bodyParam day_elec_fee decimal 挖礦幣種 Example: 11.08
     * @bodyParam custody_fee decimal 挖礦幣種 Example: 0.1
     * @bodyParam custody_fee_rate decimal 挖礦幣種 Example: 0
     * @bodyParam tag string TAG Example: HOT
     * @bodyParam tag_color string 標籤顏色 Example: #32ADE6
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
                'name' => 'required|string',
                'image' => 'required|regex:/^data:image\/(\w+);base64,([A-Za-z0-9+\/=]+)$/',
                'currency_id' => 'required|integer',
                'hash_rate' => 'required|string',
                'power_consumption' => 'required|string',
                'day_revenue' => 'required',
                'depreciation_cycle' => 'required',
                'purchase_price' => 'required',
                'purchase_total' => 'required',
                'order' => 'sometimes|integer',
                'is_show' => 'required|integer|in:0,1',
                'status' => 'required',
                'rebate_percentage_1' => 'required|numeric|between:0,999999.99',
                'rebate_percentage_2' => 'required|numeric|between:0,999999.99',
                'rebate_percentage_3' => 'required|numeric|between:0,999999.99',
                'reward_quantity_3' => 'required|numeric|between:0,999999999.999999',
                'reward_quantity_5' => 'required|numeric|between:0,999999999.999999',
                'reward_quantity_10' => 'required|numeric|between:0,999999999.999999',
                'experience_commission' => 'required|numeric|between:0,999999.99',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            if (Common::IsNotNullOrEmptyString($request->image)) {
                $path = 'org/' . $request->ORGANIZATION_ID . '/miner';
                $image_url = CloudFlareHelper::uploadBase64File($request->image, $path);
                $request->image_url = $image_url;
            }

            DB::beginTransaction();
            $service = new MinerPurchaseService();
            $rebateService = new MinerRebateService();
            $rewardService = new MinerRewardService();
            $miner = $service->create($request);
            $request->miner_id = $miner->id;
            $rebateService->create($request);
            $rewardService->multiInsert($request);
            DB::commit();

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000')
            ]);
        } catch (QueryException | Exception $exception) {
            DB::rollBack();
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
     * 系統管理 - 礦機管理 - 編輯
     *
     * @bodyParam miner_id integer required 礦機ID Example: 1
     * @bodyParam name string required 礦機名稱 Example: S19K
     * @bodyParam image image required 礦機圖片 Example:
     * @bodyParam hash_rate string required 哈希率TH/s Example:1
     * @bodyParam power_consumption string required 能量消化W Example:1
     * @bodyParam day_revenue decimal 每日收益 Example: 1.1234
     * @bodyParam purchase_total integer required 總購買數量 Example: 14
     * @bodyParam rent_total integer required 總租賃數量 Example: 14
     * @bodyParam sort integer 順序 Example: 0
     * @bodyParam is_show integer 是否展示 Example: 1
     * @bodyParam info json required 礦機內容(JSON) Example: {}
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
                'miner_id' => 'required|integer',
                'name' => 'string',
                'image' => 'regex:/^data:image\/(\w+);base64,([A-Za-z0-9+\/=]+)$/',
                'hash_rate' => 'string',
                'power_consumption' => 'string',
                'day_revenue' => 'numeric',
                'rebate_percentage_1' => 'sometimes|numeric|between:0,999999.99',
                'rebate_percentage_2' => 'sometimes|numeric|between:0,999999.99',
                'rebate_percentage_3' => 'sometimes|numeric|between:0,999999.99',
                'reward_quantity_3' => 'sometimes|numeric|between:0,999999999.999999',
                'reward_quantity_5' => 'sometimes|numeric|between:0,999999999.999999',
                'reward_quantity_10' => 'sometimes|numeric|between:0,999999999.999999',
                'order' => 'sometimes|integer',
                'is_show' => 'sometimes|integer|in:0,1',
                'experience_commission' => 'sometimes|numeric|between:0,999999.99',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            if (Common::IsNotNullOrEmptyString($request->image)) {
                $path = 'org/' . $request->ORGANIZATION_ID . '/miner';
                $image_url = CloudFlareHelper::uploadBase64File($request->image, $path);
                $request->image_url = $image_url;
            }

            DB::beginTransaction();
            $service = new MinerPurchaseService();
            $rebateService = new MinerRebateService();
            $rewardService = new MinerRewardService();
            $data = $service->update($request);
            $rebateService->update($request);
            $rewardData = [];
            if (isset($request->reward_quantity_3)) {
                $rewardData[] = ['quantity' => 3, 'amount' => $request->reward_quantity_3];
            }
            if (isset($request->reward_quantity_5)) {
                $rewardData[] = ['quantity' => 5, 'amount' => $request->reward_quantity_5];
            }
            if (isset($request->reward_quantity_10)) {
                $rewardData[] = ['quantity' => 10, 'amount' => $request->reward_quantity_10];
            }
            foreach ($rewardData as $data) {
                $data['miner_id'] = $request->miner_id;
                $rewardService->update($data);
            }
            DB::commit();

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'data' => $data
                ]
            ]);
        } catch (QueryException | Exception $exception) {
            DB::rollBack();
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
