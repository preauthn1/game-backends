<?php

namespace App\Livewire\System;

use Livewire\Component;
use App\Enums\PublicEnum;
use App\Livewire\Common;
use App\Http\Controllers\Withdrawal\BankController;
use App\Http\Controllers\Withdrawal\MainController;

class WithdrawalSetting extends Component
{
    protected array $defaultParams = [
        'page' => 1,
        'per_page' => 50
    ];
    //设置USDT to 法币汇率
    public $rateCurrencyId = 0, $fxRate = 0;
    //设置币种提现手续费比率
    public $feeCurrencyId = 0, $currencyFeeType = 0, $currencyFeeNumber = '';
    //搜尋
    public $channelType, $channelCurrency, $channelAlias;
    //新增
    public $addTypeId, $addCurrencyId, $addBankName, $addAlias, $addImage, $addMinAmount, $addMaxAmount, $addTaxType = 0, $addTax = 0, $addOrder = 0;
    //修改
    public $popBankTypeTxt, $popCurrencyName, $popBankName, $popTaxTxt;
    public $modifyBankId, $modifyBankAlias, $modifyMinAmount, $modifyMaxAmount, $modifyTaxType, $modifyTax, $modifySort, $modifyStatus, $modifyImageUrl;

    //監聽
    protected $listeners = [
        'addImageUrl' => 'addImageUrl',
        'modifyImageUrl' => 'modifyImageUrl',
    ];

    //監聽新增圖片
    public function addImageUrl($data)
    {
        $this->addImage = $data;
        //不更新畫面
        $this->skipRender();
    }

    //監聽彈窗圖片
    public function modifyImageUrl($data)
    {
        $this->modifyImageUrl = $data;
        //不更新畫面
        $this->skipRender();
    }

    public function mount()
    {
        $this->getDropdown();
        $this->getCurrencyList($this->defaultParams);
        $searchParams = $this->getSearchParams();
        $this->getDataList($searchParams);
    }

    public function render()
    {
        return view('livewire.system.withdrawal-setting');
    }

    //切換選取幣種 匯率變動
    public function changeRate()
    {
        $currencyResult = session()->get('pageCacheData')['currencyList'] ?? [];
        $this->fxRate = $currencyResult[$this->rateCurrencyId]['withdrawal_rate'] ?? 0;
    }

    //切換手續費類型 placeholder跟後綴變動
    public function changeFeeType() {}

