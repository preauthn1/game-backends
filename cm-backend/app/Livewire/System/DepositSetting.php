<?php

namespace App\Livewire\System;

use App\Enums\PublicEnum;
use App\Http\Controllers\Deposit\MainController;
use App\Http\Controllers\Deposit\BankController;
use App\Livewire\Common;
use Livewire\Component;

class DepositSetting extends Component
{
    protected $defaultParams = [
        'page' => 1,
        'per_page' => 50
    ];
    public $rateCurrencyId = 0, $fxRate = 0;
    //搜尋收款銀行
    public $depBankType, $depBankCurrency, $depBankName, $depBankStatus;
    //新增
    public $addDepBankTypeId, $addDepBankCurrencyId, $addDepBankName, $addDepBankCode, $addDepBankMinAmount, $addDepBankMaxAmount, $addDepBankOrder;
    //POP
    public $popBankId, $popBankType, $popBankCurrencyId, $popBankName;
    //修改
    public $modifyBankCode, $modifyBankMinAmount, $modifyBankMaxAmount, $modifyBankSort, $modifyBankStatus;

    public function mount()
    {
        $this->getCurrencyList($this->defaultParams);
        $this->getDropdown();
        $searchParams = $this->getSearchParams();
        $this->getDataList($searchParams);
    }

    public function render()
    {
        return view('livewire.system.deposit-setting');
    }

    //切換選取幣種 匯率變動
    public function changeRate()
    {
        $currencyResult = session()->get('pageCacheData')['currencyList'] ?? [];
        $this->fxRate = $currencyResult[$this->rateCurrencyId]['deposit_rate'] ?? 0;
    }

