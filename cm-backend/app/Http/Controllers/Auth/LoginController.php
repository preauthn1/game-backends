<?php

namespace App\Http\Controllers\Auth;

use Illuminate\Http\Request;
use Illuminate\Database\QueryException;
use App\Http\Controllers\Controller;
use App\Helpers\Common;
use App\Services\AgentService;
use Exception;
use Validator;

class LoginController extends Controller
{
    /**
     * 登入(不需要token)
     *
     * @bodyParam username string required 代理帳號 Example: username
     * @bodyParam password string required 代理密碼 Example: abc123456
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *        "id": 1,
     *        "username": "Admin",
     *        "token": "roOAsDmfT9nP1Y9uKD8aMylk6HcYPlfz"
     *    }
     * }
     */
    public function login(Request $request)
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'username' => 'required|string|between:2,20',
                'password' => 'required|string|between:6,12',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            $service = new AgentService();
            $agent = $service->findByUsername($request->username, true);
            $service->checkPassword($request->password, $agent->password);

            $ip = Common::getUserIp();
            $token = [REDACTED];
            $service->updateLastLogin($agent->id, $ip);
            $service->addLoginLog($agent->id, $ip);

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'id' => $agent->id,
                    'username' => $agent->username,
                    'token' => $token
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
     * 登出
     *
     * @bodyParam username string required 代理帳號 Example: username
     * @bodyParam password string required 代理密碼 Example: abc123456
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS"
     * }
     */
    public function logout(Request $request)
    {
        try {
            $service = new AgentService();
            $service->setTokenDisable($request->ID);

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