    //變更匯率
    public function updateWithdrawalRate()
    {
        if (!empty($this->rateCurrencyId ?? 0)) {
            $common = new Common();
            //串接修改request
            $modifyParams = [
                'currency_id' => $this->rateCurrencyId,
                'withdrawal_rate' => $this->fxRate,
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

    //變更币种提现手续费比率
    public function updateCurrencyWithdrawalFeeRate()
    {
        if (!empty($this->feeCurrencyId ?? 0)) {
            $currencyFee = $this->currencyFeeNumber;
            if (0 == ($this->currencyFeeType ?? 0)) {
                //百分比轉小數點
                $currencyFee = bcdiv($this->currencyFeeNumber, 100, 5);
            }
            $common = new Common();
            //串接修改request
            $modifyParams = [
                'currency_id' => $this->feeCurrencyId,
                'tax_type' => $this->currencyFeeType ?? 0,
                'tax' => (string)$currencyFee,
            ];
            $updateRequests = $common->getRequest($modifyParams);
            if (PublicEnum::ResponseCode['needLogin'] == $updateRequests['code']) {
                $common->doLogout();
            }
            //修改接口
            $bankController = new BankController();
            $updateResult = $bankController->updateCurrencyTax($updateRequests['request'])->getData();
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

    public function addWdBank()
    {
        //頁面傳參
        $addParams = [
            'type_id' => (int)$this->addTypeId,
            'currency_id' => $this->addCurrencyId,
            'bank_name' => $this->addBankName,
            'bank_code' => $this->addAlias,
            'min_amount' => $this->addMinAmount,
            'max_amount' => $this->addMaxAmount,
        ];
        if (!empty($this->addImage ?? '')) {
            $addParams['image'] = $this->addImage;
        }
        if (!empty($this->addTaxType ?? 0)) {
            $addParams['tax_type'] = $this->addTaxType;
        }
        if (!empty($this->addTax ?? 0)) {
            $addParams['tax'] = $this->addTax;
        }
        if (!empty($this->addOrder ?? 0)) {
            $addParams['sort'] = $this->addOrder;
        }
        //串接request
        $common = new Common();
        $requests = $common->getRequest($addParams);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        //新增接口
        $mainController = new MainController();
        $addWdBank = $mainController->create($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $addWdBank->code) {
            redirect()->to('/withdrawalSetting')->with('pagePopMessage', __('web_message.withdrawalSetting.addWdBank') . ' ' . __('web_message.general.success'));
        } else {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', collect($addWdBank->message ?? [])->implode('; '));
            //恢復原本POP
            session()->flash('popId', 'addWdBank');
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
            $mainController = new MainController();
            $bankData = $mainController->view($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $bankData->code) {
                $this->modifyBankId = $bankData->data->data->id;
                $this->popBankTypeTxt = $bankData->data->data->bankTypeTxt;
                $this->popCurrencyName = $bankData->data->data->currency_name ?? '';
                $this->popBankName = $bankData->data->data->bank_name ?? '';
                $this->modifyBankAlias = $bankData->data->data->account_alias ?? '';
                $this->modifyMinAmount = $bankData->data->data->min_amount ?? '';
                $this->modifyMaxAmount = $bankData->data->data->max_amount ?? '';
                $this->modifyTaxType = $bankData->data->data->tax_type ?? '';
                $tax = $bankData->data->data->tax ?? 0;
                if (0 == $this->modifyTaxType) {
                    $this->modifyTax = floatval(bcmul($tax, 100, PublicEnum::DefaultScale));
                } else {
                    $this->modifyTax = floatval($tax);
                }
                $this->modifySort = $bankData->data->data->sort ?? '';
                $this->modifyStatus = (1 == ($bankData->data->data->status ?? 1));
                $this->modifyImageUrl = $bankData->data->data->image ?? '';
                //展示POP
                session()->flash('showPopId', 'wdBankPop');
            }
        }
    }

    //修改
    public function doModify()
    {
        if (!empty($this->modifyBankId)) {
            //抓取資訊
            $common = new Common();
            $requests = $common->getRequest(['bank_id' => $this->modifyBankId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            $mainController = new MainController();
            $bankData = $mainController->view($requests['request'])->getData();
            //串接修改request
            $modifyParams = [];
            if ($this->modifyBankAlias != ($bankData->data->data->account_alias ?? '')) {
                $modifyParams['alias'] = $this->modifyBankAlias;
            }
            if ($this->modifyMinAmount != ($bankData->data->data->min_amount ?? '')) {
                $modifyParams['min_amount'] = $this->modifyMinAmount;
            }
            if ($this->modifyMaxAmount != ($bankData->data->data->max_amount ?? '')) {
                $modifyParams['max_amount'] = $this->modifyMaxAmount;
            }
            if ($this->modifyTaxType != ($bankData->data->data->tax_type ?? '')) {
                $modifyParams['tax_type'] = $this->modifyTaxType;
            }
            //先根據type將百分比換算成小數在跟原本數值比較是否有異動
            $modifyTax = $this->modifyTax;
            if (0 == $this->modifyTaxType) {
                $modifyTax = bcdiv($this->modifyTax, 100, 5);
            }
            if ($modifyTax != ($bankData->data->data->tax ?? 0)) {
                $modifyParams['tax'] = $modifyTax;
            }
            if ($this->modifySort != ($bankData->data->data->sort ?? '')) {
                $modifyParams['sort'] = $this->modifySort;
            }
            if ($this->modifyStatus != (1 == ($bankData->data->data->status ?? 1))) {
                $modifyParams['status'] = $this->modifyStatus ? 1 : 0;
            }
            if ($this->modifyImageUrl != ($bankData->data->data->image ?? '')) {
                $modifyParams['image'] = $this->modifyImageUrl;
            }
            //無異動
            if (empty($modifyParams)) {
                //顯示錯誤訊息POP
                session()->flash('pagePopMessage', __('web_message.general.noChange'));
                //恢復原本POP
                session()->flash('popId', 'wdBankPop');
            } else {
                $modifyParams = array_merge(['bank_id' => $this->modifyBankId], $modifyParams);
                $updateRequests = $common->getRequest($modifyParams);
                if (PublicEnum::ResponseCode['needLogin'] == $updateRequests['code']) {
                    $common->doLogout();
                }
                //修改接口
                $updateMenu = $mainController->update($updateRequests['request'])->getData();
                if (PublicEnum::ResponseCode['success'] == $updateMenu->code) {
                    $searchParams = $this->getSearchParams();
                    $this->getDataList($searchParams);
                    session()->flash('pagePopMessage', __('web_message.withdrawalSetting.modifyWdBank') . ' ' . __('web_message.general.success'));
                } else {
                    //顯示錯誤訊息POP
                    session()->flash('pagePopMessage', collect($updateMenu->message ?? [])->implode('; '));
                    //恢復原本POP
                    session()->flash('popId', 'wdBankPop');
                }
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
        $mainController = new MainController();
        //手續費類型
        $feeTypeListResult = $mainController->getFeeTypeDropdown($dropDownRequests['request'])->getData();
        //提領渠道
        $channelResult = $mainController->getGatewayTypeDropdown($dropDownRequests['request'])->getData();
        //塞進session
        $pageCacheData = session()->get('pageCacheData') ?? [];
        $pageCacheData['feeTypeList'] = $feeTypeListResult->data->list ?? [];
        $pageCacheData['channelList'] = $channelResult->data->list ?? [];
        session()->put('pageCacheData', $pageCacheData);
    }

    //currency key塞id
    private function getCurrencyList($params)
    {
        $currencyResult = $this->getCurrency($params);
        if (PublicEnum::ResponseCode['success'] == $currencyResult->code) {
            $currencyList = [];
            foreach (($currencyResult->data->list ?? []) as $data) {
                $currencyList[$data->id] = [
                    'id' => $data->id,
                    'name' => $data->name,
                    'withdrawal_rate' => $data->withdrawal_rate,
                ];
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

    //參數串接
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
        $mainController = new MainController();
        $dataList = $mainController->getList($requests['request'])->getData();
        //dd($dataList);
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
