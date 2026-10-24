<?php

namespace App\Livewire\Member;

use App\Enums\PublicEnum;
use App\Enums\TimeEnum as TimeEnum;
use App\Exports\GenericExport;
use App\Http\Controllers\Member\WalletController;
use App\Livewire\Common;
use Illuminate\Support\Carbon;
use Livewire\Component;
use App\Enums\TransactionTypeEnum;
use App\Enums\AgentCustomizedSettingsEnum;
use App\Http\Controllers\Record\TransactionLogController;
use App\Services\AgentService;
use Maatwebsite\Excel\Facades\Excel;

class ManualOperation extends Component
{
    //搜尋
    public $type, $username, $operator, $timeRange;
    //新增
    public $addType, $addUsername, $addAmount, $addMemo;
    //备注选项
    public $memoOptions = [];
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
        $this->typeDropdown();
        $this->getMemoOptions();
        $now = Carbon::now()->setTimezone(TimeEnum::TimeZone['backend']);
        $this->timeRange = $now->startOfDay()->format('Y-m-d H:i') . ' ~ ' . $now->endOfDay()->format('Y-m-d H:i');
        $searchParams = $this->getSearchParams();
        $this->getManualOperationList($searchParams);
    }

    public function render()
    {
        return view('livewire.member.manual-operation');
    }

    public function setTimeRange($option)
    {
        $common = new Common();
        $this->timeRange = $common->fastTimeRange($option);
        $searchParams = $this->getSearchParams();
        $this->getManualOperationList($searchParams);
    }

    //分頁切換
    public function changePage($oldPage, $newPage): void
    {
        if ($oldPage != $newPage) {
            if (1 > $newPage) $newPage = 1;
            if ($newPage > session()->get('pageCacheData')['totalPage']) $newPage = session()->get('pageCacheData')['totalPage'];
            session()->put('nowPage', ($newPage ?? PublicEnum::DefaultPage));
            $searchParams = $this->getSearchParams();
            $this->getManualOperationList($searchParams);
        }
    }

    //搜尋
    public function doSearch(): void
    {
        session()->put('nowPage', PublicEnum::DefaultPage);
        $searchParams = $this->getSearchParams();
        $this->getManualOperationList($searchParams);
    }

    //新增
    public function addManualOperation()
    {
        //頁面傳參
        $addParams = [
            'username' => $this->addUsername,
            'transaction_type' => $this->addType,
            'amount' => $this->addAmount,
        ];
        if (!empty($this->addMemo ?? '')) {
            $addParams['memo'] = $this->addMemo;
        }
        //串接request
        $common = new Common();
        $requests = $common->getRequest($addParams);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        //新增接口
        $walletController = new WalletController();
        $addDeposit = $walletController->adjust($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $addDeposit->code) {
            redirect()->to('/manualOperation')->with('pagePopMessage', __('web_message.menu.member.addManualOperation') . ' ' . __('web_message.general.success'));
        } else {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', collect($addDeposit->message ?? [])->implode('; '));
            //恢復原本POP
            session()->flash('popId', 'addManualOperation');
        }
    }

    public function doExport()
    {
        $searchParams = $this->getSearchParams();
        //#78 需求只需要類型跟時間條件 其他去掉
        $needParams = ['transaction_type', 'start_time', 'end_time'];
        foreach ($searchParams as $key => $value) {
            if (!in_array($key, $needParams)) {
                unset($searchParams[$key]);
            }
        }
        $searchParams['page'] = PublicEnum::DefaultExportPage;
        $searchParams['per_page'] = PublicEnum::DefaultExportPageSize;
        return Excel::download(new GenericExport('manualOperation', $searchParams), 'manualOperation' . time() . '.xlsx');
    }

    public function changeType()
    {
        $this->addMemo = '';
        session()->flash('showPopId', 'addManualOperation');
    }

    public function showAddManualOperation()
    {
        //初始畫面
        $this->addType = '-1';
        $this->addUsername = '';
        $this->addAmount = '';
        $this->addMemo = '';
        //展示POP
        session()->flash('showPopId', 'addManualOperation');
    }

    private function typeDropdown(): void
    {
        $pageCacheData = session()->get('pageCacheData') ?? [];
        foreach (TransactionTypeEnum::TransactionType as $type => $id) {
            $pageCacheData['typeList'][] = [
                'id' => $id,
                'type' => __('web_message.transactionType.' . $type),
            ];
        }
        session()->put('pageCacheData', $pageCacheData);
    }

    private function getMemoOptions(): void
    {
        $agentService = new AgentService();
        $common = new Common();
        $requests = $common->getRequest([
            'setting_keys' => [
                AgentCustomizedSettingsEnum::MANUAL_ADD_OPTIONS,
                AgentCustomizedSettingsEnum::MANUAL_SUB_OPTIONS,
                AgentCustomizedSettingsEnum::MANUAL_REWARD_OPTIONS,
                AgentCustomizedSettingsEnum::MANUAL_DEDUCT_OPTIONS,
            ]
        ]);
        $result = $agentService->getCustomizedSettings($requests['request']);
        $settings = $result->pluck('value', 'setting_key');
        $optionsMap = [
            AgentCustomizedSettingsEnum::MANUAL_ADD_OPTIONS => TransactionTypeEnum::TransactionType["manualAdd"],
            AgentCustomizedSettingsEnum::MANUAL_SUB_OPTIONS => TransactionTypeEnum::TransactionType["manualSub"],
            AgentCustomizedSettingsEnum::MANUAL_REWARD_OPTIONS => TransactionTypeEnum::TransactionType["manualReward"],
            AgentCustomizedSettingsEnum::MANUAL_DEDUCT_OPTIONS => TransactionTypeEnum::TransactionType["manualDeduct"],
        ];

        foreach ($optionsMap as $settingKey => $transactionKey) {
            $options = explode(';', $settings[$settingKey] ?? '');
            $options = array_filter($options, 'strlen');
            if (empty($options)) {
                continue;
            }
            $this->memoOptions[$transactionKey] = $options;
        }
    }

    //搜尋參數串接
    private function getSearchParams(): array
    {
        $searchParams = [];
        if (-1 != ($this->type ?? -1)) {
            $searchParams['transaction_type'] = $this->type;
        }
        if (!empty($this->username ?? '')) {
            $searchParams['username'] = $this->username;
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

    private function getManualOperationList($params): void
    {
        $common = new Common();
        $requests = $common->getRequest($params);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $transactionLogController = new TransactionLogController();
        $dataList = $transactionLogController->getManualList($requests['request'])->getData();
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
