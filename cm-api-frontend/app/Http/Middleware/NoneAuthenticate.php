<?php

namespace App\Http\Middleware;

use Closure;
use Illuminate\Http\Request;
use App;
use App\AES;
use App\Helpers\Common;
use App\Services\AgentService;
use App\Services\MemberService;

class NoneAuthenticate
{
    /**
     * Handle an incoming request.
     */
    public function handle(Request $request, Closure $next)
    {
        try {
            $language = $request->header('Accept-Language');
            // 設定語系
            if (App::getLocale() != $language && $language != '') {
                App::setLocale($language);
            }
            // 1. 檢查Key是否正確
            $params = str_replace(' ', '+', $request->params);
            $params = str_replace('\\', '', $params);
            if ($request->key == md5($params . env('API_MD5_KEY'))) {
                // 刪除沒有在加密中的參數
                foreach ($request->request as $key => $val) {
                    $request->request->remove($key);
                }
                // 2.解密param
                $aes = new AES(env('API_AES_KEY'), env('API_IV'));
                $param = json_decode($aes->decrypt($params), true);
                if (is_array($param)) {
                    $request->merge($param);
                }

                // 3. 檢查TOKEN
                $token = [REDACTED]'Bearer ', '', $request->header('Authorization'));
                if (!empty($token)) {
                    $memberService = new MemberService();
                    $memberAccessToken = [REDACTED];
                    $member = $memberService->find($memberAccessToken->member_id);
                    $agentService = new AgentService();
                    $agent = $agentService->find($member->agent_id);
                    $request->merge(array(
                        'ID' => $member->id,
                        'AGENT_ID' => $member->agent_id,
                        'AGENT_IDS' => explode('/', substr($agent->hierarchy_path, 1)),
                        'HIERARCHY_PATH' => $agent->hierarchy_path,
                        'ORGANIZATION_ID' => $agent->organization_id,
                        'CUSTOMIZED_SETTING_GROUP' => $agent->customized_setting_group_id,
                        'CASH_FLOW_GROUP' => $agent->cash_flow_group_id,
                        'SMS_GATEWAY_GROUP' => $agent->sms_gateway_group_id,
                        'MINER_GROUP' => $agent->miner_group_id
                    ));
                }

                // 加上通用資料
                $device = Common::checkIsMobile();
                $request->merge(array(
                    'LANGUAGE' => Common::getLanguageId($language),
                    'IS_MOBILE' => $device > 0,
                    'IS_PAD' => $device == 2
                ));
                $code = 1000;
            } else {
                $code = 9999;
                $message = __('error_message.9999');
            }
            list($url, $action) = explode("api/", $request->getRequestUri());
            Common::log(0, $request->all(), $action);
            if ($code == 1000) {
                $response = $next($request);
                $result = json_decode(json_encode($response, true), JSON_UNESCAPED_UNICODE);

                if (!in_array($request->getRequestUri(), ['verification/getCaptcha'])) {
                    $log = [
                        'request' => $request->all(),
                        'response' => $result
                    ];
                    Common::log(0, $log, $action);
                }

                return $response;
            } else {
                $array = [
                    'code' => $code,
                    'message' => $message
                ];
                Common::log(0, $array, $action);
                return response()->json($array);
            }
        } catch (\Illuminate\Database\QueryException $e) {
            $info = $e->errorInfo;
            return response()->json([
                'code' => $info[0],
                'message' => $info[2]
            ]);
        } catch (\Exception $e) {
            return response()->json([
                'code' => $e->getCode(),
                'message' => $e->getMessage()
            ]);
        }
    }
}
