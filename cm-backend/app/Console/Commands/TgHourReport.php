<?php

namespace App\Console\Commands;

use App\Enums\TimeEnum;
use Carbon\Carbon;
use Illuminate\Console\Command;
use Illuminate\Support\Facades\Log;
use App\Services\TaskService;
use App\Services\ReportService;
use App\Services\TelegramService;
use App\Services\AgentService;
use Illuminate\Http\Request;
use App\Helpers\NumberHelper;
use Exception;

class TgHourReport extends Command
{
    protected $signature = 'TgHourReport';

    protected $description = 'Use TG to send the hours report';

    protected $rankLimit = 10;

    protected $hoursApart = 4;

    protected $agentNameList = [];

    public function handle()
    {
        Log::info('###### TgHourReport 開始執行 ######');
        //取配置
        $taskService = new TaskService();
        $taskData = $taskService->findByCommand($this->signature, false);
        $hoursApart = $this->hoursApart;
        if (is_object($taskData)) {
            $info = json_decode(str_replace('\"', '\\', $taskData->info ?? ''), true);
            //排行數量 預設(10)
            if (!empty($info['rankLimit'] ?? 0)) {
                $this->rankLimit = $info['rankLimit'];
            }
            //小時間隔 若無用預設值(4)
            if (!empty($info['hoursApart'] ?? 0)) {
                $hoursApart = $info['hoursApart'];
            }
            //伺服器時間
            if (!empty($info['testTime'] ?? '')) {
                $testTime = Carbon::parse($info['testTime'], TimeEnum::TimeZone['backend']);
                Carbon::setTestNow($testTime);
            }
            //抓取哪些代理數據
            if (!empty($info['agentNameList'] ?? '')) {
                $this->agentNameList = explode(',', $info['agentNameList']);
            }
        }
        //計算開始結束時間
        $now = Carbon::now(TimeEnum::TimeZone['backend']);
        $addHoursApart = floor($now->hour / $hoursApart);
        $startTime = Carbon::now(TimeEnum::TimeZone['backend'])->startOfDay();
        $endTime = Carbon::parse($startTime)->addHours($addHoursApart * $hoursApart)->subHour()->endOfHour();
        if ($now->hour < 1) {
            $startTime = Carbon::now(TimeEnum::TimeZone['backend'])->subDay()->startOfDay();
            $endTime = Carbon::now(TimeEnum::TimeZone['backend'])->subDay()->endOfDay();
        }
        //串接請求
        $requestList = [
            'params' => [
                'now' => $now->toDateTimeString(),
                'nowHour' => $now->hour,
                'hoursApart' => $hoursApart,
                'start_time' => $startTime->toDateTimeString(),
                'end_time' => $endTime->toDateTimeString(),
            ],
            'basic' => [
                'start_time' => $startTime->toDateTimeString(),
                'end_time' => $endTime->toDateTimeString(),
            ],
        ];
        //分代理統計
        if (!empty($this->agentNameList)) {
            foreach ($this->agentNameList as $agentName) {
                if (!empty($agentName)) {
                    $agentService = new AgentService();
                    try {
                        $agent = $agentService->findByUsername($agentName);
                        if (is_object($agent)) {
                            if ($agent->host_id > 0) {
                                $parent = $agentService->find($agent->host_id);
                                $hierarchy_path = $parent->hierarchy_path;
                            } else {
                                $hierarchy_path = $agent->hierarchy_path;
                            }
                            $requestList['params']['agentName'] = $agentName;
                            $requestList['basic']['hierarchy_path'] = $hierarchy_path;
                        }
                    } catch (Exception $e) {
                        Log::error('###### TgHourReport 找不到代理 ' . $agentName . ' ######');
                    }
                }
                $this->doReport($requestList);
            }
        } else {
            $this->doReport($requestList);
        }

        Log::info('###### TgHourReport 結束執行 ######');
    }

