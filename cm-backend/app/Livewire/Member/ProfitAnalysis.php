<?php

namespace App\Livewire\Member;

use App\Enums\PublicEnum;
use App\Enums\TimeEnum as TimeEnum;
use App\Http\Controllers\Member\ProfitAnalysisController;
use App\Livewire\Common;
use Illuminate\Support\Carbon;
use Livewire\Component;

class ProfitAnalysis extends Component
{
    //搜尋
    public $uid, $username, $orderBy, $sort, $timeRange;
    //監聽時間相關
    protected $listeners = [
        'timeRange' => 'timeRange',
    ];

    //監聽時間區間搜尋
    public function timeRange($range)
    {
        //驗證日期
        $common = new Common();
        $dateCheck = $common->checkDateRange($range);
        if (PublicEnum::ResponseCode['success'] != $dateCheck['code']) {
            session()->flash('pagePopMessage', $dateCheck['message']);
            $this->timeRange = '';
        } else {
            $this->timeRange = $range;
        }
        //不更新畫面
        $this->skipRender();
    }

    public function mount()
    {
        $this->getDropdown();
        $now = Carbon::now()->setTimezone(TimeEnum::TimeZone['backend']);
        $this->timeRange = $now->startOfDay()->format('Y-m-d H:i') . ' ~ ' . $now->endOfDay()->format('Y-m-d H:i');
        $searchParams = $this->getSearchParams();
        $this->getDataList($searchParams);
    }

    public function render()
    {
        return view('livewire.member.profit-analysis');
    }

    public function setTimeRange($option)
    {
        $common = new Common();
        $this->timeRange = $common->fastTimeRange($option);
        $searchParams = $this->getSearchParams();
        $this->getDataList($searchParams);
    }

    //搜尋
    public function doSearch(): void
    {
        session()->put('nowPage', PublicEnum::DefaultPage);
        $searchParams = $this->getSearchParams();
        $this->getDataList($searchParams);
    }

    //分頁切換
    public function changePage($oldPage, $newPage): void
    {
        if ($oldPage != $newPage) {
            if (1 > $newPage) $newPage = 1;
            if ($newPage > session()->get('pageCacheData')['totalPage']) $newPage = session()->get('pageCacheData')['totalPage'];
            session()->put('nowPage', ($newPage ?? PublicEnum::DefaultPage));
            $searchParams = $this->getSearchParams();
            $this->getDataList($searchParams);
        }
    }

    private function getDropdown(): void
    {
        //接口
        $common = new Common();
        $dropDownRequests = $common->getRequest([]);
        if (PublicEnum::ResponseCode['needLogin'] == $dropDownRequests['code']) {
            $common->doLogout();
        }
        $profitAnalysisController = new ProfitAnalysisController();
        //渠道
        $orderByResult = $profitAnalysisController->getOrderBy($dropDownRequests['request'])->getData();
        //狀態
        $sortResult = $profitAnalysisController->getSort($dropDownRequests['request'])->getData();
        //塞進session
        $pageCacheData = session()->get('pageCacheData') ?? [];
        $pageCacheData['orderByList'] = $orderByResult->data->list ?? [];
        $pageCacheData['sortList'] = $sortResult->data->list ?? [];
        session()->put('pageCacheData', $pageCacheData);
    }

    //搜尋參數串接
    private function getSearchParams(): array
    {
        $searchParams = [];
        if (!empty($this->uid ?? '')) {
            $searchParams['uid'] = $this->uid;
        }
        if (!empty($this->username ?? '')) {
            $searchParams['username'] = $this->username;
        }
        if (-1 != ($this->orderBy ?? -1)) {
            $searchParams['order_by'] = $this->orderBy;
        }
        if (-1 != ($this->sort ?? -1)) {
            $searchParams['sort'] = $this->sort;
        }
        if (!empty($this->timeRange ?? '')) {
            $common = new Common();
            $dateCheck = $common->checkDateRange($this->timeRange);
            if (PublicEnum::ResponseCode['success'] == $dateCheck['code']) {
                $searchParams['start_time'] = $dateCheck['range']['start'];
                $searchParams['end_time'] = $dateCheck['range']['end'];
            }
        }
        $searchParams['page'] = PublicEnum::DefaultPage;
        if (!empty(session('nowPage') ?? 0)) {
            $searchParams['page'] = session('nowPage');
        }
        $searchParams['per_page'] = PublicEnum::DefaultPageSize;
        return $searchParams;
    }

    private function getDataList($params): void
    {
        $common = new Common();
        $requests = $common->getRequest($params);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $profitAnalysisController = new ProfitAnalysisController();
        $dataList = $profitAnalysisController->getList($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $dataList->code) {
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $pageCacheData['pageData'] = $dataList->data->list ?? [];
            $pageCacheData['totalPage'] = $dataList->data->pagination->pages ?? PublicEnum::DefaultPage;
            $pageCacheData['totalRow'] = $dataList->data->pagination->total ?? 0;
            session()->put('nowPage', $dataList->data->pagination->current ?? PublicEnum::DefaultPage);
            session()->put('pageCacheData', $pageCacheData);
        } else {
            session()->flash('pagePopMessage', collect($dataList->message ?? [])->implode('; '));
        }
    }
}
