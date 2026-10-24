<?php

namespace App\Livewire\Withdrawal;

use App\Enums\PublicEnum;
use App\Enums\TimeEnum as TimeEnum;
use App\Helpers\NumberHelper;
use App\Http\Controllers\Withdrawal\MainController;
use App\Http\Controllers\Withdrawal\OrderController;
use App\Http\Controllers\Member\MemberController;
use App\Livewire\Common;
use Illuminate\Support\Carbon;
use Livewire\Component;
use function Symfony\Component\String\s;

class WithdrawalList extends Component
{
    //搜尋
    public $username, $channel, $orderNo, $status = 0, $orderTimeRange, $amountMin, $amountMax, $payTimeRange;
    //新增
    public $addUsername, $addBalance, $addChannel, $addBankId, $addBankName, $addAccountId, $addAccountNo, $addAmount, $addMinAmount, $addMaxAmount, $addTaxType, $addTaxRate, $addTax, $addRate, $addAmountTip, $addRealAmount;
    //POP
    public $popUsername, $popUserTypeTxt, $popChannel, $popOrderNo, $popPoint, $popRate, $popFee, $popCardHolder, $popCardNo, $popAmount, $popStatus, $popOperator, $popUpdateAt, $popPayTime;
    //修改
    public $modifyDataId, $modifyDataRemark;

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
        $this->getDataList($searchParams);
    }

    public function render()
    {
        return view('livewire.withdrawal.withdrawal-list');
    }

    public function setOrderTimeRange($option)
    {
        $common = new Common();
        $this->orderTimeRange = $common->fastTimeRange($option);
        $searchParams = $this->getSearchParams();
        $this->getDataList($searchParams);
    }
    public function setPayTimeRange($option)
    {
        $common = new Common();
        $this->payTimeRange = $common->fastTimeRange($option);
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

    //新增提現POP
    public function showAddWithdrawal()
    {
        //初始值
        $this->addUsername = '';
        $this->addBalance = 0;
        $this->addChannel = '';
        $this->addBankId = '';
        $this->addBankName = '';
        $this->addAccountId = '';
        $this->addAccountNo = '';
        $this->addTaxType = '';
        $this->addTaxRate = '';
        $this->addTax = '';
        $this->addRate = '';
        $this->addMinAmount = '';
        $this->addMaxAmount = '';
        $this->addAmountTip = '';
        $this->addAmount = '';
        $this->addRealAmount = '';
        //展示POP
        session()->flash('showPopId', 'addWithdrawal');
    }

    //獲取會員提現資料
    public function getUserWithdrawalData()
    {
        if (empty($this->addUsername)) {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', __('error_message.3002'));
            //恢復原本POP
            session()->flash('popId', 'addWithdrawal');
            return;
        }
        //串接request
        $common = new Common();
        $requests = $common->getRequest(['username' => $this->addUsername]);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        //資訊
        $memberController = new MemberController();
        $data = $memberController->getWithdrawalData($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $data->code) {
            //初始值
            $this->addChannel = '';
            $this->addBankId = '';
            $this->addBankName = '';
            $this->addAccountId = '';
            $this->addAccountNo = '';
            $this->addTaxType = '';
            $this->addTaxRate = '';
            $this->addTax = '';
            $this->addRate = '';
            $this->addMinAmount = '';
            $this->addMaxAmount = '';
            $this->addAmountTip = '';
            $this->addAmount = '';
            $this->addRealAmount = '';
            $this->addBalance = NumberHelper::formatDecimal(1, $data->data->balance ?? 0, PublicEnum::DefaultScale, true);
            //塞進session
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $pageCacheData['withdrawalData'] = $data->data ?? [];
            session()->put('pageCacheData', $pageCacheData);
            //展示POP
            session()->flash('showPopId', 'addWithdrawal');
        } else {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', collect($data->message ?? [])->implode('; '));
            //恢復原本POP
            session()->flash('popId', 'addWithdrawal');
        }
    }

    //切換取款渠道
    public function switchWithdrawlBank()
    {
        $channel = $this->addChannel;
        //初始值
        $this->addBankId = '';
        $this->addBankName = '';
        $this->addAccountId = '';
        $this->addAccountNo = '';
        $this->addMinAmount = '';
        $this->addMaxAmount = '';
        $this->addTaxType = '';
        $this->addTaxRate = '';
        $this->addTax = '';
        $this->addRate = '';
        $this->addAmountTip = '';
        $this->addRealAmount = '';
        if (!empty(session()->get('pageCacheData')['withdrawalData']->$channel ?? [])) {
            $this->addBankId = session()->get('pageCacheData')['withdrawalData']->$channel->bank_id ?? '';
            $this->addBankName = session()->get('pageCacheData')['withdrawalData']->$channel->bank_name ?? '';
            $this->addAccountId = session()->get('pageCacheData')['withdrawalData']->$channel->account_id ?? '';
            $this->addAccountNo = session()->get('pageCacheData')['withdrawalData']->$channel->account_no ?? '';
            $this->addMinAmount = floatval(session()->get('pageCacheData')['withdrawalData']->$channel->bank_min_amount ?? 0);
            $this->addMaxAmount = floatval(session()->get('pageCacheData')['withdrawalData']->$channel->bank_max_amount ?? 0);
            $this->addTaxType = isset(session()->get('pageCacheData')['withdrawalData']->$channel->bank_tax_type) ? session()->get('pageCacheData')['withdrawalData']->$channel->bank_tax_type : '';
            $this->addTaxRate = !empty(session()->get('pageCacheData')['withdrawalData']->$channel->bank_tax) ? floatval(session()->get('pageCacheData')['withdrawalData']->$channel->bank_tax) : '';
            $this->addRate = !empty(session()->get('pageCacheData')['withdrawalData']->$channel->withdrawal_rate) ? floatval(session()->get('pageCacheData')['withdrawalData']->$channel->withdrawal_rate) : '';
            $this->addAmountTip = $this->addMinAmount . ' - ' . $this->addMaxAmount;
        }
        //不更新畫面
        $this->skipRender();
    }

    //計算手續費及到帳金額
    public function calAmount()
    {
        if (empty($this->addAmount ?? 0)) {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', __('error_message.3043'));
            //恢復原本POP
            session()->flash('popId', 'addWithdrawal');
            return;
        }
        if (!is_numeric($this->addAmount ?? 0)) {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', __('error_message.5057'));
            //恢復原本POP
            session()->flash('popId', 'addWithdrawal');
            return;
        }
        $addTax = (1 == $this->addTaxType) ? $this->addTaxRate : bcmul($this->addAmount, $this->addTaxRate, (PublicEnum::DefaultScale + 2));
        $this->addTax = number_format($addTax, PublicEnum::DefaultScale);
        $netAmount = bcsub($this->addAmount, $addTax, (PublicEnum::DefaultScale + 2));
        $this->addRealAmount = number_format(bcmul($netAmount, $this->addRate, (PublicEnum::DefaultScale + 2)), PublicEnum::DefaultScale);
        //不更新畫面
        $this->skipRender();
    }

    //新增提現
    public function addWithdrawal()
    {
        $toAmount = bcmul($this->addAmount, $this->addRate, (PublicEnum::DefaultScale + 2));
        if (($toAmount < $this->addMinAmount) || ($toAmount > $this->addMaxAmount)) {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', __('error_message.5057'));
            //恢復原本POP
            session()->flash('popId', 'addWithdrawal');
            return;
        }
        //頁面傳參
        $addParams = [
            'username' => $this->addUsername,
            'amount' => $this->addAmount,
            'bank_id' => $this->addBankId,
            'account_id' => $this->addAccountId,
            'rate' => $this->addRate,
            'bank_tax_type' => $this->addTaxType,
            'bank_tax' => $this->addTaxRate,
        ];
        //串接request
        $common = new Common();
        $requests = $common->getRequest($addParams);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        //新增接口
        $orderController = new OrderController();
        $addResult = $orderController->create($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $addResult->code) {
            redirect()->to('/withdrawalList')->with('pagePopMessage', __('web_message.menu.withdrawal.addWithdrawal') . ' ' . __('web_message.general.success'));
        } else {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', collect($addResult->message ?? [])->implode('; '));
            //恢復原本POP
            session()->flash('popId', 'addWithdrawal');
        }
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
            $dataList = $orderController->view($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $dataList->code) {
                $this->popUsername = $dataList->data->data->username ?? '';
                $this->popUserTypeTxt = $dataList->data->data->memberTypeTxt ?? '';
                $this->popChannel = $dataList->data->data->gatewayTxt ?? '';
                $this->popOrderNo = $dataList->data->data->order_no ?? '';
                $this->popPoint = number_format($dataList->data->data->point, PublicEnum::DefaultScale);
                $this->popRate = rtrim(rtrim($dataList->data->data->exchange_rate ?? 0, '0'), '.');
                $this->popFee = number_format($dataList->data->data->fee, PublicEnum::DefaultScale);
                $this->popCardHolder = $dataList->data->data->cardholder ?? '';
                $this->popCardNo = $dataList->data->data->cardno ?? '';
                $this->popAmount = number_format($dataList->data->data->amount, PublicEnum::DefaultScale);
                $this->modifyDataId = $dataList->data->data->id ?? '';
                $this->popStatus = $dataList->data->data->status ?? '';
                $this->popOperator = $dataList->data->data->operator ?? '';
                $this->popUpdateAt = '';
                if (!empty($this->popOperator)) {
                    $this->popUpdateAt = $dataList->data->data->updated_at ?? '';
                }
                $this->popPayTime = $dataList->data->data->pay_time ?? '';
                $this->modifyDataRemark = $dataList->data->data->remark ?? '';
                //展示POP
                session()->flash('showPopId', 'dataPop');
            }
        }
    }

    //修改
    public function doModify($action)
    {
        if (empty($this->modifyDataId) || empty($action ?? '')) {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', __('error_message.9001'));
            //恢復原本POP
            session()->flash('popId', 'dataPop');
            return;
        }
        //抓取資訊
        $common = new Common();
        $requests = $common->getRequest(['order_id' => $this->modifyDataId]);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $orderController = new OrderController();
        $dataList = $orderController->view($requests['request'])->getData();
        if (!in_array($dataList->data->data->status, [0, 3])) {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', __('error_message.5035'));
            //恢復原本POP
            session()->flash('popId', 'dataPop');
            return;
        }
        //串接修改request
        $modifyParams = [
            'order_id' => $this->modifyDataId
        ];
        //修改
        switch ($action) {
            case 'approved':
                $modifyParams['status'] = 3;
                break;
            case 'payed':
                $modifyParams['status'] = 1;
                break;
            case 'canceledThenBack':
                $modifyParams['status'] = 2;
                break;
            case 'canceledNotBack':
                $modifyParams['status'] = 4;
                break;
        }
        if (!empty($this->modifyDataRemark ?? '')) {
            if ($this->modifyDataRemark != ($dataList->data->data->remark ?? '')) {
                $modifyParams['remark'] = $this->modifyDataRemark;
            }
        }
        $updateRequests = $common->getRequest($modifyParams);
        if (PublicEnum::ResponseCode['needLogin'] == $updateRequests['code']) {
            $common->doLogout();
        }
        //修改接口
        $updateMember = $orderController->update($updateRequests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $updateMember->code) {
            redirect()->to('/withdrawalList')->with('pagePopMessage', __('web_message.menu.withdrawal.modifyWithdrawal') . ' ' . __('web_message.general.success'));
        } else {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', collect($updateMember->message ?? [])->implode('; '));
            //恢復原本POP
            session()->flash('popId', 'dataPop');
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
        if ('' != $this->amountMin) {
            $searchParams['amount_min'] = floatval($this->amountMin);
        }
        if ('' != $this->amountMax) {
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

    private function getDataList($params): void
    {
        $common = new Common();
        $requests = $common->getRequest($params);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $orderController = new OrderController();
        $dataList = $orderController->getList($requests['request'])->getData();
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
