<?php

namespace App\Http\Controllers\Agent;

use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
use Exception;
use Illuminate\Support\Facades\Validator;
use App\Services\AgentService;
use Illuminate\Support\Facades\DB;

class AgentController extends Controller
{
    /**
     * 管理員-列表
     *
     * @bodyParam manager_id integer 管理員ID Example: 123
     * @bodyParam organization_id string 管理員組織ID Example: 2
     * @bodyParam parent_name string 管理員上級帳號 Example: admin
     * @bodyParam manager_name string 管理員帳號 Example: test
     * @bodyParam role_id integer 角色ID Example: 11
     * @bodyParam status int 狀態 Example: 1; 0:停用, 1:啟用
     * @bodyParam page integer required 第幾頁 Example: 1
     * @bodyParam per_page integer required 每頁筆數 Example: 30
     */
    public function getList(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'manager_id' => 'digits_between:0,10',
                'organization_id' => 'sometimes|digits_between:0,10',
                'parent_name' => 'sometimes|string|between:1,20',
                'manager_name' => 'sometimes|string|between:1,20',
                'role_id' => 'sometimes|digits_between:0,10',
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

            $agentService = new AgentService();
            list($total, $result) = $agentService->getList($request);

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

    /**
     * 管理員-詳情
     *
     * @bodyParam manager_id integer 管理員ID Example: 123
     */
    public function view(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'manager_id' => 'required|digits_between:1,10'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new AgentService();
            $data = $service->getAgent($request->manager_id);

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
     * 管理員 - 新增
     *
     * @bodyParam username string required 代理名稱 Example: abc
     * @bodyParam password string required 密碼 Example: abc
     * @bodyParam parent_username string 上級代理帳號 Example: test01
     * @bodyParam referral_code string 邀請碼 Example: abc
     * @bodyParam role_id integer required 角色 Example: 1
     * @bodyParam sub_account integer 子代理 Example: 1
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS"
     * }
     */
    public function create(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'username' => 'required|string|between:4,20',
                'password' => 'required|string|between:2,20',
                'parent_username' => 'sometimes|string|between:1,20',
                'referral_code' => 'sometimes|string|between:1,10',
                'role_id' => 'required|digits_between:0,10',
                'sub_account' => 'sometimes|integer|in:0,1',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new AgentService();
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
     * 管理員-修改
     *
     * @bodyParam manager_id integer 管理員ID Example: 123
     * @bodyParam role_id integer 角色ID Example: 11
     * @bodyParam customized_setting_group_id integer 个性化組別ID Example: 1
     * @bodyParam cash_flow_group_id integer 支付組別ID Example: 2
     * @bodyParam sms_gateway_group_id integer 短信組別ID Example: 3
     * @bodyParam miner_group_id integer 礦機組別ID Example: 4
     * @bodyParam password string 密碼 Example: 123456
     * @bodyParam status int 狀態 Example: 1; 0:停用, 1:啟用
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS"
     * }
     */
    public function update(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'manager_id' => 'required|digits_between:0,10',
                'role_id' => 'sometimes|digits_between:1,10',
                'customized_setting_group_id' => 'sometimes|digits_between:1,10',
                'cash_flow_group_id' => 'sometimes|digits_between:1,10',
                'sms_gateway_group_id' => 'sometimes|digits_between:1,10',
                'miner_group_id' => 'sometimes|digits_between:1,10',
                'password' => 'sometimes|string|between:6,20',
                'status' => 'sometimes|integer|in:0,1',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new AgentService();
            $service->updateManager($request);
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
