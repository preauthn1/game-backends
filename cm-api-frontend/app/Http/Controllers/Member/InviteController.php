<?php

namespace App\Http\Controllers\Member;

use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
use App\Services\MemberService;

class InviteController extends Controller
{
    /**
     * 取得邀請資訊
     *
     */
    public function getInvite(Request $request)
    {
        try {
            $service = new MemberService();
            $member = $service->find($request->ID);
            $rebates = $service->getRebates($request->ID);
            $rewards = $service->getRewards($request->ID);

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'invite_code' => $member->invitation_code,
                    'rebate' => $rebates,
                    'reward' => $rewards
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
