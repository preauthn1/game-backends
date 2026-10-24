<?php

namespace App\Livewire\System;

use App\Enums\PublicEnum;
use App\Http\Controllers\Announcement\MainController;
use App\Livewire\Common;
use Livewire\Component;

class Announcement extends Component
{
    //搜尋
    public $announcementType, $announcementRange, $announcementStatus;
    //監聽時間相關
    protected $listeners = [
        'announcementRange' => 'modifyAnnouncementRange',
        'announcementStartTime' => 'announcementStartTime',
        'announcementEndTime' => 'announcementEndTime'
    ];
    //新增
    public $addAnnouncementType, $addAnnouncementTitle, $addAnnouncementContent, $addAnnouncementTop = false, $addAnnouncementStatus = true, $announcementStartTime = '', $announcementEndTime = '';
    //修改
    public $modifyAnnouncementId, $modifyAnnouncementType, $modifyAnnouncementTitle, $modifyAnnouncementContent, $modifyAnnouncementTop, $modifyAnnouncementStatus;

    //監聽公告時間區間搜尋
    public function modifyAnnouncementRange($range)
    {
        //驗證日期
        $common = new Common();
        $dateCheck = $common->checkDateRange($range);
        if (PublicEnum::ResponseCode['success'] != $dateCheck['code']) {
            session()->flash('pagePopMessage', $dateCheck['message']);
            $this->announcementRange = '';
        } else {
            $this->announcementRange = $range;
        }
        //不更新畫面
        $this->skipRender();
    }

    //監聽公告開始時間
    public function announcementStartTime($date)
    {
        //驗證日期
        $common = new Common();
        $dateCheck = $common->checkDateTime($date);
        if (PublicEnum::ResponseCode['success'] != $dateCheck['code']) {
            session()->flash('pagePopMessage', $dateCheck['message']);
            $this->announcementStartTime = '';
        } else {
            $this->announcementStartTime = $date;
        }
        //不更新畫面
        $this->skipRender();
    }

    //監聽公告結束時間
    public function announcementEndTime($date)
    {
        //驗證日期
        $common = new Common();
        $dateCheck = $common->checkDateTime($date);
        if (PublicEnum::ResponseCode['success'] != $dateCheck['code']) {
            session()->flash('pagePopMessage', $dateCheck['message']);
            $this->announcementEndTime = '';
        } else {
            $this->announcementEndTime = $date;
        }
        //不更新畫面
        $this->skipRender();
    }

    public function mount()
    {
        $this->typeDropdown();
        $searchParams = $this->getSearchParams();
        $this->getAnnouncementData($searchParams);
    }

    public function render()
    {
        return view('livewire.system.announcement');
    }

    public function setTimeRange($option)
    {
        $common = new Common();
        $this->announcementRange = $common->fastTimeRange($option);
        $searchParams = $this->getSearchParams();
        $this->getAnnouncementData($searchParams);
    }

    //分頁切換
    public function changePage($oldPage, $newPage)
    {
        if ($oldPage != $newPage) {
            if (1 > $newPage) $newPage = 1;
            if ($newPage > session()->get('pageCacheData')['totalPage']) $newPage = session()->get('pageCacheData')['totalPage'];
            session()->put('nowPage', ($newPage ?? PublicEnum::DefaultPage));
            $searchParams = $this->getSearchParams();
            $this->getAnnouncementData($searchParams);
        }
    }

    //搜尋
    public function doSearch()
    {
        session()->put('nowPage', PublicEnum::DefaultPage);
        $searchParams = $this->getSearchParams();
        $this->getAnnouncementData($searchParams);
    }

    //新增
    public function addAnnouncement()
    {
        //頁面傳參
        $addParams = [
            'type' => $this->addAnnouncementType,
            'title' => $this->addAnnouncementTitle,
            'content' => $this->addAnnouncementContent,
            'is_top' => $this->addAnnouncementTop ? 1 : 0,
            'start_time' => $this->announcementStartTime,
            'end_time' => $this->announcementEndTime,
            'status' => $this->addAnnouncementStatus ? 1 : 0,
        ];
        //串接request
        $common = new Common();
        $requests = $common->getRequest($addParams);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        //新增接口
        $announcementController = new MainController();
        $addAnnouncement = $announcementController->create($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $addAnnouncement->code) {
            redirect()->to('/announcement')->with('pagePopMessage', __('web_message.menu.system.addAnnouncement') . ' ' . __('web_message.general.success'));
        } else {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', collect($addAnnouncement->message ?? [])->implode('; '));
            //恢復原本POP
            session()->flash('popId', 'addAnnouncement');
        }
    }

