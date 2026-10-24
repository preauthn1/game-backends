<?php

namespace App\Livewire\System;

use App\Enums\PublicEnum;
use App\Enums\TimeEnum as TimeEnum;
use App\Http\Controllers\Member\MemberController;
use App\Http\Controllers\Miner\GroupController;
use App\Livewire\Common;
use Illuminate\Support\Carbon;
use Livewire\Component;

class GroupOrderSetting extends Component
{
    //搜尋
    public $minerName, $username, $memberType, $orderTimeRange;
    //新增
    public $addUsername, $addMinerName;
    //修改
    public $modifyId, $modifyMinerName, $modifyUserTypeTxt, $modifyUsername, $modifyCreatedAt, $modifyTestUsername;

    //監聽時間相關
    protected $listeners = [
        'orderTimeRange' => 'orderTimeRange',
    ];

    //監聽時間區間搜尋
    public function orderTimeRange($range)
    {
        //驗證日期
        $common = new Common();
        $dateCheck = $common->checkDateRange($range);
        if (PublicEnum::ResponseCode['success'] != $dateCheck['code']) {
            session()->flash('pagePopMessage', $dateCheck['message']);
            $this->orderTimeRange = '';
        } else {
            $this->orderTimeRange = $range;
        }
        //不更新畫面
        $this->skipRender();
    }

    public function mount()
    {
        $this->getDropdown();
        $now = Carbon::now()->setTimezone(TimeEnum::TimeZone['backend']);
        $this->orderTimeRange = $now->startOfDay()->format('Y-m-d H:i') . ' ~ ' . $now->endOfDay()->format('Y-m-d H:i');
        $searchParams = $this->getSearchParams();
        $this->getGroupList($searchParams);
    }

    public function render()
    {
        return view('livewire.system.group-order-setting');
    }

    public function setTimeRange($option)
    {
        $common = new Common();
        $this->orderTimeRange = $common->fastTimeRange($option);
        $searchParams = $this->getSearchParams();
        $this->getGroupList($searchParams);
    }

    //分頁切換
    public function changePage($oldPage, $newPage)
    {
        if ($oldPage != $newPage) {
            if (1 > $newPage) $newPage = 1;
            if ($newPage > session()->get('pageCacheData')['totalPage']) $newPage = session()->get('pageCacheData')['totalPage'];
            session()->put('nowPage', ($newPage ?? PublicEnum::DefaultPage));
            $searchParams = $this->getSearchParams();
            $this->getGroupList($searchParams);
        }
    }

    //搜尋
    public function doSearch()
    {
        session()->put('nowPage', PublicEnum::DefaultPage);
        $searchParams = $this->getSearchParams();
        $this->getGroupList($searchParams);
    }

    //新增
    public function addWaitingGroup()
    {
        //頁面傳參
        $addParams = [
            'username' => $this->addUsername,
            'minerName' => $this->addMinerName,
        ];
        //串接request
        $common = new Common();
        $requests = $common->getRequest($addParams);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        //新增接口
        $groupController = new GroupController();
        $addRankList = $groupController->createWaiting($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $addRankList->code) {
            redirect()->to('/groupOrderSetting')->with('pagePopMessage', __('web_message.menu.system.addGroupOrder') . ' ' . __('web_message.general.success'));
        } else {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', collect($addRankList->message ?? [])->implode('; '));
            //恢復原本POP
            session()->flash('popId', 'addWaitingGroup');
        }
    }

