<?php

namespace App\Livewire\FinancialMgmt;

use App\Enums\PublicEnum;
use App\Enums\TimeEnum as TimeEnum;
use App\Livewire\Common;
use Illuminate\Support\Carbon;
use Livewire\Component;
use App\Http\Controllers\Miner\PurchaseListController;

class PurchaseList extends Component
{
    //搜尋
    public $username, $minerName, $agentName, $minerStatus, $timeRange, $checkTimeRange;
    //監聽時間相關
    protected $listeners = [
        'createTimeRange' => 'createTimeRange',
        'checkTimeRange' => 'checkTimeRange',
    ];

    //監聽時間區間搜尋
    public function createTimeRange($range)
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

    public function checkTimeRange($range)
    {
        //驗證日期
        $common = new Common();
        $dateCheck = $common->checkDateRange($range);
        if (PublicEnum::ResponseCode['success'] != $dateCheck['code']) {
            session()->flash('pagePopMessage', $dateCheck['message']);
            $this->checkTimeRange = '';
        } else {
            $this->checkTimeRange = $range;
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
        $this->getPurchaseList($searchParams);
    }

    public function render()
    {
        return view('livewire.financialMgmt.purchase-list');
    }

    public function setCreateTimeRange($option)
    {
        $common = new Common();
        $this->timeRange = $common->fastTimeRange($option);
        $searchParams = $this->getSearchParams();
        $this->getPurchaseList($searchParams);
    }

    public function setCheckTimeRange($option)
    {
        $common = new Common();
        $this->checkTimeRange = $common->fastTimeRange($option);
        $searchParams = $this->getSearchParams();
        $this->getPurchaseList($searchParams);
    }

    //搜尋
    public function doSearch(): void
    {
        session()->put('nowPage', PublicEnum::DefaultPage);
        $searchParams = $this->getSearchParams();
        $this->getPurchaseList($searchParams);
    }

    //分頁切換
    public function changePage($oldPage, $newPage): void
    {
        if ($oldPage != $newPage) {
            if (1 > $newPage) $newPage = 1;
            if ($newPage > session()->get('pageCacheData')['totalPage']) $newPage = session()->get('pageCacheData')['totalPage'];
            session()->put('nowPage', ($newPage ?? PublicEnum::DefaultPage));
            $searchParams = $this->getSearchParams();
            $this->getPurchaseList($searchParams);
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
        //狀態
        $purchaseListController = new PurchaseListController();
        $statusResult = $purchaseListController->getStatusDropdown($dropDownRequests['request'])->getData();
        //塞進session
        $pageCacheData = session()->get('pageCacheData') ?? [];
        $pageCacheData['statusList'] = $statusResult->data->list ?? [];
        session()->put('pageCacheData', $pageCacheData);
    }

    //搜尋參數串接
    private function getSearchParams(): array
    {
        $searchParams = [];
        if (!empty($this->username ?? '')) {
            $searchParams['username'] = $this->username;
        }
        if (!empty($this->minerName ?? '')) {
            $searchParams['name'] = $this->minerName;
        }
        if (!empty($this->agentName ?? '')) {
            $searchParams['parent_username'] = $this->agentName;
        }
        if (-1 != ($this->minerStatus ?? -1)) {
            $searchParams['status'] = $this->minerStatus;
        }
        if (!empty($this->timeRange ?? '')) {
            $common = new Common();
            $dateCheck = $common->checkDateRange($this->timeRange);
            if (PublicEnum::ResponseCode['success'] == $dateCheck['code']) {
                $searchParams['create_time_start'] = $dateCheck['range']['start'];
                $searchParams['create_time_end'] = $dateCheck['range']['end'];
            }
        }
        if (!empty($this->checkTimeRange ?? '')) {
            $common = new Common();
            $dateCheck = $common->checkDateRange($this->checkTimeRange);
            if (PublicEnum::ResponseCode['success'] == $dateCheck['code']) {
                $searchParams['check_time_start'] = $dateCheck['range']['start'];
                $searchParams['check_time_end'] = $dateCheck['range']['end'];
            }
        }
        $searchParams['page'] = PublicEnum::DefaultPage;
        if (!empty(session('nowPage') ?? 0)) {
            $searchParams['page'] = session('nowPage');
        }
        $searchParams['per_page'] = PublicEnum::DefaultPageSize;
        return $searchParams;
    }

    private function getPurchaseList($params): void
    {
        $common = new Common();
        $requests = $common->getRequest($params);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $purchaseListController = new PurchaseListController();
        $dataList = $purchaseListController->getList($requests['request'])->getData();
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
