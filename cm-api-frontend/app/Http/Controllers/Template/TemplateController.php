<?php

namespace App\Http\Controllers\Template;

use App\Helpers\Common;
use App\Services\AgentDomainService;
use App\Services\TemplateService;
use App\Services\AgentService;
use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Validator;
use App\Http\Controllers\Controller;
use Exception;

class TemplateController extends Controller
{
    public function getTemplate(Request $request)
    {
        try {
            $rules = [
                'domain' => 'required|domain',
            ];
            // 驗證POST資料
            $validator = Validator::make($request->all(), $rules, $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }
            if (empty($request->CUSTOMIZED_SETTING_GROUP)) {
                $agentDomainService = new AgentDomainService();
                $agentService = new AgentService();
                $agent_id = $agentDomainService->getAgentIdByDomain($request->domain);
                $agent = $agentService->find($agent_id);
                $request->merge([
                    'CUSTOMIZED_SETTING_GROUP' => $agent->customized_setting_group_id,
                ]);
            }
            $templateService = new TemplateService();
            $data = $templateService->getTemplate($request);
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => $data
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
