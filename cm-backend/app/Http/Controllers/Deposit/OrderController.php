<?php

namespace App\Http\Controllers\Deposit;

use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
use App\Services\MemberService;
use App\Services\DepositService;
use App\Services\MemberWalletService;
use App\Services\TransactionLogService;
use App\Enums\WalletTypeEnum;
use Exception;
use Illuminate\Support\Facades\Validator;
use Illuminate\Support\Facades\DB;
use Illuminate\Database\QueryException;

class OrderController extends Controller
{
    /**
     * 充值管理 > 充值明細 > list
     *
     * @bodyParam username string 人員 Example: abc
     * @bodyParam gateway_id integer 渠道ID Example: 1
     * @bodyParam order_id integer 訂單ID Example: 1
     * @bodyParam order_no string 訂單編號 Example: D2024010100000
     * @bodyParam status integer 狀態 Example: 3:進行中/1:成功/2:取消
     * @bodyParam amount integer 訂單金額 Example: 100
     * @bodyParam start_time datetime 創建開始時間 Example: 2020-01-01 00:00
     * @bodyParam end_time datetime 創建結束時間 Example: 2020-01-02 00:00
     * @bodyParam pay_time_start datetime 支付開始時間 Example: 2020-01-01 00:00
     * @bodyParam pay_time_end datetime 支付結束時間 Example: 2020-01-02 00:00
     * @bodyParam page integer required 第幾頁 Example: 1
     * @bodyParam per_page integer required 每頁筆數 Example: 30
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *        "id": 1
     *    }
     * }
     */
    public function getList(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'username' => 'string|between:1,1000',
                'gateway_id' => 'digits_between:0,10',
                'order_id' => 'digits_between:0,10',
                'order_no' => 'string|between:1,1000',
                'status' => 'numeric|between:0,10',
                'amount_min' => 'sometimes|digits_between:0,10',
                'amount_max' => 'sometimes|digits_between:0,10',
                'start_time' => 'date_format:Y-m-d H:i',
                'end_time' => 'date_format:Y-m-d H:i|after_or_equal:start_time',
                'pay_time_start' => 'date_format:Y-m-d H:i',
                'pay_time_end' => 'date_format:Y-m-d H:i|after_or_equal:pay_time_start',
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

            $service = new DepositService();
            list($total, $result) = $service->getOrders($request);

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
     * 充值管理 > 充值明細 > detail
     *
     * @bodyParam order_id int 訂單ID Example: 1
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *        "id": 1
     *    }
     * }
     */
    public function view(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'order_id' => 'integer|between:1,1000'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new DepositService();
            $result = $service->getOrder($request->order_id);

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'data' => $result,
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
     * 充值管理 > 充值明細 > 狀態編輯
     *
     * @bodyParam order_id integer 充值訂單ID Example: 1
     * @bodyParam status integer 狀態; 1:成功, 2:取消
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *    }
     * }
     */
    public function update(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'order_id' => 'required|digits_between:0,10',
                'status' => 'required|numeric|between:1,2',
                'remark' => 'string'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new DepositService();
            $order = $service->checkOrder($request->order_id);

            DB::beginTransaction();
            $data = array();
            $data['operator_id'] = $request->ID;
            $data['order_id'] = $request->order_id;
            $data['status'] = $request->status;
            $data['remark'] = $request->remark;
            $service->updateOrder(...$data);

            if ($request->status == 1) {
                //取錢包lock
                $memberWalletService = new MemberWalletService();
                $memberWallet = $memberWalletService->lockForUpdateByMemberId($order->member_id, WalletTypeEnum::MAIN);

                //insert transaction_logs
                $data = array();
                $data['wallet_type_id'] = WalletTypeEnum::MAIN;
                $data['member_id'] = $order->member_id;
                $data['transaction_type_id'] = 1;
                $data['order_id'] = $order->id;
                $data['point'] = $order->point;
                $data['before_balance'] = $memberWallet->balance;
                $data['after_balance'] = ($memberWallet->balance + $order->point);
                $transactionLogService = new TransactionLogService();
                $transactionLog = $transactionLogService->create(...$data);

                //更新錢包
                $data = array();
                $data['member_id'] = $order->member_id;
                $data['point'] = $order->point;
                $data['wallet_type_id'] = WalletTypeEnum::MAIN;
                list($before_balance, $after_balance) = $memberWalletService->increment(...$data);

                $memberService = new MemberService();
                $memberService->setFirstDeposit($order->member_id);

                //update transaction_logs
                $transactionLogService->setSuccess($transactionLog->id, $order->member_id);
            }

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