    //充值加提現前10玩家
    private function getDepositWithdrawalRank($requestRank)
    {
        $result = [
            '',
            '[' . Carbon::parse($requestRank->start_time)->toDateString() . '充值加提現前' . $this->rankLimit . '玩家]',
        ];

        $requestRank->merge([
            'limit' => $this->rankLimit
        ]);
        $reportService = new ReportService();
        $dataD = $reportService->getMajorRank($requestRank);
        $rankD = [];
        $index = 1;
        if (is_object($dataD)) {
            $dataD->map(function ($item) use (&$rankD, &$index) {
                $rankD[] = $index . '. ' . $item->username . '(' . $item->agent_name . ') / ' . NumberHelper::formatDecimal(1, $item->total_deposit_amount, 2, true) . ' / ' . NumberHelper::formatDecimal(1, $item->total_withdrawal_amount, 2, true);
                $index++;
            });
        }
        return array_merge($result, $rankD);
    }

    //當日統計
    private function getSummary($requestSummary)
    {
        $result = [
            '',
            '[' . Carbon::parse($requestSummary->start_time)->toDateString() . '當日統計]',
        ];

        $reportService = new ReportService();
        $data = $reportService->getReport($requestSummary);
        $register = number_format($data[0]->regi_count ?? 0);
        $login = number_format($data[0]->login_count ?? 0);
        $active = number_format($data[0]->active_count ?? 0);
        $deposit = NumberHelper::formatDecimal(1, $data[0]->total_deposit_amount ?? 0, 2, true);
        $withdrawal = NumberHelper::formatDecimal(1, $data[0]->total_withdrawal_amount ?? 0, 2, true);
        $netDepositWithdrawal = NumberHelper::formatDecimal(1, bcsub($data[0]->total_deposit_amount ?? 0, $data[0]->total_withdrawal_amount ?? 0, 6), 2, true);
        $award = NumberHelper::formatDecimal(1, $data[0]->award_amount ?? 0, 2, true);
        $consume = NumberHelper::formatDecimal(1, bcadd($data[0]->total_purchase_amount ?? 0, $data[0]->total_rent_amount ?? 0, 6), 2, true);
        $income = NumberHelper::formatDecimal(1, bcadd($data[0]->total_purchase_income ?? 0, bcadd($data[0]->total_group_income ?? 0, bcadd($data[0]->total_rent_income ?? 0, bcadd($data[0]->total_reward_income ?? 0, $data[0]->total_rebate_income ?? 0, 6), 6), 6), 6), 2, true);
        $summary = [
            '注冊人數 ' . $register . '，登入人數 ' . $login . '，活躍人數 ' . $active,
            '充值金額 ' . $deposit . ' USDT，提現金額 ' . $withdrawal . ' USDT，淨充值 ' . $netDepositWithdrawal . ' USDT',
            '總獎勵 ' . $award . ' USDT，總消費 ' . $consume . ' USDT，總收益 ' . $income . ' USDT'
        ];
        return array_merge($result, $summary);
    }

    //抓取代理數據
    private function doReport($request)
    {
        Log::info('報表參數：' . json_encode(array_merge($request['params'], $request['basic']), JSON_UNESCAPED_UNICODE));
        $requestBasic = new Request();
        $requestBasic->merge($request['basic']);
        //當日充值加提現前十資料
        $rankDW = $this->getDepositWithdrawalRank($requestBasic);
        //當日統計資料
        $summary = $this->getSummary($requestBasic);
        //組合通知訊息
        $preMessage = [
            '',
            '代理：' . ($request['params']['agentName'] ?? ''),
            '計算時間段：' . $request['basic']['start_time'] . ' ~ ' . $request['basic']['end_time'],
        ];
        $message = implode(PHP_EOL, array_merge($preMessage, $rankDW, $summary));
        //log
        $result = [
            'rankDW' => $rankDW,
            'summary' => $summary,
            'message' => $message
        ];
        Log::info('報表結果數據：' . json_encode($result, JSON_UNESCAPED_UNICODE));
        //send TG
        $telegramService = new TelegramService();
        $telegramService->sendMessage($message, 'miner', '', true);
    }
}
