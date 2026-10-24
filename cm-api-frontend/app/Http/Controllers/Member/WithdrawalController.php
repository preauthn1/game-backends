<?php

namespace App\Http\Controllers\Member;

use App\Services\MemberService;
use Exception;
use App\Http\Controllers\Controller;
use App\Models\MemberBankAccount;
use App\Models\MemberCryptoAccount;
use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\Validator;
use Illuminate\Http\JsonResponse;

class WithdrawalController extends Controller
{
    /**
     * 創建會員銀行卡
     *
     * @bodyParam bank_id integer required 銀行系統編號 Example: 1
     * @bodyParam account_name string required 銀行帳戶名 Example: "LeahMiner"
     * @bodyParam account_no string required 銀行帳戶 Example: "15454578451634802"
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     * }
     */
    public function createBank(Request $request): JsonResponse
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'bank_id' => 'required|digits_between:1,5',
                'account_name' => 'required|string',
                'account_no' => 'required|string|between:1,20',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            $memberService = new MemberService();
            //存在使用中的銀行帳戶
            $memberActiveBank = $memberService->getActiveBankAccount($request->ID);
            if (is_object($memberActiveBank)) {
                throw new Exception(__('error_message.1304'), 1304);
            }
            //銀行帳戶是否唯一
            $memberService->checkBankAccountExists($request);

            DB::beginTransaction();
            $result = $memberService->createBankAccount($request);
            DB::commit();
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
            ]);
        } catch (QueryException $exception) {
            DB::rollback();
            $info = $exception->errorInfo;
            return response()->json([
                'code' => $info[0],
                'message' => $info[2]
            ]);
        } catch (Exception $exception) {
            DB::rollback();
            return response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
    }

    /**
     * 創建會員加密貨幣錢包地址
     *
     * @bodyParam bank_id string required 銀行系統編號 Example: 2
     * @bodyParam crypto_address string required 錢包地址 Example: "password123"
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     * }
     */
    public function createCryptos(Request $request): JsonResponse
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'bank_id' => 'required|digits_between:1,5',
                'crypto_address' => 'required|string',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            $memberService = new MemberService();
            //存在使用中的錢包地址
            $memberActiveCrypto = $memberService->getActiveCryptoAccount($request->ID);
            if (is_object($memberActiveCrypto)) {
                throw new Exception(__('error_message.1305'), 1305);
            }
            //錢包地址是否唯一
            $memberService->checkCryptoAccountExists($request);

            DB::beginTransaction();
            $result = $memberService->createCryptoAccount($request);
            DB::commit();
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
            ]);
        } catch (QueryException $exception) {
            DB::rollback();
            $info = $exception->errorInfo;
            return response()->json([
                'code' => $info[0],
                'message' => $info[2]
            ]);
        } catch (Exception $exception) {
            DB::rollback();
            return response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
    }
}
