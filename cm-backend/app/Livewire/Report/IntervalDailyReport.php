<?php

namespace App\Livewire\Report;

use App\Enums\PublicEnum;
use App\Enums\TimeEnum as TimeEnum;
use App\Exports\GenericExport;
use App\Livewire\Common;
use Illuminate\Support\Carbon;
use Livewire\Component;
use App\Http\Controllers\Report\ReportController;
use Maatwebsite\Excel\Facades\Excel;

class IntervalDailyReport extends Component
{
    public $statisticTitle, $dailyTitle;
    //搜尋
    public $managerName, $reportType = 1, $reportTimeRange;

    //監聽時間相關
    protected $listeners = [
        'reportTimeRange' => 'reportTimeRange',
    ];

    //監聽時間區間搜尋
    public function reportTimeRange($range)
    {
        //驗證日期
        $common = new Common();
        $dateCheck = $common->checkDateRange($range);
        if (PublicEnum::ResponseCode['success'] != $dateCheck['code']) {
            session()->flash('pagePopMessage', $dateCheck['message']);
            $this->reportTimeRange = '';
        } else {
            $this->reportTimeRange = $range;
        }
        //不更新畫面
        $this->skipRender();
    }

    public function mount()
    {
        $now = Carbon::now()->setTimezone(TimeEnum::TimeZone['backend']);
        $this->reportTimeRange = $now->startOfDay()->format('Y-m-d') . ' ~ ' . $now->endOfDay()->format('Y-m-d');
        $this->changeReportType();
    }

    public function render()
    {
        return view('livewire.report.interval-daily-report');
    }

    public function setTimeRange($option)
    {
        $common = new Common();
        $this->reportTimeRange = $common->fastDateRange($option);
    }

    public function changeReportType()
    {
        $this->statisticTitle = [
            'type', 'newRegister', 'todayLogin', 'activeMember', 'totalDeposit', 'totalWithdrawal', 'netDeposit', 'totalConsume', 'totalIncome', 'totalAward'
        ];
        $this->dailyTitle = [
            'date', 'newRegister', 'todayLogin', 'activeMember', 'totalDeposit', 'totalWithdrawal', 'netDeposit', 'totalConsume', 'totalIncome', 'totalAward'
        ];
        if (2 == ($this->reportType ?? 1)) {
            $this->statisticTitle = [
                'type', 'newRegister', 'todayLogin', 'activeMember', 'totalDeposit', 'totalWithdrawal', 'netDeposit', 'totalConsume', 'totalPurchase', 'totalRent', 'totalIncome', 'minerIncome', 'rentIncome', 'inviteIncome', 'rebateIncome', 'totalAward'
            ];
            $this->dailyTitle = [
                'date', 'newRegister', 'todayLogin', 'activeMember', 'totalDeposit', 'totalWithdrawal', 'netDeposit', 'totalConsume', 'totalPurchase', 'totalRent', 'totalIncome', 'minerIncome', 'rentIncome', 'inviteIncome', 'rebateIncome', 'totalAward'
            ];
        }
    }

    //搜尋
    public function doSearch(): void
    {
        $this->changeReportType();
        $searchParams = $this->getSearchParams();
        $this->getDataList($searchParams);
    }

    public function doExport()
    {
        $this->changeReportType();
        $searchParams = $this->getSearchParams();
        return Excel::download(new GenericExport('intervalDailyReport', $searchParams), 'intervalDailyReport' . time() . '.xlsx');
    }

    //搜尋參數串接
    private function getSearchParams(): array
    {
        $searchParams = [];
        if (!empty($this->managerName ?? '')) {
            $searchParams['manager_name'] = $this->managerName;
        }
        if (!empty($this->reportTimeRange ?? '')) {
            $common = new Common();
            $dateCheck = $common->checkDateRange($this->reportTimeRange);
            if (PublicEnum::ResponseCode['success'] == $dateCheck['code']) {
                $searchParams['start_date'] = $dateCheck['range']['start'];
                $searchParams['end_date'] = $dateCheck['range']['end'];
            }
        }
        $searchParams['type'] = $this->reportType ?? 1;
        return $searchParams;
    }

    private function getDataList($params): void
    {
        $common = new Common();
        $requests = $common->getRequest($params);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $reportController = new ReportController();
        $reportData = $reportController->getReport($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $reportData->code) {
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $pageCacheData['pageData']['data'] = $reportData->data->data ?? [];
            $pageCacheData['pageData']['statistic'] = $reportData->data->statistic ?? [];
            session()->put('pageCacheData', $pageCacheData);
        } else {
            session()->flash('pagePopMessage', collect($reportData->message ?? [])->implode('; '));
        }
    }
}
