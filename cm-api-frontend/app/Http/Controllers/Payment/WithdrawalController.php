<?php

namespace App\Http\Controllers\Payment;

use Illuminate\Http\Request;
use Illuminate\Http\JsonResponse;
use Illuminate\Database\QueryException;
use App\Http\Controllers\Controller;
use App\Services\WithdrawalService;
use App\Services\BankService;
use App\Services\MemberService;
use App\Services\TransactionLogService;
use App\Services\AgentService;
use App\Helpers\Google2faHelper;
use App\Helpers\Common;
use App\Enums\WalletTypeEnum;
use App\Enums\TransactionTypeEnum;
use App\Enums\AgentCustomizedSettingsEnum;
use Exception;
use Illuminate\Support\Facades\Validator;
use Illuminate\Support\Facades\DB;

class WithdrawalController extends Controller
{
    /**
     * 取得充值銀行種類
     *
     * @bodyParam withdrawal_type integer 提領類別; 0:全部, 1:銀行, 3:加密貨幣
     *
     */
    public function getBank(Request $request): JsonResponse
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'withdrawal_type' => 'digits_between:0,5'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            $service = new BankService();
            list($banks, $crypto) = $service->getWithdrawalList($request->CASH_FLOW_GROUP, $request->withdrawal_type);

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'bank' => $banks,
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
     * 建立領提訂單
     *
     * @bodyParam amount decimal require 提領金額
     * @bodyParam bank_id integer require 銀行ID
     * @bodyParam account_id integer require 銀行戶口ID
     * @bodyParam rate decimal require 匯率
     * @bodyParam fund_password string require 資金密碼
     * @bodyParam gauth_code string require Google2fa驗證
     *
     */
    public function create(Request $request): JsonResponse
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'amount' => 'required|numeric|between:0,9999999999.9999999999',
                'bank_id' => 'required|digits_between:1,5',
                'account_id' => 'required|digits_between:1,5',
                'rate' => 'required|numeric|between:0,9999999.99999999',
                'bank_tax_type' => 'required|digits_between:1,4',
                'bank_tax' => 'required|numeric|between:0,99999999999.99999',
                'fund_password' => 'required|string|between:1,30',
                'gauth_code' => 'required|string|between:1,30'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }
            //提現是否實名判斷走配置
            $agentService = new AgentService();
            $request->merge([
                'setting_key' => AgentCustomizedSettingsEnum::WITHDRAWAL_REQUIRES_VERIFIED_ID,
            ]);
            $settingResult = $agentService->getCustomizedSetting($request)?->value ?? 1;
            if (1 == $settingResult) {
                $memberService = new MemberService();
                $id_information = $memberService->getIdInformation($request->ID);
                if (1 != ($id_information['status'] ?? 9)) {
                    throw new Exception(__('error_message.1401'), 1401);
                }
            }
            $ip = Common::getUserIp();
            $memberService = new MemberService();
            $member = $memberService->find($request->ID);
            Google2faHelper::verifyKey($member->google2fa_secret, $request->gauth_code, $ip);

            //檢查資金密碼
            $fundPassword = [REDACTED], $request->fund_password, $ip);

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
            list($banks, $crypto) = $memberService->getBankAccount($request->ID);
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
            $order_no = Common::getTransactionNo('W');

            /**
             * 1.寫訂單
             * 2.扣錢包
             * 3.寫log
             */
            $withdrawalservice = new WithdrawalService();
            $order = $withdrawalservice->createOrder(
                $request->ID,
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

            list($before_balance, $after_balance) = $memberService->walletDecrement($request->ID, $request->amount);

            $transactionLogService = new TransactionLogService();
            $transactionLogService->create(
                $request->ID,
                WalletTypeEnum::MAIN,
                TransactionTypeEnum::TYPE_OUT,
                $order->id,
                $request->amount,
                $before_balance,
                $after_balance,
                1
            );

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
     * 領提清單
     *
     * @bodyParam days integer require 最近幾日
     *
     */
    public function getOrders(Request $request): JsonResponse
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'days' => 'required|digits_between:1,5'
            ]);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            $request->page = $request->page ?? 1;
            $request->per_page = $request->per_page ?? 20;

            $service = new WithdrawalService();
            list($total, $result) = $service->getOrders($request->ID, $request->days, $request->page, $request->per_page);

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
     * 清單明細
     *
     * @bodyParam withdrawal_id integer require 領提清單ID
     *
     */
    public function getOrder(Request $request): JsonResponse
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'withdrawal_id' => 'required|digits_between:1,10'
            ]);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            $service = new WithdrawalService();
            $result = $service->getOrder($request->ID, $request->withdrawal_id);

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'data' => $result
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
