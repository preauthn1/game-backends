<?php

namespace App\Http\Controllers\Payment;

use Illuminate\Http\Request;
use Illuminate\Http\JsonResponse;
use Illuminate\Database\QueryException;
use App\Helpers\Common;
use App\Http\Controllers\Controller;
use App\Enums\AgentCustomizedSettingsEnum as AgentCustomizedSettingsEnum;
use App\Services\DepositService;
use App\Services\BankService;
use App\Services\AgentService;
use App\Helpers\CloudFlareHelper;
use Exception;
use Validator;

class DepositController extends Controller
{
    /**
     * 取得充值銀行種類
     *
     * @bodyParam top_up_type integer 充值類別; 0:全部, 1:銀行, 3:加密貨幣
     *
     */
    public function getBank(Request $request): JsonResponse
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'top_up_type' => 'digits_between:1,5'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            $service = new BankService();
            $agentService = new AgentService();
            $banks = $service->getDepositList($request->CASH_FLOW_GROUP, $request->top_up_type);
            $request->setting_key = AgentCustomizedSettingsEnum::USDT_DEPOSIT_PROMO_TXT;
            $usdtPromo = $agentService->getCustomizedSetting($request)?->value ?: '';

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $banks,
                    'usdt_promo' => $usdtPromo
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
     * 取得充值銀行帳號/加密錢包地址
     *
     * @bodyParam bank_id integer 銀行ID
     * @bodyParam bank_account_id integer 收款銀行ID
     *
     */
    public function getBankAccount(Request $request): JsonResponse
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'bank_id' => 'required|digits_between:1,10',
                'bank_account_id' => 'required|digits_between:1,10'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            $service = new BankService();
            $account = $service->getAccount($request->CASH_FLOW_GROUP, $request->bank_id, $request->bank_account_id);

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'account' => $account
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
     * 建立充值訂單
     *
     * @bodyParam amount integer require 充值金額
     * @bodyParam bank_id integer require 銀行ID
     * @bodyParam cardholder string require 卡片持有者
     *
     */
    public function create(Request $request): JsonResponse
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'amount' => 'required|numeric|between:0,9999999999.9999999999',
                'bank_id' => 'required|digits_between:1,5',
                'cardholder' => 'string|between:1,30'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            $service = new DepositService();
            $order = $service->create(
                $request->CASH_FLOW_GROUP,
                $request->ID,
                $request->amount,
                $request->bank_id,
                $request->cardholder
            );
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'order_id' => $order->id,
                    'bank_account_id' => $order->bank_account_id,
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
     * 上傳充值訂單圖片
     *
     * @bodyParam order_id integer require 充值訂單ID
     * @bodyParam image image require 圖片
     * @bodyParam transaction_no string require 交易序號/加密貨幣transaction_hash
     *
     */
    public function uploadImage(Request $request): JsonResponse
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'order_id' => 'required|digits_between:1,5',
                'image' => [
                    'required',
                    'regex:/^data:image\/(\w+);base64,([A-Za-z0-9+\/=]+)$/',
                ],
                'transaction_no' => 'required|string|between:1,50'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            $path = 'member/' . $request->ID. '/deposit';
            $image_url = CloudFlareHelper::uploadBase64File($request->image, $path);

            $service = new DepositService();
            $service->updateImageAndStatus($request->ID, $request->order_id, $image_url, $request->transaction_no);

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000')
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
     * 充值清單
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

            $service = new DepositService();
            list($total, $result) = $service->getOrders($request);

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
     * 充值訂單明細
     *
     * @bodyParam order_id integer require 訂單ID
     *
     */
    public function getOrder(Request $request): JsonResponse
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'order_id' => 'required|digits_between:1,5'
            ]);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            $service = new DepositService();
            $data = $service->getOrder($request->ID, $request->order_id);

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'order' => $data
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
