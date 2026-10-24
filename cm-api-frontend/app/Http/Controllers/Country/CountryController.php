<?php

namespace App\Http\Controllers\Country;

use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Validator;
use App\Http\Controllers\Controller;
use App\Services\CountryService;
use App\Services\AgentDomainService;
use App\Services\AgentService;
use Exception;

class CountryController extends Controller
{
    /**
     * 獲取手機國碼列表
     *
     * @bodyParam domain string required 網域
     * @bodyParam type integer 類型: 0=配置的國碼列表; 1=支援簡訊的國碼列表;
     */
    public function getList(Request $request)
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'domain' => 'required|domain',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            $sms_gateway_group_id = $request->SMS_GATEWAY_GROUP ?? '';
            if (empty($sms_gateway_group_id)) {
                //查組織ID
                $agentDomainService = new AgentDomainService();
                $agent_id = $agentDomainService->getAgentIdByDomain($request->domain);
                $agentService = new AgentService();
                $agent = $agentService->find($agent_id);
                $organization_id = is_object($agent) ? $agent->organization_id : 0;
                if ($organization_id == 0) {
                    throw new Exception(__('error_message.9997'), 9997);
                }
                $sms_gateway_group_id = $agent->sms_gateway_group_id;
            }

            $service = new CountryService();
            if (1 == $request->type) {
                $list = $service->getListBySms($sms_gateway_group_id);
            } else {
                $list = $service->getList($sms_gateway_group_id);
            }

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
}
