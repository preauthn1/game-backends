<?php

namespace App\Http\Controllers\MemberMiner;

use Illuminate\Http\Request;
use Illuminate\Database\QueryException;
use App\Http\Controllers\Controller;
use App\Services\RebateService;
use Exception;
use Illuminate\Support\Facades\Validator;

class RebateController extends Controller
{
    /**
     * 反佣信息
     *
     * @bodyParam username string 會員名稱 Example: abc
     * @bodyParam account string 下級會員 Example: def
     * @bodyParam name string 礦機名稱 Example: s19
     * @bodyParam start_time datetime 創建開始時間 Example: 2020-01-01 00:00
     * @bodyParam end_time datetime 創建結束時間 Example: 2020-01-02 00:00
     * @bodyParam page integer required 第幾頁 Example: 1
     * @bodyParam per_page integer required 每頁筆數 Example: 30
     *
     */
    public function getList(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'username' => 'sometimes|string|between:1,1000',
                'account' => 'sometimes|string|between:1,1000',
                'name' => 'sometimes|string|between:1,20',
                'start_time' => 'date_format:Y-m-d H:i',
                'end_time' => 'date_format:Y-m-d H:i|after_or_equal:start_time',
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

            $service = new RebateService();
            list($total, $result) = $service->getList($request);

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $result,
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
