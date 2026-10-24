<?php

namespace App\Livewire\Deposit;

use App\Enums\PublicEnum;
use App\Enums\TimeEnum as TimeEnum;
use App\Livewire\Common;
use Illuminate\Support\Carbon;
use Livewire\Component;
use App\Http\Controllers\Deposit\OrderController;
use App\Http\Controllers\Member\WalletController;
use App\Http\Controllers\Deposit\MainController;

class DepositList extends Component
{
    //搜尋
    public $username, $channel, $orderNo, $status = 3, $orderTimeRange, $amountMin, $amountMax, $payTimeRange;
    //POP
    public $popUsername, $popChannel, $popOrderNo, $popAmount, $popRate, $popPoint, $popImage, $popStatus, $popOperator, $popUpdateAt, $popPayTime;
    //修改
    public $modifyDepositId, $modifyDepositRemark;

    //監聽時間相關
    protected $listeners = [
        'orderTimeRange' => 'orderTimeRange',
        'payTimeRange' => 'payTimeRange',
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

    public function payTimeRange($range)
    {
        //驗證日期
        $common = new Common();
        $dateCheck = $common->checkDateRange($range);
        if (PublicEnum::ResponseCode['success'] != $dateCheck['code']) {
            session()->flash('pagePopMessage', $dateCheck['message']);
            $this->payTimeRange = '';
        } else {
            $this->payTimeRange = $range;
        }
        //不更新畫面
        $this->skipRender();
    }

    public function mount()
    {
        $this->getDropdown();
        if (session()->has('emitUsername')) {
            $this->username = session()->get('emitUsername');
        }
        $now = Carbon::now()->setTimezone(TimeEnum::TimeZone['backend']);
        $this->orderTimeRange = $now->startOfDay()->format('Y-m-d H:i') . ' ~ ' . $now->endOfDay()->format('Y-m-d H:i');
        $searchParams = $this->getSearchParams();
        $this->getDepositList($searchParams);
    }

    public function render()
    {
        return view('livewire.deposit.deposit-list');
    }

    public function setOrderTimeRange($option)
    {
        $common = new Common();
        $this->orderTimeRange = $common->fastTimeRange($option);
        $searchParams = $this->getSearchParams();
        $this->getDepositList($searchParams);
    }

    public function setPayTimeRange($option)
    {
        $common = new Common();
        $this->payTimeRange = $common->fastTimeRange($option);
        $searchParams = $this->getSearchParams();
        $this->getDepositList($searchParams);
    }

    //分頁切換
    public function changePage($oldPage, $newPage): void
    {
        if ($oldPage != $newPage) {
            if (1 > $newPage) $newPage = 1;
            if ($newPage > session()->get('pageCacheData')['totalPage']) $newPage = session()->get('pageCacheData')['totalPage'];
            session()->put('nowPage', ($newPage ?? PublicEnum::DefaultPage));
            $searchParams = $this->getSearchParams();
            $this->getDepositList($searchParams);
        }
    }

    //搜尋
    public function doSearch(): void
    {
        session()->put('nowPage', PublicEnum::DefaultPage);
        $searchParams = $this->getSearchParams();
        $this->getDepositList($searchParams);
    }

    //修改/詳細 POP
    public function showDataPop($orderId = '', $action = '')
    {
        if (!empty($orderId) && !empty($action)) {
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $pageCacheData['popAction'] = $action;
            session()->put('pageCacheData', $pageCacheData);
            //串接request
            $common = new Common();
            $requests = $common->getRequest(['order_id' => $orderId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            //資訊
            $orderController = new OrderController();
            $depositList = $orderController->view($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $depositList->code) {
                $this->popUsername = $depositList->data->data->username ?? '';
                $this->popChannel = $depositList->data->data->gatewayTxt ?? '';
                $this->popOrderNo = $depositList->data->data->order_no ?? '';
                $this->popAmount = number_format($depositList->data->data->amount, PublicEnum::DefaultScale);
                $this->popRate = rtrim(rtrim($depositList->data->data->exchange_rate ?? 0, '0'), '.');
                $this->popPoint = number_format($depositList->data->data->point, PublicEnum::DefaultScale);
                $this->popImage = $depositList->data->data->image ?? '';
                $this->modifyDepositId = $depositList->data->data->id ?? '';
                $this->popStatus = $depositList->data->data->status ?? '';
                $this->popOperator = $depositList->data->data->operator ?? '';
                $this->popUpdateAt = '';
                if (!empty($this->popOperator)) {
                    $this->popUpdateAt = $depositList->data->data->updated_at ?? '';
                }
                $this->popPayTime = $depositList->data->data->pay_time ?? '';
                $this->modifyDepositRemark = $depositList->data->data->remark ?? '';
                //展示POP
                session()->flash('showPopId', 'depositPop');
            }
        }
    }

    //修改
    public function doModify($action)
    {
        if (empty($this->modifyDepositId) || empty($action ?? '')) {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', __('error_message.9001'));
            //恢復原本POP
            session()->flash('popId', 'modifyDeposit');
            return;
        }
        //抓取資訊
        $common = new Common();
        $requests = $common->getRequest(['order_id' => $this->modifyDepositId]);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $orderController = new OrderController();
        $depositList = $orderController->view($requests['request'])->getData();
        if (3 != $depositList->data->data->status) {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', __('error_message.5035'));
            //恢復原本POP
            session()->flash('popId', 'modifyDeposit');
            return;
        }
        //串接修改request
        $modifyParams = [
            'order_id' => $this->modifyDepositId
        ];
        //修改
        $modifyParams['status'] = 'approved' == $action ? 1 : 2;
        if (!empty($this->modifyDepositRemark ?? '')) {
            if ($this->modifyDepositRemark != ($depositList->data->data->remark ?? '')) {
                $modifyParams['remark'] = $this->modifyDepositRemark;
            }
        }
        $updateRequests = $common->getRequest($modifyParams);
        if (PublicEnum::ResponseCode['needLogin'] == $updateRequests['code']) {
            $common->doLogout();
        }
        //修改接口
        $updateMember = $orderController->update($updateRequests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $updateMember->code) {
            redirect()->to('/depositList')->with('pagePopMessage', __('web_message.menu.deposit.modifyDeposit') . ' ' . __('web_message.general.success'));
        } else {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', collect($updateMember->message ?? [])->implode('; '));
            //恢復原本POP
            session()->flash('popId', 'modifyDeposit');
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
        $mainController = new MainController();
        //渠道
        $channelResult = $mainController->getGatewayDropdown($dropDownRequests['request'])->getData();
        //狀態
        $statusResult = $mainController->getStatusDropdown($dropDownRequests['request'])->getData();
        //塞進session
        $pageCacheData = session()->get('pageCacheData') ?? [];
        $pageCacheData['channelList'] = $channelResult->data->list ?? [];
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
        if (-1 != ($this->channel ?? -1)) {
            $searchParams['gateway_id'] = $this->channel;
        }
        if (!empty($this->orderNo ?? '')) {
            $searchParams['order_no'] = $this->orderNo;
        }
        if (!empty($this->orderTimeRange ?? '')) {
            $common = new Common();
            $dateCheck = $common->checkDateRange($this->orderTimeRange);
            if (PublicEnum::ResponseCode['success'] == $dateCheck['code']) {
                $searchParams['start_time'] = $dateCheck['range']['start'];
                $searchParams['end_time'] = $dateCheck['range']['end'];
            }
        }
        if (-1 != ($this->status ?? -1)) {
            $searchParams['status'] = $this->status;
        }
        if (isset($this->amountMin)) {
            $searchParams['amount_min'] = floatval($this->amountMin);
        }
        if (isset($this->amountMax)) {
            $searchParams['amount_max'] = floatval($this->amountMax);
        }
        if (!empty($this->payTimeRange ?? '')) {
            $common = new Common();
            $dateCheck = $common->checkDateRange($this->payTimeRange);
            if (PublicEnum::ResponseCode['success'] == $dateCheck['code']) {
                $searchParams['pay_time_start'] = $dateCheck['range']['start'];
                $searchParams['pay_time_end'] = $dateCheck['range']['end'];
            }
        }
        $searchParams['page'] = PublicEnum::DefaultPage;
        if (!empty(session('nowPage') ?? 0)) {
            $searchParams['page'] = session('nowPage');
        }
        $searchParams['per_page'] = PublicEnum::DefaultPageSize;
        return $searchParams;
    }

    private function getDepositList($params): void
    {
        $common = new Common();
        $requests = $common->getRequest($params);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $orderController = new OrderController();
        $depositList = $orderController->getList($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $depositList->code) {
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $pageCacheData['pageData'] = $depositList->data->list ?? [];
            $pageCacheData['totalPage'] = $depositList->data->pagination->pages ?? PublicEnum::DefaultPage;
            $pageCacheData['totalRow'] = $depositList->data->pagination->total ?? 0;
            session()->put('nowPage', $depositList->data->pagination->current ?? PublicEnum::DefaultPage);
            session()->put('pageCacheData', $pageCacheData);
        } else {
            session()->flash('pagePopMessage', collect($depositList->message ?? [])->implode('; '));
        }
    }
}
