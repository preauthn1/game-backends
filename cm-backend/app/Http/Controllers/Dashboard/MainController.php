<?php

namespace App\Http\Controllers\Dashboard;

use App\Enums\PublicEnum;
use Illuminate\Http\Request;
use Illuminate\Database\QueryException;
use App\Http\Controllers\Controller;
use App\Services\DepositService;
use App\Services\WithdrawalService;
use App\Services\MemberService;
use Exception;
use Illuminate\Support\Carbon;
use Validator;

class MainController extends Controller
{
    /**
     * 數據統計 - 儀表板數據
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *    }
     * }
     */
    public function getInfo(Request $request)
    {
        $start = microtime(true);
        try {
            $memberService = new MemberService();
            $memberCount = $memberService->getMemberCount($request);
            $total_registration = $memberCount->registration_count;
            $total_deposit_number = $memberCount->deposit_count;
            $total_withdrawal_number = $memberCount->withdrawal_count;
            $total_balance = $memberCount->balance;
            $total_residual_value = $memberCount->residual_value;
            $today_registration = $memberService->getTodayRegistrationCount($request);

            $depositService = new DepositService();
            list($deposit_list_7, $deposit_list_30) = $depositService->getDataCollection($request);
            $today_depositor = number_format($depositService->getSummary($request, 'member_id', 'today'));
            $today_deposit_amount = number_format($depositService->getSummary($request, 'point', 'today', 'sum'),PublicEnum::DefaultScale);
            $total_deposit_amount = number_format($depositService->getSummary($request, 'point', 'total', 'sum'),PublicEnum::DefaultScale);

            $withdrawalService = new WithdrawalService();
            list($withdrawal_list_7, $withdrawal_list_30) = $withdrawalService->getDataCollection($request);
            $today_withdrawal_amount = number_format($withdrawalService->getSummary($request, 'point', 'today', 'sum'),PublicEnum::DefaultScale);
            $total_withdrawal_amount = number_format($withdrawalService->getSummary($request, 'point', 'total', 'sum'),PublicEnum::DefaultScale);

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    //會員總註冊數
                    'total_registration' => $total_registration,
                    //今日註冊人數
                    'today_registration_number' => $today_registration,

                    //今日充值人數
                    'today_depositor_number' => $today_depositor,
                    //今日充值金額
                    'today_deposit_amount' => $today_deposit_amount,
                    //平台總充值金額
                    'total_deposit_amount' => $total_deposit_amount,
                    //总充值人数
                    'total_deposit_number' => $total_deposit_number,
                    //近7日近30日每日充值情況
                    'deposit_list_7' => $deposit_list_7,
                    'deposit_list_30' => $deposit_list_30,

                    //今日提現金額
                    'today_withdrawal_amount' => $today_withdrawal_amount,
                    //平台總提現金額
                    'total_withdrawal_amount' => $total_withdrawal_amount,
                    //总提现人数
                    'total_withdrawal_number' => $total_withdrawal_number,
                    //近7日近30日提現情況
                    'withdrawal_list_7' => $withdrawal_list_7,
                    'withdrawal_list_30' => $withdrawal_list_30,

                    //总余额(下级账号里的所有钱包余额加上矿机残值)
                    'total_balance' => number_format(($total_balance + $total_residual_value),PublicEnum::DefaultScale),
                    'balance' => $total_balance,
                    'residual_value' => $total_residual_value
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
}
