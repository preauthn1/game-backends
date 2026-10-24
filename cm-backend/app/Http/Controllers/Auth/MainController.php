<?php

namespace App\Http\Controllers\Auth;

use Illuminate\Http\Request;
use Illuminate\Database\QueryException;
use App\Http\Controllers\Controller;
use App\Helpers\Common;
use App\Services\AgentService;
use Exception;
use Validator;
use App\REDIS;
use App;
use Log;

class MainController extends Controller
{
    /**
     * 登入(不需要token)
     *
     * @bodyParam token string required TOKEN Example: kNnUttcNDcVbZihUkwlCXRUL9guv7vmA
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *        "AGENT_IDS": [
     *              "1",
     *              "100001"
     *          ],
     *      "HIERARCHY_PATH": "/1/100001",
     *      "ID": 100001,
     *      "PARENT": 1,
     *      "TOKEN": "kNnUttcNDcVbZihUkwlCXRUL9guv7vmA",
     *      "LANGUAGE": 1,
     *      "ORGANIZATION_ID": 1,
     *      "CUSTOMIZED_SETTING_GROUP": 1,
     *      "CASH_FLOW_GROUP": 2,
     *      "SMS_GATEWAY_GROUP": 3
     *      "MINER_GROUP": 4
     *    }
     * }
     */
    public function Authenticate(Request $request)
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'token' => 'required|string|between:32,64'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            // 2. 檢查TOKEN
            $token = [REDACTED];

            $language = App::getLocale(); // 語系

            $agentService = new AgentService();
            $agentAccessToken = [REDACTED];
            $agent = $agentService->find($agentAccessToken->agent_id);

            if ($agent->host_id > 0) {
                $parent = $agentService->find($agent->host_id);
                $hierarchy_path = $parent->hierarchy_path;
            } else {
                $hierarchy_path = $agent->hierarchy_path;
            }

            // 刪除沒有在加密中的參數
            foreach ($request->request as $key => $val) {
                $request->request->remove($key);
            }

            // 加上通用資料
            $request->merge(array(
                'AGENT_IDS' => explode('/', substr($hierarchy_path, 1)),
                'HIERARCHY_PATH' => $hierarchy_path,
                'HOST_ID' => $agent->host_id,
                'ID' => $agent->id,
                'PARENT' => $agent->parent,
                'TOKEN' => $token,
                'LANGUAGE' => Common::getLanguageId($language),
                'ORGANIZATION_ID' => $agent->organization_id,
                'ROLE_ID' => $agent->role_id,
                'CUSTOMIZED_SETTING_GROUP' => $agent->customized_setting_group_id,
                'CASH_FLOW_GROUP' => $agent->cash_flow_group_id,
                'SMS_GATEWAY_GROUP' => $agent->sms_gateway_group_id,
                'MINER_GROUP' => $agent->miner_group_id
            ));

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => $request->all()
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
