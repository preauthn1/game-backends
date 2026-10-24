<?php

namespace App\Http\Controllers\Report;

use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
use Exception;
use Illuminate\Support\Carbon;
use Illuminate\Support\Facades\Validator;
use App\Services\ReportService;
use App\Services\AgentService;

class ReportController extends Controller
{
    /**
     * 報表 - 數據區間日報表
     *
     * @bodyParam manager_name string 管理員帳號 Example: mybittest
     * @bodyParam type integer 報表類型 Example: 1, 1:彙總, 2:詳情
     * @bodyParam start_date datetime 開始日期 Example: 2024-12-01
     * @bodyParam end_date datetime 結束日期 Example: 2024-12-31
     *
     */
    public function getReport(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'manager_name' => 'sometimes|string|between:4,20',
                'type' => 'required|integer|in:1,2',
                'start_date' => 'required|date_format:Y-m-d',
                'end_date' => 'required|date_format:Y-m-d|after_or_equal:start_time',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            //最多查詢30天區間
            $reportStart = Carbon::parse($request->start_date)->format('Y-m-d 00:00:00');
            $reportEnd = Carbon::parse($request->end_date)->format('Y-m-d 23:59:59');
            if (Carbon::parse($reportStart)->diffInDays(Carbon::parse($reportEnd)) > 31) {
                throw new Exception(__('error_message.4003'), 4003);
            }

            //管理員資訊
            if (!empty($request->manager_name)) {
                $agentService = new AgentService();
                $manager = $agentService->findByUsername($request->manager_name);
                //管理員帳號不存在
                if (empty($manager)) {
                    throw new Exception(__('error_message.5051'), 5051);
                }
                //只可查自己或自己下線得管理員
                if ($manager->host_id > 0) {
                    $parent = $agentService->find($manager->host_id);
                    $request->hierarchy_path = $parent->hierarchy_path;
                } else {
                    $request->hierarchy_path = $manager->hierarchy_path;
                }
                if (!str_contains($request->hierarchy_path, $request->HIERARCHY_PATH)) {
                    throw new Exception(__('error_message.5051'), 5051);
                }
            }

            $service = new ReportService();
            list($data, $statistic) = $service->getDaysReport($reportStart, $reportEnd, $request);

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'data' => $data,
                    'statistic' => $statistic,
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
}
