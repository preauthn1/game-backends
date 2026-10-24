<?php

namespace App\Http\Controllers\Withdrawal;

use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
use App\Services\WithdrawalService;
use App\Services\MemberService;
use App\Services\MemberWalletService;
use App\Services\TransactionLogService;
use App\Services\AgentService;
use App\Services\BankService;
use App\Enums\WalletTypeEnum;
use App\Enums\AgentCustomizedSettingsEnum;
use App\Enums\TransactionTypeEnum;
use Exception;
use Illuminate\Support\Facades\Validator;
use Illuminate\Support\Facades\DB;
use App\Helpers\Common;

class OrderController extends Controller
{
    /**
     * 會員提現管理 > 提現明細 > list
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

            $service = new WithdrawalService();
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
     * 會員提現管理 > 提現明細 > detail
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

            $service = new WithdrawalService();
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
     * 會員提現管理 > 提現明細 > 狀態編輯
     *
     * @bodyParam order_id integer 充值訂單ID Example: 1
     * @bodyParam status integer 狀態; 1:成功, 2:退還, 3:審核
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
                'status' => 'required|numeric|between:1,4',
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

            $service = new WithdrawalService();
            $order = $service->checkOrder($request->order_id, $request->status);

            DB::beginTransaction();
            $data = array();
            $data['operator_id'] = $request->ID;
            $data['order_id'] = $request->order_id;
            $data['status'] = $request->status;
            $data['remark'] = $request->remark;
            $service->updateOrder(...$data);

            if ($request->status == 2) {
                //取錢包lock
                $memberWalletService = new MemberWalletService();
                $memberWallet = $memberWalletService->lockForUpdateByMemberId($order->member_id, WalletTypeEnum::MAIN);

                //insert transaction_logs
                $data = array();
                $data['wallet_type_id'] = WalletTypeEnum::MAIN;
                $data['member_id'] = $order->member_id;
                $data['transaction_type_id'] = 14;
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

                //update transaction_logs
                $transactionLogService->setSuccess($transactionLog->id, $order->member_id);

            } elseif ($request->status == 1) {
                $memberService = new MemberService();
                $memberService->setFirstWithdrawal($order->member_id);
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

    /**
     * 會員提現管理 > 提現明細 > 創建訂單
     *
     * @bodyParam username string require 會員帳號 Example: username
     * @bodyParam amount decimal require 提領金額
     * @bodyParam bank_id integer require 銀行ID
     * @bodyParam account_id integer require 銀行戶口ID
     * @bodyParam rate decimal require 匯率
     * @bodyParam bank_tax_type integer require 手續費類型
     * @bodyParam bank_tax decimal require 手續費比率
     *
     */
    public function create(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'username' => 'required|string|between:4,20',
                'amount' => 'required|numeric|between:0,9999999999.9999999999',
                'bank_id' => 'required|digits_between:1,5',
                'account_id' => 'required|digits_between:1,5',
                'rate' => 'required|numeric|between:0,9999999.99999999',
                'bank_tax_type' => 'required|digits_between:1,4',
                'bank_tax' => 'required|numeric|between:0,99999999999.99999',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            //會員資訊
            $service = new MemberService();
            $memberData = $service->findByUsername($request->username, true, $request->HIERARCHY_PATH);
            $request->merge(['member_id'=>$memberData->id]);
            //提現是否實名判斷走配置
            $agentService = new AgentService();
            $request->merge([
                'setting_key' => AgentCustomizedSettingsEnum::WITHDRAWAL_REQUIRES_VERIFIED_ID,
            ]);
            $settingResult = $agentService->getCustomizedSetting($request)?->value ?? 1;
            if (1 == $settingResult) {
                $memberService = new MemberService();
                $id_information = $memberService->getIdInformation($request->member_id);
                if (1 != ($id_information['status'] ?? 9)) {
                    throw new Exception(__('error_message.1401'), 1401);
                }
            }
            $ip = Common::getUserIp();
            $bankService = new BankService();
            $bankService->checkWithdrawalRate($request->CASH_FLOW_GROUP, $request->bank_id, $request->rate);
            $bank = $bankService->find($request->bank_id);
            //檢查手續費類型
            if ($bank->tax_type != $request->bank_tax_type) {
                throw new Exception(__('error_message.1503') . (1), 1503);
            }
            //檢查手續費
            if ($bank->tax != $request->bank_tax) {
                throw new Exception(__('error_message.1503') . (2), 1503);
            }

            //抓取會員提現帳號資料
            $cardNo = '';
            $cardHolder = '';
            list($banks, $crypto) = $memberService->getBankAccount($request->member_id);
            if ($request->bank_id == ($banks->bank_id ?? '')) {
                $cardNo = $banks->account_no;
                $cardHolder = $banks->account_name;
            }
            if ($request->bank_id == ($crypto->bank_id ?? '')) {
                $cardNo = $crypto->crypto_address;
            }
            if (empty($cardNo)) {
                throw new Exception(__('error_message.5021'), 5021);
            }

            DB::beginTransaction();
            $order_no = Common::getTransactionNo('M');

            /**
             * 1.寫訂單
             * 2.扣錢包
             * 3.寫log
             */
            $withdrawalservice = new WithdrawalService();
            $order = $withdrawalservice->createOrder(
                $request->member_id,
                $order_no,
                $request->bank_id,
                $request->rate,
                $bank->tax_type,
                $bank->tax,
                $request->amount,
                $ip,
                $cardNo,
                $cardHolder
            );

            list($before_balance, $after_balance) = $memberService->walletDecrement($request->member_id, $request->amount);

            $transactionLogService = new TransactionLogService();
            $transactionLogService->create(
                $request->member_id,
                WalletTypeEnum::MAIN,
                TransactionTypeEnum::TYPE_OUT,
                $order->id,
                $request->amount,
                $before_balance,
                $after_balance,
                1
            );

            DB::commit();
            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
            ]);
        } catch (QueryException | Exception $exception) {
            DB::rollback();
            $isException = true;
            $response = response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage(),
                'file' => $exception->getFile(),
                'line' => $exception->getLine()
            ]);
        }
        $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, $isException ?? false);
        return $response;
    }
}
