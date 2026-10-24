<?php

namespace App\Http\Controllers\Deposit;

use App\Services\CurrencyService;
use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
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
            $list = $service->getDropdown(1);
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
     * 充值管理-幣種明細
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
            list($total, $result) = $currencyService->getList($request, 'deposit');
            if (count($result) > 0) {
                foreach ($result as $key => $row) {
                    $bank = $bankService->getDepositByCurrencyId($row->id, $request);
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
     * 充值管理-匯率編輯
     *
     * @bodyParam currency_id integer 幣別ID Example: 1
     * @bodyParam deposit_rate numeric 匯率 Example: 0.0001
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
                'deposit_rate' => 'required|numeric'
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
        } catch (QueryException | Exception $exception) {
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
     * 充值管理-收款銀行列表
     *
     * @bodyParam type integer 充值類別(0:全部, 1:銀行, 3:加密貨幣) Example: 1
     * @bodyParam currency_id integer 幣別ID Example: 1
     * @bodyParam bank_name string 收款銀行名 Example: abc
     * @bodyParam status integer 狀態(0:停用, 1:啟用) Example: 1
     * }
     */
    public function getDepositBankList(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'type' => 'sometimes|digits_between:1,2',
                'currency_id' => 'sometimes|integer',
                'bank_name' => 'sometimes|string|between:1,50',
                'status' => 'sometimes|integer|in:0,1',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new BankService();
            list($total, $result) = $service->getDepositBankList($request);

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
     * 充值管理-新增收款銀行
     *
     * @bodyParam type_id integer 類型ID 1=銀行;3=加密貨幣 Example: 1
     * @bodyParam currency_id integer 幣別ID Example: 1
     * @bodyParam bank_name string 銀行名稱 Example: MayBank
     * @bodyParam bank_code string 銀行代號 Example: MYD001
     * @bodyParam min_amount decimal(18,4) 最小金額 Example: 10
     * @bodyParam max_amount decimal(18,4) 最大金額 Example: 1000
     * @bodyParam sort integer 排序 Example: 2
     */
    public function create(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'type_id' => 'required|between:1,9',
                'currency_id' => 'required|digits_between:1,10',
                'bank_name' => 'required|string|between:2,20',
                'bank_code' => 'sometimes|string|between:1,20',
                'min_amount' => 'required|numeric|between:0,9999999999.999',
                'max_amount' => 'required|numeric|between:0,9999999999.999',
                'sort' => 'sometimes|digits_between:1,5',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new BankService();
            $service->createBank($request);

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000')
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
     * 充值管理-收款銀行詳情
     *
     * @bodyParam bank_id integer required 銀行ID Example: 7
     */
    public function depositBankView(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'bank_id' => 'required|integer|between:1,999999999'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new BankService();
            $data = $service->depositBankView($request);

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
     * 充值管理-收款銀行修改
     *
     * @bodyParam bank_id integer required 銀行ID Example: 7
     * @bodyParam bank_code string 銀行代號 Example: MYD001
     * @bodyParam min_amount decimal(18,4) 最小金額 Example: 10
     * @bodyParam max_amount decimal(18,4) 最大金額 Example: 1000
     * @bodyParam sort integer 排序 Example: 2
     * @bodyParam status int 狀態 Example: 1; 0:停用, 1:啟用
     */
    public function updateDepositBank(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'bank_id' => 'required|integer|between:1,999999999',
                'bank_code' => 'sometimes|string|between:1,20',
                'min_amount' => 'sometimes|numeric|between:0,9999999999.999',
                'max_amount' => 'sometimes|numeric|between:0,9999999999.999',
                'sort' => 'sometimes|digits_between:1,5',
                'status' => 'sometimes|integer|in:0,1',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new BankService();
            $service->updateDepositBank($request);

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000')
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
