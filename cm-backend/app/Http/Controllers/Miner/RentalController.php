<?php

namespace App\Http\Controllers\Miner;

use App\Services\MinerRentalService;
use Illuminate\Http\Request;
use Illuminate\Database\QueryException;
use App\Http\Controllers\Controller;
use App\Services\MinerService;
use App\Helpers\CloudFlareHelper;
use App\Helpers\Common;
use Exception;
use Validator;

class RentalController extends Controller
{
    /**
     * 系統管理 - 租賃管理
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

            $service = new MinerRentalService();
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
     * 系統管理 - 租賃管理 - 明細
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

            $minerService = new MinerRentalService();
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
     * 系統管理 - 租賃管理 - 編輯
     *
     * @bodyParam miner_id integer required 礦機ID Example: 1
     * @bodyParam rent_price integer 租賃金額 Example: 6000
     * @bodyParam rent_total integer required 總租賃數量 Example: 14
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
                //'rent_price' => 'required',
                'rent_total' => 'required',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            $minerService = new MinerRentalService();
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
