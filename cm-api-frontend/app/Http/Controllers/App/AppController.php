<?php

namespace App\Http\Controllers\App;

use App\Helpers\Common;
use App\Services\AgentDomainService;
use App\Services\AppService;
use App\Services\AgentService;
use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Validator;
use App\Http\Controllers\Controller;
use Exception;

class AppController extends Controller
{
    public function getInfoByDomain(Request $request)
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
            $agentDomainService = new AgentDomainService();
            $agentService = new AgentService();
            $appService = new AppService();
            $agent_id = $agentDomainService->getAgentIdByDomain($request->domain);
            $agent = $agentService->find($agent_id);
            $request->merge([
                'CUSTOMIZED_SETTING_GROUP' => $agent->customized_setting_group_id,
            ]);
            $data = $appService->getInfo($request);
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

    public function getInfo(Request $request)
    {
        try {
            $appService = new AppService();
            $data = $appService->getInfo($request);
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
