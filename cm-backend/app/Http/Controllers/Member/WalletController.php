<?php

namespace App\Http\Controllers\Member;

use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
use App\Services\MemberService;
use App\Services\MemberWalletService;
use App\Services\TransactionLogService;
use App\Enums\WalletTypeEnum;
use Exception;
use Illuminate\Support\Facades\Validator;
use Illuminate\Support\Facades\DB;

class WalletController extends Controller
{
    /**
     * 會員管理 - 會員錢包
     *
     * @bodyParam member_id integer 會員ID Example: 1
     * @bodyParam username string 會員ID Example: abc
     * @bodyParam start_time datetime 開始時間 Example: 2020-01-01 00:00
     * @bodyParam end_time datetime 結束時間 Example: 2020-01-02 00:00
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
    public function getLoginList(Request $request)
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'member_id' => 'required|digits_between:0,10',
                'username' => 'string|between:2,20',
                'start_time' => 'date_format:Y-m-d H:i',
                'end_time' => 'date_format:Y-m-d H:i|after_or_equal:start_time',
                'page' => 'required|integer|between:1,1000',
                'per_page' => 'required|integer|between:1,10000'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            $service = new MemberService();
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
     * 人工上分 / 人工下分 添加額度紀錄及更新錢包金額
     *
     * @bodyParam username string required 會員帳號 Example: leahminer03
     * @bodyParam transaction_type integer required 類型 12=上分; 13=下分; Example: 12
     * @bodyParam amount integer required 金額 Example: 123
     * @bodyParam memo string 備註 Example: test
     */
    public function adjust(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'username' => 'required|string|between:4,20',
                'transaction_type' => 'required|integer|in:12,13,16,17',
                'amount' => 'required|numeric|between:0,9999999999.9999999999',
                'memo' => 'sometimes|string|between:1,55',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new MemberService();
            $member = $service->findByUsername($request->username, true);

            $wallet_type_id = WalletTypeEnum::MAIN;
            $increaseTypes = [12, 16];

            DB::beginTransaction();
            //取錢包lock
            $memberWalletService = new MemberWalletService();
            $memberWallet = $memberWalletService->lockForUpdateByMemberId($member->id, $wallet_type_id);

            //insert transaction_logs
            $data = array();
            $data['wallet_type_id'] = $wallet_type_id;
            $data['member_id'] = $member->id;
            $data['transaction_type_id'] = $request->transaction_type;
            $data['order_id'] = 0;
            $data['point'] = $request->amount;
            $data['before_balance'] = $memberWallet->balance;
            $data['after_balance'] = in_array($request->transaction_type, $increaseTypes)
                ? ($memberWallet->balance + $request->amount)
                : ($memberWallet->balance - $request->amount);
            $data['memo'] = $request->memo ?? '';
            $transactionLogService = new TransactionLogService();
            $transactionLog = $transactionLogService->create(...$data);

            //更新錢包
            $data = array();
            $data['member_id'] = $member->id;
            $data['point'] = $request->amount;
            $data['wallet_type_id'] = $wallet_type_id;
            list($before_balance, $after_balance) = in_array($request->transaction_type, $increaseTypes)
                ? $memberWalletService->increment(...$data)
                : $memberWalletService->decrement(...$data);

            //update transaction_logs
            $transactionLogService->setSuccess($transactionLog->id, $member->id, $request->ID);

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
