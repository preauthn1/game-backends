<?php

namespace App\Livewire\System;

use Livewire\Component;
use App\Enums\PublicEnum;
use App\Livewire\Common;
use App\Http\Controllers\Country\MainController;

class CountryCodeMgmt extends Component
{
    //搜尋
    public $isoCode, $countryCode, $status;
    //新增
    public $addCountryCode, $addIsoCode, $addCountryName, $addPhoneRule, $addOrder = 0, $addStatus = true;
    //修改
    public $modifyId, $modifyCountryCode, $modifyIsoCode, $modifyCountryName, $modifyPhoneRule, $modifyOrder, $modifyStatus;

    public function mount()
    {
        $this->getDropdown();
        $searchParams = $this->getSearchParams();
        $this->getDataList($searchParams);
    }

    public function render()
    {
        return view('livewire.system.country-code-mgmt');
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
    public function doSearch(): void
    {
        session()->put('nowPage', PublicEnum::DefaultPage);
        $searchParams = $this->getSearchParams();
        $this->getDataList($searchParams);
    }

    //新增
    public function addCountry()
    {
        //頁面傳參
        $addParams = [
            'countryCode' => $this->addCountryCode,
            'isoCode' => $this->addIsoCode,
            'countryName' => $this->addCountryName,
            'phoneRule' => $this->addPhoneRule,
            'status' => $this->addStatus ? 1 : 0,
        ];
        if (!empty($this->addOrder ?? 0)) {
            $addParams['order'] = $this->addOrder;
        }
        //串接request
        $common = new Common();
        $requests = $common->getRequest($addParams);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        //新增接口
        $mainController = new MainController();
        $addResult = $mainController->create($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $addResult->code) {
            redirect()->to('/countryCodeMgmt')->with('pagePopMessage', __('web_message.menu.system.addCountryCode') . ' ' . __('web_message.general.success'));
        } else {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', collect($addResult->message ?? [])->implode('; '));
            //恢復原本POP
            session()->flash('popId', 'addCountry');
        }
    }

    //修改POP
    public function showModify($countryId)
    {
        if (!empty($countryId)) {
            //串接request
            $common = new Common();
            $requests = $common->getRequest(['id' => $countryId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            //資訊
            $mainController = new MainController();
            $data = $mainController->view($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $data->code) {
                $this->modifyId = $data->data->data->id;
                $this->modifyCountryCode = $data->data->data->country_code ?? '';
                $this->modifyIsoCode = $data->data->data->code ?? '';
                $this->modifyCountryName = $data->data->data->name ?? '';
                $this->modifyPhoneRule = $data->data->data->phone_rule ?? '';
                $this->modifyOrder = $data->data->data->sort ?? 0;
                $this->modifyStatus = (1 == ($data->data->data->status ?? 1));
                //展示POP
                session()->flash('showPopId', 'modifyCountry');
            }
        }
    }

    //修改
    public function doModify()
    {
        if (!empty($this->modifyId)) {
            //抓取資訊
            $common = new Common();
            $requests = $common->getRequest(['id' => $this->modifyId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            $mainController = new MainController();
            $originData = $mainController->view($requests['request'])->getData();
            //串接修改request
            $modifyParams = [];
            if ($this->modifyCountryCode != ($originData->data->data->country_code ?? '')) {
                $modifyParams['countryCode'] = $this->modifyCountryCode;
            }
            if ($this->modifyIsoCode != ($originData->data->data->code ?? '')) {
                $modifyParams['isoCode'] = $this->modifyIsoCode;
            }
            if ($this->modifyCountryName != ($originData->data->data->name ?? '')) {
                $modifyParams['countryName'] = $this->modifyCountryName;
            }
            if ($this->modifyPhoneRule != ($originData->data->data->phone_rule ?? '')) {
                $modifyParams['phoneRule'] = $this->modifyPhoneRule;
            }
            if ($this->modifyOrder != ($originData->data->data->sort ?? 0)) {
                $modifyParams['order'] = $this->modifyOrder;
            }
            if ($this->modifyStatus != (1 == ($originData->data->data->status ?? 1))) {
                $modifyParams['status'] = $this->modifyStatus ? 1 : 0;
            }
            //無異動
            if (empty($modifyParams)) {
                //顯示錯誤訊息POP
                session()->flash('pagePopMessage', __('web_message.general.noChange'));
                //恢復原本POP
                session()->flash('popId', 'modifyCountry');
            } else {
                $modifyParams = array_merge(['id' => $this->modifyId], $modifyParams);
                $updateRequests = $common->getRequest($modifyParams);
                if (PublicEnum::ResponseCode['needLogin'] == $updateRequests['code']) {
                    $common->doLogout();
                }
                //修改接口
                $updateResult = $mainController->update($updateRequests['request'])->getData();
                if (PublicEnum::ResponseCode['success'] == $updateResult->code) {
                    $searchParams = $this->getSearchParams();
                    $this->getDataList($searchParams);
                    session()->flash('pagePopMessage', __('web_message.menu.system.modifyCountryCode') . ' ' . __('web_message.general.success'));
                } else {
                    //顯示錯誤訊息POP
                    session()->flash('pagePopMessage', collect($updateResult->message ?? [])->implode('; '));
                    //恢復原本POP
                    session()->flash('popId', 'modifyCountry');
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
        //狀態
        $statusList = $mainController->getStatusDropdown($dropDownRequests['request'])->getData();
        //塞進session
        $pageCacheData = session()->get('pageCacheData') ?? [];
        $pageCacheData['statusList'] = $statusList->data->list ?? [];
        session()->put('pageCacheData', $pageCacheData);
    }

    //搜尋參數串接
    private function getSearchParams(): array
    {
        $searchParams = [];
        if (!empty($this->isoCode ?? '')) {
            $searchParams['isoCode'] = $this->isoCode;
        }
        if (!empty($this->countryCode ?? '')) {
            $searchParams['countryCode'] = $this->countryCode;
        }
        if (-1 != ($this->status ?? -1)) {
            $searchParams['status'] = $this->status;
        }
        $searchParams['page'] = PublicEnum::DefaultPage;
        if (!empty(session('nowPage') ?? 0)) {
            $searchParams['page'] = session('nowPage');
        }
        $searchParams['per_page'] = PublicEnum::DefaultPageSize;
        return $searchParams;
    }

    private function getDataList($params)
    {
        $common = new Common();
        $requests = $common->getRequest($params);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $mainController = new MainController();
        $dataList = $mainController->getList($requests['request'])->getData();
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
