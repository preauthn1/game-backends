<?php

namespace App\Http\Controllers\AgentGroup;

use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
use Exception;
use App\Services\GroupDefineService;
use Illuminate\Support\Facades\Validator;

class GroupDefineController extends Controller
{

    public function getDataTypeDropdown(Request $request)
    {
        try {
            $list = array();
            $list[] = array('code' => 'integer', 'name' => __('param.data_type.integer'));
            $list[] = array('code' => 'string', 'name' => __('param.data_type.string'));
            $list[] = array('code' => 'boolean', 'name' => __('param.data_type.boolean'));
            $list[] = array('code' => 'decimal', 'name' => __('param.data_type.decimal'));

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
     * 列表
     *
     * @bodyParam group_type integer 類型 Example: 1
     */
    public function getList(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'group_type' => 'required|digits_between:1,10',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new GroupDefineService();
            $result = $service->getList($request);

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $result,
                    'pagination' => [
                        'pages' => count($result),
                        'total' => count($result),
                        'current' => 1,
                    ],
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

    /**
     * 詳情
     *
     * @bodyParam define_id integer 定義ID Example: 123
     */
    public function view(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'define_id' => 'required|digits_between:1,999999999',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new GroupDefineService();
            $data = $service->view($request);

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'data' => $data,
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

    /**
     * 新增定義
     *
     * @bodyParam define_id integer 定義ID Example: 2000
     * @bodyParam parent_define_id integer 上級定義ID Example: 0
     * @bodyParam group_type integer 組別類型(1:個性化 2:支付 3:簡訊) Example: 1
     * @bodyParam data_type string 資料型態(integer:整數 string:字串 boolean:布林 decimal:小數) Example: boolean
     * @bodyParam default_value string 預設值 Example: 1
     * @bodyParam language_text string 語言文字 Example: {"en":"test","cn":"測試"}
     * @bodyParam memo string 備註 Example: 個性化配置
     */
    public function create(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'define_id' => 'required|digits_between:1,999999999',
                'parent_define_id' => 'required|digits_between:0,999999999',
                'group_type' => 'required|digits_between:1,10',
                'data_type' => 'sometimes|string|between:3,100',
                'default_value' => 'sometimes|string|between:1,200',
                'language_text' => 'sometimes|string|between:42,1000',
                'memo' => 'sometimes|string|between:1,400',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new GroupDefineService();
            $service->create($request);

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000')
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

    /**
     * 修改定義
     *
     * @bodyParam define_id integer 定義ID Example: 2000
     * @bodyParam language_text string 語言文字 Example: {"en":"test","cn":"測試"}
     * @bodyParam default_value string 預設值 Example: 1
     * @bodyParam memo string 備註 Example: 個性化配置
     */
    public function update(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'define_id' => 'required|digits_between:1,999999999',
                'default_value' => 'sometimes|string|between:1,200',
                'language_text' => 'sometimes|string|between:42,1000',
                'memo' => 'sometimes|string|between:0,400',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new GroupDefineService();
            $service->update($request);

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
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
