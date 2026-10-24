<?php

namespace App\Http\Controllers\Auth;

use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
use App\Services\MenuService;
use Exception;
use Validator;

class MenuController extends Controller
{
    /**
     * 選單
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *      "list": [
     *          {
     *              "mm_id": 709,
     *              "member_id": 90,
     *              "username": "puffgff001",
     *              "name": "PS5 Pro",
     *              "rent_price": "111.0000",
     *              "parent_id": null,
     *              "parent_username": null,
     *              "status": 1,
     *              "start_time": "2024-10-02 00:00",
     *              "end_time": "2024-10-29 23:59",
     *              "cycle_days": 4,
     *              "statusTxt": "进行中",
     *          }
     *      ]
     *    }
     * }
     */
    public function getList(Request $request)
    {
        try {
            $service = new MenuService();
            list($list, $totalUri) = $service->getAgentMenu($request->ID);

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $list,
                    'totalUri' => $totalUri
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
