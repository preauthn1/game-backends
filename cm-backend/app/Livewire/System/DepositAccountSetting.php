<?php

namespace App\Livewire\System;

use App\Enums\PublicEnum;
use App\Http\Controllers\Deposit\MainController;
use App\Http\Controllers\Deposit\BankController;
use App\Http\Controllers\Deposit\BankAccountController;
use App\Livewire\Common;
use Livewire\Component;

class DepositAccountSetting extends Component
{
    protected $defaultParams = [
        'page' => 1,
        'per_page' => 50
    ];
    //搜尋收款帳號
    public $channelType, $channelCurrency, $channelAlias, $channelStatus;
    //新增收款帳號
    public $addChannelCurrencyId, $addChannelBankId, $addChannelAlias, $addChannelAccountName, $addAccountNo;

    public function mount()
    {
        $this->getCurrencyList($this->defaultParams);
        $this->getDropdown();
        $searchParams = $this->getSearchParams();
        $this->getDataList($searchParams);
    }

    public function render()
    {
        return view('livewire.system.deposit-account-setting');
    }

    public function showAddChannel()
    {
        //初始畫面
        $this->addChannelCurrencyId = 0;
        $this->addChannelBankId = 0;
        $this->addChannelAlias = '';
        $this->addChannelAccountName = '';
        $this->addAccountNo = '';
        //展示POP
        session()->flash('showPopId', 'addChannel');
    }

    //切換選取幣種 銀行選單變動
    public function changeBank()
    {
        $currencyResult = session()->get('pageCacheData')['currencyList'] ?? [];
        $bankList = $currencyResult[$this->addChannelCurrencyId]['bank'] ?? [];
        $this->addChannelBankId = 0;
        $pageCacheData = session()->get('pageCacheData') ?? [];
        $pageCacheData['bankList'] = $bankList;
        session()->put('pageCacheData', $pageCacheData);
        //恢復原本POP
        session()->flash('showPopId', 'addChannel');
    }

    //分頁切換
    public function changePage($oldPage, $newPage)
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
    public function doSearch()
    {
        session()->put('nowPage', PublicEnum::DefaultPage);
        $searchParams = $this->getSearchParams();
        $this->getDataList($searchParams);
    }

    //新增
    public function addChannel()
    {
        //頁面傳參
        $addParams = [
            'bank_id' => $this->addChannelBankId,
            'account_name' => $this->addChannelAccountName,
            'account_no' => $this->addAccountNo,
        ];
        if (!empty($this->addChannelAlias ?? '')) {
            $addParams['account_alias'] = $this->addChannelAlias;
        }
        //串接request
        $common = new Common();
        $requests = $common->getRequest($addParams);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        //新增接口
        $bankAccountController = new BankAccountController();
        $addChannel = $bankAccountController->create($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $addChannel->code) {
            redirect()->to('/depositAccountSetting')->with('pagePopMessage', __('web_message.depositSetting.addDepAccount') . ' ' . __('web_message.general.success'));
        } else {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', collect($addChannel->message ?? [])->implode('; '));
            //恢復原本POP
            session()->flash('popId', 'addChannel');
        }
    }

    public function doModifyStatus($id, $status)
    {
        if (!empty($id ?? '')) {
            $common = new Common();
            //串接修改request
            $modifyParams = [
                'bank_account_id' => $id,
                'status' => $status,
            ];
            $updateRequests = $common->getRequest($modifyParams);
            if (PublicEnum::ResponseCode['needLogin'] == $updateRequests['code']) {
                $common->doLogout();
            }
            //修改接口
            $bankAccountController = new BankAccountController();
            $updateResult = $bankAccountController->updateStatus($updateRequests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $updateResult->code) {
                $searchParams = $this->getSearchParams();
                $this->getDataList($searchParams);
                session()->flash('pagePopMessage', __('web_message.general.setting') . ' ' . __('web_message.general.success'));
            } else {
                //顯示錯誤訊息POP
                session()->flash('pagePopMessage', collect($updateResult->message ?? [])->implode('; '));
            }
        }
    }

    //currency key塞id
    private function getCurrencyList($params)
    {
        $currencyResult = $this->getCurrency($params);
        if (PublicEnum::ResponseCode['success'] == $currencyResult->code) {
            $currencyList = [];
            foreach (($currencyResult->data->list ?? []) as $data) {
                $bankList = [];
                foreach (($data->bank ?? []) as $bank) {
                    $bankList[$bank->id] = [
                        'bank_id' => $bank->id,
                        'name' => $bank->name,
                    ];
                }
                if (!empty($bankList)) {
                    $currencyList[$data->id] = [
                        'id' => $data->id,
                        'code' => $data->code,
                        'name' => $data->name,
                        'deposit_rate' => $data->deposit_rate,
                        'bank' => $bankList
                    ];
                }
            }
            if (!empty($currencyList)) {
                $pageCacheData = session()->get('pageCacheData') ?? [];
                $pageCacheData['currencyList'] = $currencyList;
                session()->put('pageCacheData', $pageCacheData);
            }
        } else {
            session()->flash('pagePopMessage', $currencyResult->message);
        }
    }

    //呼叫api取資料
    private function getCurrency($params)
    {
        $common = new Common();
        $requests = $common->getRequest($params);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $bankController = new BankController();
        return $bankController->getList($requests['request'])->getData();
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
        $channelResult = $mainController->getGatewayTypeDropdown($dropDownRequests['request'])->getData();
        //狀態
        $statusResult = $mainController->getGatewayStatusDropdown($dropDownRequests['request'])->getData();
        //塞進session
        $pageCacheData = session()->get('pageCacheData') ?? [];
        $pageCacheData['channelList'] = $channelResult->data->list ?? [];
        $pageCacheData['statusList'] = $statusResult->data->list ?? [];
        session()->put('pageCacheData', $pageCacheData);
    }

    //收款帳號參數串接
    private function getSearchParams(): array
    {
        $searchParams = [];
        $searchParams['type'] = $this->channelType ?? 0;
        if (!empty($this->channelCurrency ?? 0)) {
            $searchParams['currency_id'] = $this->channelCurrency;
        }
        if (!empty($this->channelAlias ?? '')) {
            $searchParams['account_alias'] = $this->channelAlias;
        }
        if (-1 != ($this->channelStatus ?? -1)) {
            $searchParams['status'] = $this->channelStatus;
        }
        $searchParams['page'] = PublicEnum::DefaultPage;
        if (!empty(session('nowPage') ?? 0)) {
            $searchParams['page'] = session('nowPage');
        }
        $searchParams['per_page'] = PublicEnum::DefaultPageSize;
        return $searchParams;
    }

    //收款帳號列表
    private function getDataList($params): void
    {
        $common = new Common();
        $requests = $common->getRequest($params);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $bankAccountController = new BankAccountController();
        $dataList = $bankAccountController->getList($requests['request'])->getData();
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
