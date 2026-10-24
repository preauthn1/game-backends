<?php

namespace App\Livewire\Member;

use App\Enums\PublicEnum;
use App\Enums\TimeEnum as TimeEnum;
use App\Http\Controllers\Member\IdCardController;
use App\Livewire\Common;
use Illuminate\Support\Carbon;
use Livewire\Component;

class IdInformation extends Component
{
    //搜尋
    public $uid, $username, $status, $operator, $timeRange;
    //POP
    public $popUid, $popUsername, $popFrontImage, $popBackImage, $popCreatedAt, $popOperator, $popUpdateAt;
    //修改
    public $modifyId, $modifyStatus, $modifyRemark;

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
        return view('livewire.member.id-information');
    }

    public function setTimeRange($option)
    {
        $common = new Common();
        $this->timeRange = $common->fastTimeRange($option);
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

    //搜尋
    public function doSearch(): void
    {
        session()->put('nowPage', PublicEnum::DefaultPage);
        $searchParams = $this->getSearchParams();
        $this->getDataList($searchParams);
    }

    //修改/詳細 POP
    public function showDataPop($idCardId = '', $action = '')
    {
        if (!empty($idCardId) && !empty($action)) {
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $pageCacheData['popAction'] = $action;
            session()->put('pageCacheData', $pageCacheData);
            //串接request
            $common = new Common();
            $requests = $common->getRequest(['member_id_card_id' => $idCardId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            //資訊
            $idCardController = new IdCardController();
            $idCardData = $idCardController->view($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $idCardData->code) {
                $this->popUid = $idCardData->data->data->uid ?? '';
                $this->popUsername = $idCardData->data->data->username ?? '';
                $this->popFrontImage = $idCardData->data->data->image_front ?? '';
                $this->popBackImage = $idCardData->data->data->image_back ?? '';
                $this->popCreatedAt = $idCardData->data->data->created_at ?? '';
                $this->popOperator = $idCardData->data->data->operator ?? '';
                $this->popUpdateAt = '';
                if (!empty($this->popOperator)) {
                    $this->popUpdateAt = $idCardData->data->data->updated_at ?? '';
                }
                $this->modifyId = $idCardData->data->data->member_id_card_id ?? '';
                $this->modifyStatus = $idCardData->data->data->status ?? '';
                $this->modifyRemark = $idCardData->data->data->remark ?? '';
                //展示POP
                session()->flash('showPopId', 'idCardPop');
            }
        }
    }

    //修改
    public function doModify($action)
    {
        if (empty($this->modifyId) || empty($action ?? '')) {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', __('error_message.9001'));
            //恢復原本POP
            session()->flash('popId', 'idCardPop');
            return;
        }
        //抓取資訊
        $common = new Common();
        $requests = $common->getRequest(['member_id_card_id' => $this->modifyId]);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $idCardController = new IdCardController();
        $idCardData = $idCardController->view($requests['request'])->getData();
        if (0 != $idCardData->data->data->status) {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', __('error_message.5035'));
            //恢復原本POP
            session()->flash('popId', 'idCardPop');
            return;
        }
        //串接修改request
        $modifyParams = [
            'member_id_card_id' => $this->modifyId
        ];
        //修改
        $modifyParams['status'] = 'approved' == $action ? 1 : 2;
        if (!empty($this->modifyRemark ?? '')) {
            if ($this->modifyRemark != ($idCardData->data->data->remark ?? '')) {
                $modifyParams['remark'] = $this->modifyRemark;
            }
        }
        $updateRequests = $common->getRequest($modifyParams);
        if (PublicEnum::ResponseCode['needLogin'] == $updateRequests['code']) {
            $common->doLogout();
        }
        //修改接口
        $updateIdCard = $idCardController->update($updateRequests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $updateIdCard->code) {
            redirect()->to('/idInformation')->with('pagePopMessage', __('web_message.menu.member.modifyIdInformation') . ' ' . __('web_message.general.success'));
        } else {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', collect($updateIdCard->message ?? [])->implode('; '));
            //恢復原本POP
            session()->flash('popId', 'idCardPop');
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
        $idCardController = new IdCardController();
        //狀態
        $statusResult = $idCardController->getDropdown($dropDownRequests['request'])->getData();
        //塞進session
        $pageCacheData = session()->get('pageCacheData') ?? [];
        $pageCacheData['statusList'] = $statusResult->data->list ?? [];
        session()->put('pageCacheData', $pageCacheData);
    }

    //搜尋參數串接
    private function getSearchParams(): array
    {
        $searchParams = [];
        if (!empty($this->uid ?? 0)) {
            $searchParams['uid'] = $this->uid;
        }
        if (!empty($this->username ?? '')) {
            $searchParams['username'] = $this->username;
        }
        if (-1 != ($this->status ?? -1)) {
            $searchParams['status'] = $this->status;
        }
        if (!empty($this->operator ?? '')) {
            $searchParams['operator'] = $this->operator;
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
        $idCardController = new IdCardController();
        $dataList = $idCardController->getList($requests['request'])->getData();
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
