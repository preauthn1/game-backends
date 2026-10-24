<?php

namespace App\Http\Controllers\Task;

use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
use Exception;
use Illuminate\Support\Facades\Validator;
use App\Services\TaskService;

class TaskController extends Controller
{
    public function getStatusDropdown(Request $request)
    {
        try {
            $list = array();
            $list[] = array('id' => 0, 'name' => __('param.disable'));
            $list[] = array('id' => 1, 'name' => __('param.enable'));

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
     * 定時任務列表
     *
     * @bodyParam command string 命令 Example: testTask
     * @bodyParam status int 狀態 Example: 1; 0:停用, 1:啟用
     * @bodyParam page integer required 第幾頁 Example: 1
     * @bodyParam per_page integer required 每頁筆數 Example: 10
     */
    public function getList(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'command' => 'sometimes|string|between:1,100',
                'status' => 'sometimes|integer|in:0,1',
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

            $service = new TaskService();
            list($total, $result) = $service->getList($request);

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $result,
                    'pagination' => $this->getPagination($total, $request->per_page, $request->page)
                ]
            ]);
        } catch (QueryException | Exception $exception) {
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
     * 定時任務詳情
     *
     * @bodyParam id integer required 定時任務ID Example: 33
     */
    public function view(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'id' => 'required|integer|between:1,999999999'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new TaskService();
            $data = $service->find($request->id);

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'data' => $data
                ]
            ]);
        } catch (QueryException | Exception $exception) {
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
     * 定時任務新增
     *
     * @bodyParam command string 命令 Example: testTask
     * @bodyParam expression string 時間表達式 Example: * * * * *
     * @bodyParam description string 描述 Example: 測試定時任務
     * @bodyParam status int 狀態 Example: 1; 0:停用, 1:啟用
     * @bodyParam info string 其他設定 Example: {"hoursApart":4}
     */
    public function create(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'command' => 'required|string|between:1,100',
                'expression' => 'required|string|between:9,13',
                'description' => 'sometimes|string|between:1,300',
                'status' => 'required|integer|in:0,1',
                'info' => 'sometimes|string|between:1,3000',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new TaskService();
            $service->create($request);

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000')
            ]);
        } catch (QueryException | Exception $exception) {
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
     * 定時任務修改
     *
     * @bodyParam id integer required 定時任務ID Example: 33
     * @bodyParam expression string 時間表達式 Example: * * * * *
     * @bodyParam description string 描述 Example: 測試定時任務
     * @bodyParam status int 狀態 Example: 1; 0:停用, 1:啟用
     * @bodyParam info string 其他設定 Example: {"hoursApart":4}
     */
    public function update(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'id' => 'required|integer|between:1,999999999',
                'expression' => 'sometimes|string|between:9,13',
                'description' => 'sometimes|string|between:1,300',
                'status' => 'sometimes|integer|in:0,1',
                'info' => 'sometimes|string|between:1,3000',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new TaskService();
            $service->update($request);
            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
            ]);
        } catch (QueryException | Exception $exception) {
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
