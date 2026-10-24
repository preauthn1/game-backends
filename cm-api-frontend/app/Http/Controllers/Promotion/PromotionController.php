<?php

namespace App\Http\Controllers\Promotion;

use Illuminate\Http\Request;
use App\Enums\PromotionEnum;
use App\Http\Controllers\Controller;
use App\Services\PromotionService;

class PromotionController extends Controller
{
    public function getList(Request $request)
    {
        try {
            $service = new PromotionService();
            $result = $service->getList($request->MINER_GROUP, $request->LANGUAGE);

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $result
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

    public function getPopup(Request $request)
    {
        try {
            $service = new PromotionService();
            $result = $service->getList(
                $request->AGENT_IDS, $request->LANGUAGE, PromotionEnum::TYPE_POPUP
            );

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $result
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
