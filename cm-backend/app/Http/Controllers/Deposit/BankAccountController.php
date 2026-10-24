<?php

namespace App\Http\Controllers\Deposit;

use App\Services\CurrencyService;
use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
use App\Services\BankAccountService;
use Exception;
use Illuminate\Support\Facades\Validator;
use Illuminate\Support\Facades\DB;

class BankAccountController extends Controller
{
    /**
     * 充值管理-渠道列表
     *
     * @bodyParam type integer 充值類別(0:全部, 1:銀行, 3:加密貨幣) Example: 1
     * @bodyParam currency_id integer 幣別ID Example: 1
     * @bodyParam account_alias string 渠道別名 Example: abc
     * @bodyParam status integer 狀態(0:停用, 1:啟用) Example: 1
     * }
     */
    public function getList(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'type' => 'sometimes|digits_between:1,2',
                'currency_id' => 'sometimes|integer',
                'account_alias' => 'sometimes|string|between:1,50',
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

            $service = new BankAccountService();
            list($total, $result) = $service->getList($request);

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
     * 充值管理-修改渠道狀態
     *
     * @bodyParam bank_account_id integer 收款銀行帳號ID Example: 1
     * @bodyParam status integer 狀態 Example: 0
     */
    public function updateStatus(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'bank_account_id' => 'required|digits_between:1,10',
                'status' => 'required|integer|in:0,1',
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
            $service = new BankAccountService();
            $service->updateStatus($request);
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
     * 充值管理-新增渠道
     *
     * @bodyParam currency_id integer 幣別ID Example: 1
     * @bodyParam bank_id integer 銀行ID Example: 1
     * @bodyParam account_alias string 渠道別名 Example: abc
     * @bodyParam account_name string 銀行帳號名 Example: abc
     * @bodyParam account_no string 銀行帳號 Example: 123456
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
                'bank_id' => 'required|digits_between:1,10',
                'account_alias' => 'sometimes|string|between:1,30',
                'account_name' => 'required|string|between:2,20',
                'account_no' => 'required|string|between:4,100',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new BankAccountService();
            $service->createBankAccount($request);

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
