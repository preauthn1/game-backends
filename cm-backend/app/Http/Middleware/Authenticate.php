<?php

namespace App\Http\Middleware;

use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use App\Helpers\Common;
use App\Services\AgentService;
use App\Services\MemberService;
use App\Services\MemberAccessTokenService;
use App\AES;
use Closure;
use App;
use Exception;

class Authenticate
{

    /**
     * Handle an incoming request.
     */
    public function handle(Request $request, Closure $next)
    {
        try {
            $start_time = microtime(true);
            list($url, $action) = explode("api/", $request->getRequestUri());
            $language = $request->header('Accept-Language'); // 語系
            // 設定語系
            if (App::getLocale() != $request->header('Accept-Language') && $request->header('Accept-Language') != '') {
                App::setLocale($request->header('Accept-Language'));
            }
            // 1. 檢查Key是否正確
            $params = str_replace(' ', '+', $request->params);
            $params = str_replace('\\', '', $params);
            ///Common::log(0, $request->header(), $action . 'header');
            if ($request->key != md5($params . env('API_MD5_KEY'))) {
                throw new Exception(__('error_message.9999'), 9999);
            }

            // 2. 檢查TOKEN
            $token = [REDACTED]'Bearer ', '', $request->header('Authorization'));

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

            // 5.解密param
            $aes = new AES(env('API_AES_KEY'), env('API_IV'));
            $param = json_decode($aes->decrypt($params), true);
            if (is_array($param)) {
                $request->merge($param);
            }
            // 加上通用資料
            $request->merge(array(
                'AGENT_IDS' => explode('/', substr($agent->hierarchy_path, 1)),
                'HIERARCHY_PATH' => $hierarchy_path,
                'ID' => $agent->id,
                'PARENT' => $agent->parent,
                'CODE' => 1000,
                'TOKEN' => $token,
                'LANGUAGE' => Common::getLanguageId($language),
                'ORGANIZATION_ID' => $agent->organization_id,
                'CUSTOMIZED_SETTING_GROUP' => $agent->customized_setting_group_id,
                'CASH_FLOW_GROUP' => $agent->cash_flow_group_id,
                'SMS_GATEWAY_GROUP' => $agent->sms_gateway_group_id,
                'MINER_GROUP' => $agent->miner_group_id
            ));

            $response = $next($request);
            $result = json_decode(json_encode($response, true), JSON_UNESCAPED_UNICODE);
            $end_time = microtime(true);
            $log = [
                'request' => $request->all(),
                'response' => $result,
                'start_time' => $start_time,
                'end_time' => $end_time
            ];
            Common::log($agent->id, $log, $action);
            return $response;
        } catch (QueryException $exception) {
            $info = $exception->errorInfo;
            Common::log(0, [
                'code' => $info[0],
                'message' => $info[2],
                'message1' => $exception->getMessage(),
                'file' => $exception->getFile(),
                'line' => $exception->getLine()
            ], $action);
            return response()->json([
                'code' => $info[0],
                'message' => $info[2]
            ]);
        } catch (Exception $exception) {
            Common::log(0, [
                'code' => $exception->getCode(),
                'message' => $exception->getMessage(),
                'file' => $exception->getFile(),
                'line' => $exception->getLine()
            ], $action);
            return response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
    }
}
