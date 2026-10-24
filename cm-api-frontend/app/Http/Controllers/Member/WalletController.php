<?php

namespace App\Http\Controllers\Member;

use App\Http\Controllers\Controller;
use Illuminate\Http\Request;
use Illuminate\Database\QueryException;
use App\Services\MemberService;
use App\Services\WalletTypeService;
use Exception;
use Validator;

class WalletController extends Controller
{
    public function getList(Request $request)
    {
        try {
            $service = new MemberService();
            $data = $service->getWallet($request->ID, 0, $request->LANGUAGE);

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $data,
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