    //變更匯率
    public function updateDepositRate()
    {
        if (!empty($this->rateCurrencyId ?? 0)) {
            $common = new Common();
            //串接修改request
            $modifyParams = [
                'currency_id' => $this->rateCurrencyId,
                'deposit_rate' => $this->fxRate,
            ];
            $updateRequests = $common->getRequest($modifyParams);
            if (PublicEnum::ResponseCode['needLogin'] == $updateRequests['code']) {
                $common->doLogout();
            }
            //修改接口
            $bankController = new BankController();
            $updateResult = $bankController->updateRate($updateRequests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $updateResult->code) {
                $this->getCurrencyList($this->defaultParams);
                session()->flash('pagePopMessage', __('web_message.general.setting') . ' ' . __('web_message.general.success'));
            } else {
                //顯示錯誤訊息POP
                session()->flash('pagePopMessage', collect($updateResult->message ?? [])->implode('; '));
            }
        }
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

    public function addDepBank()
    {
        //頁面傳參
        $addParams = [
            'type_id' => $this->addDepBankTypeId,
            'currency_id' => $this->addDepBankCurrencyId,
            'bank_name' => $this->addDepBankName,
            'min_amount' => $this->addDepBankMinAmount,
            'max_amount' => $this->addDepBankMaxAmount,
        ];
        if (!empty($this->addDepBankCode ?? '')) {
            $addParams['bank_code'] = $this->addDepBankCode;
        }
        if (!empty($this->addDepBankOrder ?? 0)) {
            $addParams['sort'] = $this->addDepBankOrder;
        }
        //串接request
        $common = new Common();
        $requests = $common->getRequest($addParams);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        //新增接口
        $bankController = new BankController();
        $addChannel = $bankController->create($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $addChannel->code) {
            redirect()->to('/depositSetting')->with('pagePopMessage', __('web_message.depositSetting.addDepBank') . ' ' . __('web_message.general.success'));
        } else {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', collect($addChannel->message ?? [])->implode('; '));
            //恢復原本POP
            session()->flash('popId', 'addDepBank');
        }
    }

    //修改/詳細 POP
    public function showDataPop($bankId = '', $action = '')
    {
        if (!empty($bankId) && !empty($action)) {
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $pageCacheData['popAction'] = $action;
            session()->put('pageCacheData', $pageCacheData);
            //串接request
            $common = new Common();
            $requests = $common->getRequest(['bank_id' => $bankId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            //資訊
            $bankController = new BankController();
            $bankData = $bankController->depositBankView($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $bankData->code) {
                $this->popBankId = $bankData->data->data->id ?? '';
                $this->popBankType = $bankData->data->data->bankTypeTxt ?? '';
                $this->popBankCurrencyId = $bankData->data->data->currency_id ?? '';
                $this->popBankName = $bankData->data->data->bank_name ?? '';
                $this->modifyBankCode = $bankData->data->data->bank_code ?? '';
                $this->modifyBankMinAmount = $bankData->data->data->min_amount ?? '';
                $this->modifyBankMaxAmount = $bankData->data->data->max_amount ?? '';
                $this->modifyBankSort = $bankData->data->data->sort ?? '';
                $this->modifyBankStatus = (1 == ($bankData->data->data->status ?? 1));
                //展示POP
                session()->flash('showPopId', 'depBankPop');
            }
        }
    }

    //修改
    public function doModify()
    {
        if (!empty($this->popBankId)) {
            //抓取資訊
            $common = new Common();
            $requests = $common->getRequest(['bank_id' => $this->popBankId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            $bankController = new BankController();
            $bankData = $bankController->depositBankView($requests['request'])->getData();
            //串接修改request
            $modifyParams = [];
            if ($this->modifyBankCode != ($bankData->data->data->bank_code ?? '')) {
                $modifyParams['bank_code'] = $this->modifyBankCode;
            }
            if ($this->modifyBankMinAmount != ($bankData->data->data->min_amount ?? '')) {
                $modifyParams['min_amount'] = $this->modifyBankMinAmount;
            }
            if ($this->modifyBankMaxAmount != ($bankData->data->data->max_amount ?? '')) {
                $modifyParams['max_amount'] = $this->modifyBankMaxAmount;
            }
            if ($this->modifyBankSort != ($bankData->data->data->sort ?? '')) {
                $modifyParams['sort'] = $this->modifyBankSort;
            }
            if ($this->modifyBankStatus != (1 == ($bankData->data->data->status ?? 1))) {
                $modifyParams['status'] = $this->modifyBankStatus ? 1 : 0;
            }
            //無異動
            if (empty($modifyParams)) {
                //顯示錯誤訊息POP
                session()->flash('pagePopMessage', __('web_message.general.noChange'));
                //恢復原本POP
                session()->flash('popId', 'depBankPop');
            } else {
                $modifyParams = array_merge(['bank_id' => $this->popBankId], $modifyParams);
                $updateRequests = $common->getRequest($modifyParams);
                if (PublicEnum::ResponseCode['needLogin'] == $updateRequests['code']) {
                    $common->doLogout();
                }
                //修改接口
                $updateMiner = $bankController->updateDepositBank($updateRequests['request'])->getData();
                if (PublicEnum::ResponseCode['success'] == $updateMiner->code) {
                    $searchParams = $this->getSearchParams();
                    $this->getDataList($searchParams);
                    session()->flash('pagePopMessage', __('web_message.depositSetting.modifyDepBank') . ' ' . __('web_message.general.success'));
                } else {
                    //顯示錯誤訊息POP
                    session()->flash('pagePopMessage', collect($updateMiner->message ?? [])->implode('; '));
                    //恢復原本POP
                    session()->flash('popId', 'depBankPop');
                }
            }
        }
    }

    //currency key塞id
    private function getCurrencyList($params)
    {
        $currencyResult = $this->getCurrency($params);
        if (PublicEnum::ResponseCode['success'] == $currencyResult->code) {
            $currencyList = []; //有配置銀行的幣種列表
            $depositCurrencyList = []; //可以充值的幣種列表
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
                $depositCurrencyList[] = [
                    'id' => $data->id,
                    'name' => $data->name,
                ];
            }
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $pageCacheData['currencyList'] = $currencyList;
            $pageCacheData['depositCurrencyList'] = $depositCurrencyList;
            session()->put('pageCacheData', $pageCacheData);
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
        //類型
        $typeResult = $mainController->getGatewayTypeDropdown($dropDownRequests['request'])->getData();
        //狀態
        $statusResult = $mainController->getGatewayStatusDropdown($dropDownRequests['request'])->getData();
        //塞進session
        $pageCacheData = session()->get('pageCacheData') ?? [];
        $pageCacheData['typeList'] = $typeResult->data->list ?? [];
        $pageCacheData['statusList'] = $statusResult->data->list ?? [];
        session()->put('pageCacheData', $pageCacheData);
    }

    //收款銀行參數串接
    private function getSearchParams(): array
    {
        $searchParams = [];
        $searchParams['type'] = $this->depBankType ?? 0;
        if (!empty($this->depBankCurrency ?? 0)) {
            $searchParams['currency_id'] = $this->depBankCurrency;
        }
        if (!empty($this->depBankName ?? '')) {
            $searchParams['bank_name'] = $this->depBankName;
        }
        if (-1 != ($this->depBankStatus ?? -1)) {
            $searchParams['status'] = $this->depBankStatus;
        }
        $searchParams['page'] = PublicEnum::DefaultPage;
        if (!empty(session('nowPage') ?? 0)) {
            $searchParams['page'] = session('nowPage');
        }
        $searchParams['per_page'] = PublicEnum::DefaultPageSize;
        return $searchParams;
    }

    //收款銀行列表
    private function getDataList($params): void
    {
        $common = new Common();
        $requests = $common->getRequest($params);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $bankController = new BankController();
        $dataList = $bankController->getDepositBankList($requests['request'])->getData();
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
