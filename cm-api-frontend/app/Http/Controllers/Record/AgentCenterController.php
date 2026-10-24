<?php

namespace App\Http\Controllers\Record;

use Illuminate\Http\Request;
use Illuminate\Database\QueryException;
use App\Http\Controllers\Controller;
use App\Services\MemberService;
use App\Services\MinerService;
use App\Services\ReportService;
use Exception;
use Validator;

class AgentCenterController extends Controller
{
    /**
     * Agent Center
     *
     * @bodyParam days integer 天數 Example: 1
     * @bodyParam level integer Level Example: 1
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *    }
     * }
     */
    public function getList(Request $request)
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'days' => 'numeric|between:1,100',
                'level' => 'numeric|between:1,100',
                'miner_id' => 'integer|between:1,100',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            $memberService = new MemberService();
            list($team, $team_count) = $memberService->getTeamList($request);
            $minerService = new MinerService();
            $sales = $minerService->getSalesList($request);

            $sales_rebate = 0;
            $referral_reward = 0;

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'team_count' => $team_count,
                    'sales_rebate' => $sales_rebate,
                    'referral_reward' => $referral_reward,
                    'team' => $team,
                    'sales' => $sales
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
