<?php

namespace App\Http\Controllers\Member;

use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
use App\Services\MemberService;
use Exception;
use Illuminate\Support\Facades\Validator;

class AgentCenterController extends Controller
{
    /**
     * 下級級數下拉選單
     */
    public function getLevelDropdown(Request $request)
    {
        try {
            $list = array();
            $list[] = array('id' => 1, 'name' => __('param.member_child.level.1'));
            $list[] = array('id' => 2, 'name' => __('param.member_child.level.2'));
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $list
                ]
            ]);

        } catch (Exception $exception) {
            return response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
    }

    /**
     * 代理中心
     *
     * @bodyParam username string required 會員名稱 Example: abc
     * @bodyParam name string 礦機名稱 Example: s19
     * @bodyParam level integer 下級級數 Example: 1
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
     *      ],
     *      "pagination": {
     *          "per_page": 10,
     *          "total": 4,
     *          "pages": 1,
     *          "previous": "",
     *          "current": 1,
     *          "next": ""
     *      }
     *    }
     * }
     */
    public function view(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'username' => 'required|string|between:1,1000',
                'name' => 'sometimes|string|between:1,20',
                'level' => 'sometimes|numeric|between:0,10',
                'page' => 'required|integer|between:1,1000',
                'per_page' => 'required|integer|between:1,10000'
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
            list($total, $result, $summary, $level_detail, $max_level) = $service->getAgentCenter($request);

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'summary' => $summary,
                    'data' => $result,
                    'level' => $level_detail,
                    'max_level' => $max_level,
                    'pagination' => $this->getPagination($total, $request->per_page, $request->page)
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