    //修改/詳細 POP
    public function showModify($mm_id)
    {
        if (!empty($mm_id)) {
            //串接request
            $common = new Common();
            $requests = $common->getRequest(['mm_id' => $mm_id]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            //資訊
            $groupController = new GroupController();
            $groupData = $groupController->waitingview($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $groupData->code) {
                $this->modifyId = $groupData->data->data->id;
                $this->modifyMinerName = $groupData->data->data->name ?? '';
                $this->modifyUserTypeTxt = $groupData->data->data->memberTypeTxt ?? '';
                $this->modifyUsername = $groupData->data->data->username ?? '';
                $this->modifyCreatedAt = $groupData->data->data->created_at ?? '';
                //展示POP
                session()->flash('showPopId', 'modifyWaitingGroup');
            }
        }
    }

    //修改
    public function doModify()
    {
        if (!empty($this->modifyId)) {
            //串接修改request
            $modifyParams = [];
            if (!empty($this->modifyTestUsername)) {
                $modifyParams['testUsername'] = $this->modifyTestUsername;
            }
            $origin = [
                'mm_id' => $this->modifyId,
            ];
            $modifyParams = array_merge($origin, $modifyParams);
            $common = new Common();
            $updateRequests = $common->getRequest($modifyParams);
            if (PublicEnum::ResponseCode['needLogin'] == $updateRequests['code']) {
                $common->doLogout();
            }
            //修改接口
            $groupController = new GroupController();
            $updateGroupOrder = $groupController->updateWaiting($updateRequests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $updateGroupOrder->code) {
                $searchParams = $this->getSearchParams();
                $this->getGroupList($searchParams);
                session()->flash('pagePopMessage', __('web_message.menu.system.modifyGroupOrder') . ' ' . __('web_message.general.success'));
            } else {
                //顯示錯誤訊息POP
                session()->flash('pagePopMessage', collect($updateGroupOrder->message ?? [])->implode('; '));
                //恢復原本POP
                session()->flash('popId', 'modifyWaitingGroup');
            }
        }
    }

    //取消
    public function doCancel($mm_id)
    {
        if (!empty($mm_id)) {
            //串接request
            $common = new Common();
            $requests = $common->getRequest(['mm_id' => $mm_id]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            //取消接口
            $groupController = new GroupController();
            $cancelGroup = $groupController->cancelWaiting($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $cancelGroup->code) {
                $searchParams = $this->getSearchParams();
                $this->getGroupList($searchParams);
                session()->flash('pagePopMessage', __('web_message.menu.system.cancelGroupOrder') . ' ' . __('web_message.general.success'));
            } else {
                //顯示錯誤訊息POP
                session()->flash('pagePopMessage', collect($cancelGroup->message ?? [])->implode('; '));
            }
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
        //會員類型
        $memberController = new MemberController();
        $memberTypeResult = $memberController->getDropdown($dropDownRequests['request'])->getData();
        //塞進session
        $pageCacheData = session()->get('pageCacheData') ?? [];
        $pageCacheData['memberTypeList'] = $memberTypeResult->data->list ?? [];
        session()->put('pageCacheData', $pageCacheData);
    }

    //搜尋參數串接
    private function getSearchParams(): array
    {
        $searchParams = [];
        if (!empty($this->minerName ?? '')) {
            $searchParams['name'] = $this->minerName;
        }
        if (!empty($this->username ?? '')) {
            $searchParams['username'] = $this->username;
        }
        if (-1 != ($this->memberType ?? -1)) {
            $searchParams['member_type'] = $this->memberType;
        }
        if (!empty($this->orderTimeRange ?? '')) {
            $common = new Common();
            $dateCheck = $common->checkDateRange($this->orderTimeRange);
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

    private function getGroupList($params): void
    {
        $common = new Common();
        $requests = $common->getRequest($params);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $groupController = new GroupController();
        $groupList = $groupController->getWaitingList($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $groupList->code) {
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $pageCacheData['pageData'] = $groupList->data->list ?? [];
            $pageCacheData['totalPage'] = $groupList->data->pagination->pages ?? PublicEnum::DefaultPage;
            $pageCacheData['totalRow'] = $groupList->data->pagination->total ?? 0;
            session()->put('nowPage', $groupList->data->pagination->current ?? PublicEnum::DefaultPage);
            session()->put('pageCacheData', $pageCacheData);
        } else {
            session()->flash('pagePopMessage', collect($groupList->message ?? [])->implode('; '));
        }
    }
}
