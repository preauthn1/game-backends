<?php

namespace App\Http\Controllers\Auth;

use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
use App\Services\AgentService;
use App\Services\LanguageService;
use App\Helpers\Common;
use Exception;
use Validator;

class TokenController extends Controller
{
    /**
 * 登入(不需要token)
 *
 * @bodyParam token string required TOKEN Example: username
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
    public function token($token, $params = array(), $language = 'zh-tw')
    {
        try {
            $agentService = new AgentService();
            $agentAccessToken = [REDACTED];
            $agent = $agentService->find($agentAccessToken->agent_id);

            if ($agent->host_id > 0) {
                $parent = $agentService->find($agent->host_id);
                $hierarchy_path = $parent->hierarchy_path;
            } else {
                $hierarchy_path = $agent->hierarchy_path;
            }

            $languageService = new LanguageService();
            $language_id = $languageService->getLanguageIdByCode($language);

            // 加上通用資料
            $params->merge(array(
                'AGENT_IDS' => explode('/', substr($agent->hierarchy_path, 1)),
                'HIERARCHY_PATH' => $hierarchy_path,
                'HOST_ID' => $agent->host_id,
                'ID' => $agent->id,
                'PARENT' => $agent->parent,
                'CODE' => 1000,
                'TOKEN' => $token,
                'LANGUAGE' => $language_id,
                'ORGANIZATION_ID' => $agent->organization_id,
                'CUSTOMIZED_SETTING_GROUP' => $agent->customized_setting_group_id,
                'CASH_FLOW_GROUP' => $agent->cash_flow_group_id,
                'SMS_GATEWAY_GROUP' => $agent->sms_gateway_group_id,
                'MINER_GROUP' => $agent->miner_group_id
            ));

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => $params
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
