<?php

namespace App\Http\Controllers\Withdrawal;

use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
use App\Services\CurrencyService;
use App\Services\BankService;
use Exception;
use Illuminate\Support\Facades\Validator;
use Illuminate\Support\Facades\DB;

class BankController extends Controller
{
    public function getDropdown(Request $request)
    {
        try {
            $service = new BankService();
            list($bank, $crypto) = $service->getWithdrawalDropdown($request->CASH_FLOW_GROUP);
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'bank' => $bank,
                    'crypto' => $crypto
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
     * 提現管理-提現明細
     *
     * @bodyParam type integer 公告類型 Example: 2, 2:法幣, 3:加密貨幣
     * @bodyParam name string 貨幣名稱 Example: USD
     * @bodyParam status integer 狀態 Example: 1
     * @bodyParam page integer required 第幾頁 Example: 1
     * @bodyParam per_page integer required 每頁筆數 Example: 30
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *      "list": [
     *          {
     *              "id": 101,
     *              "type": 3,
     *              "code": "USDT",
     *              "name": "USDT",
     *              "deposit_rate": "1.0000000000",
     *              "withdrawal_rate": "1.0000000000",
     *              "status": 1,
     *              "created_at": "2024-08-10T03:28:37.000000Z",
     *              "updated_at": "2024-08-10T03:28:37.000000Z"
     *          },
     *          ...
     *      ],
     *      "pagination": {
     *          "per_page": "50",
     *          "total": 2,
     *          "pages": 1,
     *          "previous": "",
     *          "current": "1",
     *          "next": ""
     *       }
     *    }
     * }
     */
    public function getList(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'type' => 'integer|in:2,3',
                'name' => 'string|between:1,10',
                'status' => 'integer',
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

            $currencyService = new CurrencyService();
            $bankService = new BankService();
            list($total, $result) = $currencyService->getList($request, 'withdrawal');
            if (count($result) > 0) {
                foreach ($result as $key => $row) {
                    $bank = $bankService->getWithdrawalByCurrencyId($row->id, $request);
                    $result[$key]->bank = $bank;
                }
            }

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
     * 提現管理-明細
     *
     * @bodyParam currency_id integer 幣別ID Example: 1
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *      "currency": {
     *          "id": 11,
     *          "type": 2,
     *          "code": "USD",
     *          "name": "USD",
     *          "deposit_rate": "1.0000000000",
     *          "withdrawal_rate": "1.3000000000",
     *          "status": 1,
     *          "created_at": "2024-08-10T03:28:37.000000Z",
     *          "updated_at": "2024-09-07T11:11:53.000000Z"
     *      },
     *      "bank": [
     *          {
     *              "id": 4,
     *              "agent_group_id": 2,
     *              "type": 11,
     *              "name": "MayBank",
     *              "code": "MayBank",
     *              "image": null,
     *              "currency_id": 11,
     *              "info": null,
     *              "tax_type": 0,
     *              "tax": "6.00000",
     *              "min_amount": "100.0000",
     *              "max_amount": "100000.0000",
     *              "sort": 0,
     *              "status": 1,
     *              "remark": null,
     *              "created_at": "2024-08-24T07:45:38.000000Z",
     *              "updated_at": "2024-09-07T11:27:31.000000Z"
     *          }
     *      ]
     *    }
     * }
     */
    public function view(Request $request)
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'currency_id' => 'required|integer|between:1,1000'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            $currencyService = new CurrencyService();
            $currency = $currencyService->getById($request->currency_id);

            $bankService = new BankService();
            $bank = $bankService->getWithdrawalByCurrencyId($request->currency_id, $request);

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'currency' => $currency,
                    'bank' => $bank
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
     * 提現管理-匯率、提現手續費%編輯
     *
     * @bodyParam currency_id integer 幣別ID Example: 1
     * @bodyParam withdrawal_rate numeric 匯率 Example: 0.0001
     * @bodyParam tax datetime 提現手續費% Example:1.1%
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
                'currency_id' => 'required|integer',
                'withdrawal_rate' => 'required|numeric',
                'tax' => 'required|numeric',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            DB::beginTransaction();
            $currencyService = new CurrencyService();
            $currencyService->updateRate($request);

            $bankService = new BankService();
            $bankService->updateWithdrawalTaxByCurrencyId($request);

            DB::commit();
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000')
            ]);
        } catch (QueryException $exception) {
            DB::rollback();
            $info = $exception->errorInfo;
            return response()->json([
                'code' => $info[0],
                'message' => $info[2],
                'file' => $exception->getFile(),
                'line' => $exception->getLine()
            ]);
        } catch (Exception $exception) {
            DB::rollback();
            return response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage(),
                'file' => $exception->getFile(),
                'line' => $exception->getLine()
            ]);
        }
    }

    /**
     * 提現管理-匯率、提現手續費%編輯
     *
     * @bodyParam currency_id integer 幣別ID Example: 1
     * @bodyParam withdrawal_rate numeric 匯率 Example: 0.0001
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS"
     * }
     */
    public function updateRate(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'currency_id' => 'required|integer',
                'withdrawal_rate' => 'required|numeric'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            DB::beginTransaction();
            $currencyService = new CurrencyService();
            $currencyService->updateRate($request);
            DB::commit();
            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000')
            ]);
        } catch (QueryException|Exception $exception) {
            DB::rollback();
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
     * 提現管理 - 提現手續費%編輯
     *
     * @bodyParam currency_id integer 銀行ID Example: 1
     * @bodyParam tax_type datetime 提現手續費類型 0=比率; 1=固定金額; Example:0
     * @bodyParam tax datetime 提現手續費 Example:0.011
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS"
     * }
     */
    public function updateCurrencyTax(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'currency_id' => 'required|integer',
                'tax_type' => 'required|digits_between:1,4',
                'tax' => 'required|numeric'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            DB::beginTransaction();
            $service = new BankService();
            $service->updateWithdrawalTaxByCurrencyId($request);
            DB::commit();
            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000')
            ]);
        } catch (QueryException|Exception $exception) {
            DB::rollback();
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
     * 提現管理 - 提現手續費%編輯
     *
     * @bodyParam bank_id integer 銀行ID Example: 1
     * @bodyParam tax_type datetime 提現手續費類型 0=比率; 1=固定金額; Example:0
     * @bodyParam tax datetime 提現手續費 Example:0.1
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS"
     * }
     */
    public function updateTax(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'bank_id' => 'required|integer',
                'tax_type' => 'required|digits_between:1,4',
                'tax' => 'required|numeric'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            DB::beginTransaction();
            $service = new BankService();
            $service->updateTax($request);
            DB::commit();
            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000')
            ]);
        } catch (QueryException|Exception $exception) {
            DB::rollback();
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
