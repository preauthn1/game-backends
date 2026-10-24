<?php

namespace App\Livewire\FinancialMgmt;

use App\Enums\MinerEnum;
use App\Enums\PublicEnum;
use App\Enums\TimeEnum as TimeEnum;
use App\Livewire\Common;
use Illuminate\Support\Carbon;
use Livewire\Component;
use App\Http\Controllers\Miner\RentListController;
use App\Http\Controllers\Miner\MinerController;

class RentList extends Component
{
    //搜尋
    public $username, $minerName, $agentName, $minerStatus, $isExperience, $operator, $createTimeRange, $startTimeRange, $endTimeRange, $checkTimeRange;
    //新增/POP
    public $addExperienceMinerId, $popPurchaseRentPrice, $popPurchaseRentCount, $popPurchaseRentCycle, $popExperienceCommission, $addExperienceUsername;
    //監聽時間相關
    protected $listeners = [
        'createTimeRange' => 'createTimeRange',
        'startTimeRange' => 'startTimeRange',
        'endTimeRange' => 'endTimeRange',
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
            $this->createTimeRange = '';
        } else {
            $this->createTimeRange = $range;
        }
        //不更新畫面
        $this->skipRender();
    }

    public function startTimeRange($range)
    {
        //驗證日期
        $common = new Common();
        $dateCheck = $common->checkDateRange($range);
        if (PublicEnum::ResponseCode['success'] != $dateCheck['code']) {
            session()->flash('pagePopMessage', $dateCheck['message']);
            $this->startTimeRange = '';
        } else {
            $this->startTimeRange = $range;
        }
        //不更新畫面
        $this->skipRender();
    }

    public function endTimeRange($range)
    {
        //驗證日期
        $common = new Common();
        $dateCheck = $common->checkDateRange($range);
        if (PublicEnum::ResponseCode['success'] != $dateCheck['code']) {
            session()->flash('pagePopMessage', $dateCheck['message']);
            $this->endTimeRange = '';
        } else {
            $this->endTimeRange = $range;
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
        $this->createTimeRange = $now->startOfDay()->format('Y-m-d H:i') . ' ~ ' . $now->endOfDay()->format('Y-m-d H:i');
        $searchParams = $this->getSearchParams();
        $this->getRentList($searchParams);
    }

    public function render()
    {
        return view('livewire.financialMgmt.rent-list');
    }

    public function setCreateTimeRange($option)
    {
        $common = new Common();
        $this->createTimeRange = $common->fastTimeRange($option);
        $searchParams = $this->getSearchParams();
        $this->getRentList($searchParams);
    }

    public function setStartTimeRange($option)
    {
        $common = new Common();
        $this->startTimeRange = $common->fastTimeRange($option);
        $searchParams = $this->getSearchParams();
        $this->getRentList($searchParams);
    }

    public function setEndTimeRange($option)
    {
        $common = new Common();
        $this->endTimeRange = $common->fastTimeRange($option);
        $searchParams = $this->getSearchParams();
        $this->getRentList($searchParams);
    }

    public function setCheckTimeRange($option)
    {
        $common = new Common();
        $this->checkTimeRange = $common->fastTimeRange($option);
        $searchParams = $this->getSearchParams();
        $this->getRentList($searchParams);
    }

    //搜尋
    public function doSearch(): void
    {
        session()->put('nowPage', PublicEnum::DefaultPage);
        $searchParams = $this->getSearchParams();
        $this->getRentList($searchParams);
    }

    //分頁切換
    public function changePage($oldPage, $newPage): void
    {
        if ($oldPage != $newPage) {
            if (1 > $newPage) $newPage = 1;
            if ($newPage > session()->get('pageCacheData')['totalPage']) $newPage = session()->get('pageCacheData')['totalPage'];
            session()->put('nowPage', ($newPage ?? PublicEnum::DefaultPage));
            $searchParams = $this->getSearchParams();
            $this->getRentList($searchParams);
        }
    }

    public function showAddExperience()
    {
        //初始數據
        $this->addExperienceMinerId = 0;
        $this->popPurchaseRentPrice = '';
        $this->popPurchaseRentCount = '';
        $this->popPurchaseRentCycle = '';
        $this->popExperienceCommission = '';
        $this->addExperienceUsername = '';
        //展示POP
        session()->flash('showPopId', 'addExperience');
    }

    public function changeMiner()
    {
        //初始數據
        $this->popPurchaseRentPrice = '';
        $this->popPurchaseRentCount = '';
        $this->popPurchaseRentCycle = '';
        $this->popExperienceCommission = '0.00';
        $minerList = session()->get('pageCacheData')['minerList'] ?? [];
        foreach ($minerList as $data) {
            if ($this->addExperienceMinerId == $data->miner_id) {
                $this->popPurchaseRentPrice = number_format($data->rent_price, PublicEnum::DefaultScale);
                $this->popPurchaseRentCount = $data->promos->p_num ?? 0;
                $this->popPurchaseRentCycle = intval($data->promos->p_days ?? 0) / MinerEnum::CycleDays;
                $this->popExperienceCommission = $data->promos->r_ec ?? '0.00';
            }
        }
        //展示POP
        session()->flash('showPopId', 'addExperience');
    }

    public function addExperience()
    {
        //頁面傳參
        $addParams = [
            'miner_id' => $this->addExperienceMinerId,
            'username' => $this->addExperienceUsername,
        ];
        //串接request
        $common = new Common();
        $requests = $common->getRequest($addParams);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        //新增接口
        $minerController = new MinerController();
        $addExperience = $minerController->createExperience($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $addExperience->code) {
            redirect()->to('/rentList')->with('pagePopMessage', __('web_message.menu.financialMgmt.addExperience') . ' ' . __('web_message.general.success'));
        } else {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', collect($addExperience->message ?? [])->implode('; '));
            //恢復原本POP
            session()->flash('popId', 'addExperience');
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
        $rentListController = new RentListController();
        $statusResult = $rentListController->getStatusDropdown($dropDownRequests['request'])->getData();
        //礦機
        $minerController = new MinerController();
        $minerListResult = $minerController->getDropdown($dropDownRequests['request'])->getData();
        //塞進session
        $pageCacheData = session()->get('pageCacheData') ?? [];
        $pageCacheData['statusList'] = $statusResult->data->list ?? [];
        $pageCacheData['minerList'] = $minerListResult->data->list ?? [];
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
        if (-1 != ($this->isExperience ?? -1)) {
            $searchParams['isExperience'] = $this->isExperience;
        }
        if (!empty($this->operator ?? '')) {
            $searchParams['operator'] = $this->operator;
        }
        if (!empty($this->createTimeRange ?? '')) {
            $common = new Common();
            $dateCheck = $common->checkDateRange($this->createTimeRange);
            if (PublicEnum::ResponseCode['success'] == $dateCheck['code']) {
                $searchParams['c_start_time'] = $dateCheck['range']['start'];
                $searchParams['c_end_time'] = $dateCheck['range']['end'];
            }
        }
        if (!empty($this->startTimeRange ?? '')) {
            $common = new Common();
            $dateCheck = $common->checkDateRange($this->startTimeRange);
            if (PublicEnum::ResponseCode['success'] == $dateCheck['code']) {
                $searchParams['s_start_time'] = $dateCheck['range']['start'];
                $searchParams['s_end_time'] = $dateCheck['range']['end'];
            }
        }
        if (!empty($this->endTimeRange ?? '')) {
            $common = new Common();
            $dateCheck = $common->checkDateRange($this->endTimeRange);
            if (PublicEnum::ResponseCode['success'] == $dateCheck['code']) {
                $searchParams['e_start_time'] = $dateCheck['range']['start'];
                $searchParams['e_end_time'] = $dateCheck['range']['end'];
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

    private function getRentList($params): void
    {
        $common = new Common();
        $requests = $common->getRequest($params);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $rentListController = new RentListController();
        $dataList = $rentListController->getList($requests['request'])->getData();
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
