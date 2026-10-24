<?php

namespace App\Http\Controllers\Record;

use Illuminate\Http\Request;
use Illuminate\Database\QueryException;
use App\Http\Controllers\Controller;
use App\Services\ReportService;
use Exception;
use Illuminate\Support\Facades\Validator;

class FundController extends Controller
{
    /**
     * 額度變更類型清單
     */
    public function getDropdown(Request $request)
    {
        try {
            //transaction_type.
            $list = array();
            $list[] = array('id' => 1, 'name' => __('param.transaction_type.1'));
            $list[] = array('id' => 2, 'name' => __('param.transaction_type.2'));
            $list[] = array('id' => 3, 'name' => __('param.transaction_type.3'));
            $list[] = array('id' => 4, 'name' => __('param.transaction_type.4'));
            $list[] = array('id' => 5, 'name' => __('param.transaction_type.5'));
            $list[] = array('id' => 6, 'name' => __('param.transaction_type.6'));
            $list[] = array('id' => 7, 'name' => __('param.transaction_type.7'));
            $list[] = array('id' => 8, 'name' => __('param.transaction_type.8'));
            $list[] = array('id' => 9, 'name' => __('param.transaction_type.9'));
            $list[] = array('id' => 10, 'name' => __('param.transaction_type.10'));
            $list[] = array('id' => 11, 'name' => __('param.transaction_type.11'));
            $list[] = array('id' => 12, 'name' => __('param.transaction_type.12'));
            $list[] = array('id' => 13, 'name' => __('param.transaction_type.13'));
            $list[] = array('id' => 14, 'name' => __('param.transaction_type.14'));
            $list[] = array('id' => 15, 'name' => __('param.transaction_type.15'));
            $list[] = array('id' => 16, 'name' => __('param.transaction_type.16'));
            $list[] = array('id' => 17, 'name' => __('param.transaction_type.17'));
            $list[] = array('id' => 18, 'name' => __('param.transaction_type.18'));

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $list
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
     * 資金紀錄
     *
     * @bodyParam type integer 類型 1=充值; 2=提現; 3=挖礦收益; 4=推薦獎勵; Example: 1
     * @bodyParam days integer 天數 Example: 1
     * @bodyParam page integer required 第幾頁 Example: 1
     * @bodyParam per_page integer required 每頁筆數 Example: 30
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
                'type' => 'sometimes|digits_between:1,2',
                'days' => 'numeric|between:1,100',
                'page' => 'required|integer|between:1,1000',
                'per_page' => 'required|integer|between:1,10000'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            $service = new ReportService();
            list($total, $result) = $service->getFundList($request);

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
}