    //修改POP
    public function showModify($announcementId)
    {
        if (!empty($announcementId)) {
            //串接request
            $common = new Common();
            $requests = $common->getRequest(['id' => $announcementId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            //資訊
            $announcementController = new MainController();
            $announcementData = $announcementController->view($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $announcementData->code) {
                $this->modifyAnnouncementId = $announcementData->data->data->id;
                $this->modifyAnnouncementType = $announcementData->data->data->type ?? 0;
                $this->modifyAnnouncementTitle = $announcementData->data->data->title ?? '';
                $this->modifyAnnouncementContent = $announcementData->data->data->content ?? '';
                $this->modifyAnnouncementTop = (1 == ($announcementData->data->data->is_top ?? 0));
                $this->announcementStartTime = $announcementData->data->data->start_time ?? '';
                $this->announcementEndTime = $announcementData->data->data->end_time ?? '';
                $this->modifyAnnouncementStatus = (1 == ($announcementData->data->data->status ?? 1));
                //展示POP
                session()->flash('showPopId', 'modifyAnnouncement');
            }
        }
    }

    //修改
    public function doModify()
    {
        if (!empty($this->modifyAnnouncementId)) {
            //抓取資訊
            $common = new Common();
            $requests = $common->getRequest(['id' => $this->modifyAnnouncementId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            $announcementController = new MainController();
            $announcementData = $announcementController->view($requests['request'])->getData();
            //串接修改request
            $modifyParams = [];
            if ($this->modifyAnnouncementType != ($announcementData->data->data->type ?? 0)) {
                $modifyParams['type'] = $this->modifyAnnouncementType;
            }
            if ($this->modifyAnnouncementTitle != ($announcementData->data->data->title ?? '')) {
                $modifyParams['title'] = $this->modifyAnnouncementTitle;
            }
            if ($this->modifyAnnouncementContent != ($announcementData->data->data->content ?? '')) {
                $modifyParams['content'] = $this->modifyAnnouncementContent;
            }
            if ($this->modifyAnnouncementTop != (1 == ($announcementData->data->data->is_top ?? 0))) {
                $modifyParams['is_top'] = $this->modifyAnnouncementTop ? 1 : 0;
            }
            if ($this->announcementStartTime != ($announcementData->data->data->start_time ?? '')) {
                $modifyParams['start_time'] = $this->announcementStartTime;
            }
            if ($this->announcementEndTime != ($announcementData->data->data->end_time ?? '')) {
                $modifyParams['end_time'] = $this->announcementEndTime;
            }
            if ($this->modifyAnnouncementStatus != (1 == ($announcementData->data->data->status ?? 1))) {
                $modifyParams['status'] = $this->modifyAnnouncementStatus ? 1 : 0;
            }
            //無異動
            if (empty($modifyParams)) {
                //顯示錯誤訊息POP
                session()->flash('pagePopMessage', __('web_message.general.noChange'));
                //恢復原本POP
                session()->flash('popId', 'modifyAnnouncement');
            } else {
                $modifyParams = array_merge(['id' => $this->modifyAnnouncementId], $modifyParams);
                $updateRequests = $common->getRequest($modifyParams);
                if (PublicEnum::ResponseCode['needLogin'] == $updateRequests['code']) {
                    $common->doLogout();
                }
                //修改接口
                $updateAnnouncement = $announcementController->update($updateRequests['request'])->getData();
                if (PublicEnum::ResponseCode['success'] == $updateAnnouncement->code) {
                    $searchParams = $this->getSearchParams();
                    $this->getAnnouncementData($searchParams);
                    session()->flash('pagePopMessage', __('web_message.menu.system.modifyAnnouncement') . ' ' . __('web_message.general.success'));
                } else {
                    //顯示錯誤訊息POP
                    session()->flash('pagePopMessage', collect($updateAnnouncement->message ?? [])->implode('; '));
                    //恢復原本POP
                    session()->flash('popId', 'modifyAnnouncement');
                }
            }
        }
    }

    //搜尋參數串接
    private function getSearchParams(): array
    {
        $searchParams = [];
        $searchParams['type'] = $this->announcementType ?? 0;
        if (!empty($this->announcementRange ?? '')) {
            $common = new Common();
            $dateCheck = $common->checkDateRange($this->announcementRange);
            if (PublicEnum::ResponseCode['success'] == $dateCheck['code']) {
                $searchParams['start_time'] = $dateCheck['range']['start'];
                $searchParams['end_time'] = $dateCheck['range']['end'];
            }
        }
        if (-1 != ($this->announcementStatus ?? -1)) {
            $searchParams['status'] = $this->announcementStatus;
        }
        $searchParams['page'] = PublicEnum::DefaultPage;
        if (!empty(session('nowPage') ?? 0)) {
            $searchParams['page'] = session('nowPage');
        }
        $searchParams['per_page'] = PublicEnum::DefaultPageSize;
        return $searchParams;
    }

    private function typeDropdown(): void
    {
        //接口
        $common = new Common();
        $dropDownRequests = $common->getRequest([]);
        if (PublicEnum::ResponseCode['needLogin'] == $dropDownRequests['code']) {
            $common->doLogout();
        }
        $announcementController = new MainController();
        $dropDownResult = $announcementController->getDropdown($dropDownRequests['request'])->getData();
        $pageCacheData = session()->get('pageCacheData') ?? [];
        $pageCacheData['typeList'] = $dropDownResult->data->list ?? [];
        session()->put('pageCacheData', $pageCacheData);
    }

    private function getAnnouncementData($params)
    {
        $common = new Common();
        $requests = $common->getRequest($params);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $announcementController = new MainController();
        $announcementList = $announcementController->getList($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $announcementList->code) {
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $pageCacheData['pageData'] = $announcementList->data->list ?? [];
            $pageCacheData['totalPage'] = $announcementList->data->pagination->pages ?? PublicEnum::DefaultPage;
            $pageCacheData['totalRow'] = $announcementList->data->pagination->total ?? 0;
            session()->put('nowPage', $announcementList->data->pagination->current ?? PublicEnum::DefaultPage);
            session()->put('pageCacheData', $pageCacheData);
        } else {
            session()->flash('pagePopMessage', collect($announcementList->message ?? [])->implode('; '));
        }
    }
}
