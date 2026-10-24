<?php

namespace App\Http\Controllers\Member;

use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
use Exception;
use App\Services\AvatarService;
use App\Services\MemberService;
use Validator;

class AvatarController extends Controller
{
    public function getList(Request $request)
    {
        try {
            //取得avatar清單
            $avatarService = new AvatarService();
            $avatarList = $avatarService->getList();
            //取得個人avatar_id
            $memberService = new MemberService();
            $member = $memberService->find($request->ID);

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $avatarList,
                    'avatar_id' => $member->avatar_id
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


    public function set(Request $request)
    {
        try {
            $rules = [
                'avatar_id' => 'required|digits_between:0,10'
            ];
            $validator = Validator::make($request->all(), $rules, $this->validatorMessage);
            if (!$validator->passes()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            $avatarService = new AvatarService();
            $avatarService->check($request->avatar_id);

            $service = new MemberService();
            $service->setAvatar($request->ID, $request->avatar_id);

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
}
