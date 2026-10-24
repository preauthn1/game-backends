<?php

namespace App\Livewire\FinancialMgmt;

use App\Enums\PublicEnum;
use App\Enums\TimeEnum as TimeEnum;
use App\Livewire\Common;
use Illuminate\Support\Carbon;
use Livewire\Component;
use App\Http\Controllers\MemberMiner\RebateController;

class RebateInfo extends Component
{
    //搜尋
    public $username, $account, $minerName, $timeRange;
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
        $now = Carbon::now()->setTimezone(TimeEnum::TimeZone['backend']);
        $this->timeRange = $now->startOfDay()->format('Y-m-d H:i') . ' ~ ' . $now->endOfDay()->format('Y-m-d H:i');
        $searchParams = $this->getSearchParams();
        $this->getDataList($searchParams);
    }

    public function render()
    {
        return view('livewire.financialMgmt.rebate-info');
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

    //搜尋參數串接
    private function getSearchParams(): array
    {
        $searchParams = [];
        if (!empty($this->username ?? '')) {
            $searchParams['username'] = $this->username;
        }
        if (!empty($this->account ?? '')) {
            $searchParams['account'] = $this->account;
        }
        if (!empty($this->minerName ?? '')) {
            $searchParams['name'] = $this->minerName;
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
        $rebateController = new RebateController();
        $dataList = $rebateController->getList($requests['request'])->getData();
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
