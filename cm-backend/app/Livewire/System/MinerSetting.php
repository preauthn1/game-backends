<?php

namespace App\Livewire\System;

use App\Enums\PublicEnum;
use App\Livewire\Common;
use App\Helpers\Common as Helper;
use Livewire\Component;
use App\Http\Controllers\Miner\PurchaseController;
use App\Http\Controllers\Currency\MainController;
use Livewire\WithFileUploads;

class MinerSetting extends Component
{
    use WithFileUploads;

    //搜尋
    public $minerName, $minerDisplay, $minerStatus = 1;
    //新增
    public $addMinerName, $addMinerImage, $addMinerDayRevenue, $addMinerOrder = 0, $addMinerDisplay=true ,$addMinerStatus = true, $addMiningCurrency, $addMinerHashRate, $addMinerPowerConsumption, $addMinerDayElecFee, $addMinerCustodyFee, $addMinerCustodyFeeRate, $addMinerDepreciationCycle, $addMinerTagTxt, $addMinerTagColor, $addMinerPurchasePrice, $addMinerPurchaseTotal, $addMinerPurchaseRentCount, $addMinerPurchaseRentCycle, $addMinerRentPrice, $addMinerExperienceCommission = '0.00', $addMinerGroupTotal, $addMinerGroupRentCount, $addMinerGroupRentCycle, $addMinerRebatePercentage1, $addMinerRebatePercentage2, $addMinerRebatePercentage3, $addMinerRewardQuantity3, $addMinerRewardQuantity5, $addMinerRewardQuantity10;
    //POP
    public $popMinerId, $popMinerName, $popMinerImageUrl, $popMiningCurrency, $popMinerDepreciationCycle, $popMinerHashRate, $popMinerPowerConsumption, $popMinerDayElecFee, $popMinerCustodyFee, $popMinerCustodyFeeRate, $popMinerPurchasePrice, $popMinerRentPrice;
    //修改
    public $popMinerImage, $popMinerDayRevenue, $popMinerOrder, $popMinerDisplay, $popMinerStatus, $popMinerTagTxt, $popMinerTagColor, $popMinerPurchaseTotal, $popMinerPurchaseRentCount, $popMinerPurchaseRentCycle, $popMinerExperienceCommission, $popMinerGroupTotal, $popMinerGroupRentCount, $popMinerGroupRentCycle, $popMinerRebatePercentage1, $popMinerRebatePercentage2, $popMinerRebatePercentage3, $popMinerRewardQuantity3, $popMinerRewardQuantity5, $popMinerRewardQuantity10;
    //監聽
    protected $listeners = [
        'addMinerImage' => 'addMinerImage',
        'popMinerImage' => 'popMinerImage',
    ];

    //監聽新增礦機圖片
    public function addMinerImage($data)
    {
        $this->addMinerImage = $data;
        //不更新畫面
        $this->skipRender();
    }

    //監聽彈窗礦機圖片
    public function popMinerImage($data)
    {
        $this->popMinerImage = $data;
        //不更新畫面
        $this->skipRender();
    }

    public function mount()
    {
        $this->currencyDropdown();
        $searchParams = $this->getSearchParams();
        $this->getMinerList($searchParams);
    }

    public function render()
    {
        return view('livewire.system.miner-setting');
    }

    //分頁切換
    public function changePage($oldPage, $newPage): void
    {
        if ($oldPage != $newPage) {
            if (1 > $newPage) $newPage = 1;
            if ($newPage > session()->get('pageCacheData')['totalPage']) $newPage = session()->get('pageCacheData')['totalPage'];
            session()->put('nowPage', ($newPage ?? PublicEnum::DefaultPage));
            $searchParams = $this->getSearchParams();
            $this->getMinerList($searchParams);
        }
    }

    //搜尋
    public function doSearch(): void
    {
        session()->put('nowPage', PublicEnum::DefaultPage);
        $searchParams = $this->getSearchParams();
        $this->getMinerList($searchParams);
    }

    //新增
    public function addMiner()
    {
        //頁面傳參
        $addParams = [
            'name' => $this->addMinerName,
            'image' => $this->addMinerImage,
            'day_revenue' => $this->addMinerDayRevenue,
            'order' => $this->addMinerOrder ?? 0,
            'is_show' => $this->addMinerDisplay ? 1 : 0,
            'status' => $this->addMinerStatus ? 1 : 0,
            'currency_id' => $this->addMiningCurrency,
            'depreciation_cycle' => $this->addMinerDepreciationCycle,
            'hash_rate' => $this->addMinerHashRate,
            'power_consumption' => $this->addMinerPowerConsumption,
            'day_elec_fee' => $this->addMinerDayElecFee,
            'custody_fee' => $this->addMinerCustodyFee,
            'custody_fee_rate' => $this->addMinerCustodyFeeRate,
            'tag' => $this->addMinerTagTxt,
            'tag_color' => $this->addMinerTagColor,
            'purchase_price' => $this->addMinerPurchasePrice,
            'purchase_total' => $this->addMinerPurchaseTotal,
            'promo_purchase_num' => $this->addMinerPurchaseRentCount,
            'promo_purchase_days' => $this->addMinerPurchaseRentCycle,
            'rent_price' => $this->addMinerRentPrice,
            'group_total' => $this->addMinerGroupTotal,
            'promo_group_num' => $this->addMinerGroupRentCount,
            'promo_group_days' => $this->addMinerGroupRentCycle,
            'rebate_percentage_1' => $this->addMinerRebatePercentage1,
            'rebate_percentage_2' => $this->addMinerRebatePercentage2,
            'rebate_percentage_3' => $this->addMinerRebatePercentage3,
            'reward_quantity_3' => $this->addMinerRewardQuantity3,
            'reward_quantity_5' => $this->addMinerRewardQuantity5,
            'reward_quantity_10' => $this->addMinerRewardQuantity10,
            'experience_commission' => $this->addMinerExperienceCommission,
        ];
        //串接request
        $common = new Common();
        $requests = $common->getRequest($addParams);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        //新增接口
        $purchaseController = new PurchaseController();
        $addMiner = $purchaseController->create($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $addMiner->code) {
            redirect()->to('/minerSetting')->with('pagePopMessage', __('web_message.menu.system.addMiner') . ' ' . __('web_message.general.success'));
        } else {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', collect($addMiner->message ?? [])->implode('; '));
            //恢復原本POP
            session()->flash('popId', 'addMiner');
        }
    }

    //修改/詳細 POP
    public function showDataPop($minerId = '', $action = '')
    {
        if (!empty($minerId) && !empty($action)) {
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $pageCacheData['popAction'] = $action;
            session()->put('pageCacheData', $pageCacheData);
            //串接request
            $common = new Common();
            $requests = $common->getRequest(['miner_id' => $minerId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            //資訊
            $purchaseController = new PurchaseController();
            $minerData = $purchaseController->view($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $minerData->code) {
                $this->popMinerId = $minerData->data->data->miner_id ?? '';
                $this->popMinerName = $minerData->data->data->name ?? '';
                $this->popMinerImageUrl = $minerData->data->data->image ?? '';
                $this->popMinerDayRevenue = number_format($minerData->data->data->day_revenue, PublicEnum::DefaultScale);
                $this->popMinerOrder = $minerData->data->data->sort ?? 0;
                $this->popMinerDisplay = (1 == $minerData->data->data->is_show ?? 0);
                $this->popMinerStatus = (1 == $minerData->data->data->status ?? 0);
                $this->popMiningCurrency = $minerData->data->data->currency_id ?? '';
                $this->popMinerDepreciationCycle = $minerData->data->data->depreciation_cycle ?? '';
                $this->popMinerHashRate = $minerData->data->data->hash_rate ?? '';
                $this->popMinerPowerConsumption = $minerData->data->data->power_consumption ?? '';
                $this->popMinerDayElecFee = $minerData->data->data->info->day_elec_fee ?? '';
                $this->popMinerCustodyFee = $minerData->data->data->info->custody_fee ?? '';
                $this->popMinerCustodyFeeRate = $minerData->data->data->info->custody_fee_rate ?? '';
                $this->popMinerTagTxt = $minerData->data->data->info->tag ?? '';
                $this->popMinerTagColor = $minerData->data->data->info->tag_color ?? '';
                $this->popMinerPurchasePrice = number_format($minerData->data->data->purchase_price, PublicEnum::DefaultScale);
                $this->popMinerPurchaseTotal = $minerData->data->data->purchase_total ?? 0;
                $this->popMinerPurchaseRentCount = $minerData->data->data->promo->purchase->num ?? '';
                $this->popMinerPurchaseRentCycle = $minerData->data->data->promo->purchase->cycle ?? '';
                $this->popMinerRentPrice = number_format($minerData->data->data->rent_price, PublicEnum::DefaultScale);
                $this->popMinerExperienceCommission = $minerData->data->data->promo->rent->experience_commission ?? '0.00';
                $this->popMinerGroupTotal = $minerData->data->data->group_total ?? 0;
                $this->popMinerGroupRentCount = $minerData->data->data->promo->group->num ?? '';
                $this->popMinerGroupRentCycle = $minerData->data->data->promo->group->cycle ?? '';
                $this->popMinerRebatePercentage1 = $this->formatRebate($minerData->data->data->rebate_percentage1 ?? '');
                $this->popMinerRebatePercentage2 = $this->formatRebate($minerData->data->data->rebate_percentage2 ?? '');
                $this->popMinerRebatePercentage3 = $this->formatRebate($minerData->data->data->rebate_percentage3 ?? '');
                $this->popMinerRewardQuantity3 = floatval($minerData->data->data->reward_quantity_3 ?? 0);
                $this->popMinerRewardQuantity5 = floatval($minerData->data->data->reward_quantity_5 ?? 0);
                $this->popMinerRewardQuantity10 = floatval($minerData->data->data->reward_quantity_10 ?? 0);
                //展示POP
                session()->flash('showPopId', 'minerPop');
            }
        }
    }

    //修改
    public function doModify()
    {
        if (!empty($this->popMinerId)) {
            //抓取資訊
            $common = new Common();
            $requests = $common->getRequest(['miner_id' => $this->popMinerId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            $purchaseController = new PurchaseController();
            $minerData = $purchaseController->view($requests['request'])->getData();
            //串接修改request
            $modifyParams = [];
            if (!empty($this->popMinerImage ?? '')) {
                $modifyParams['image'] = $this->popMinerImage;
            }
            if ($this->popMinerDayRevenue != ($minerData->data->data->day_revenue ?? '')) {
                $modifyParams['day_revenue'] = $this->popMinerDayRevenue;
            }
            if ($this->popMinerOrder != ($minerData->data->data->sort ?? '')) {
                $modifyParams['order'] = $this->popMinerOrder;
            }
            if ($this->popMinerDisplay != (1 == ($minerData->data->data->is_show ?? 1))) {
                $modifyParams['is_show'] = $this->popMinerDisplay ? 1 : 0;
            }   
            if ($this->popMinerStatus != (1 == ($minerData->data->data->status ?? 1))) {
                $modifyParams['status'] = $this->popMinerStatus ? 1 : 0;
            }
            if ($this->popMinerTagTxt != ($minerData->data->data->info->tag ?? '')) {
                $modifyParams['tag'] = $this->popMinerTagTxt;
            }
            if ($this->popMinerTagColor != ($minerData->data->data->info->tag_color ?? '')) {
                $modifyParams['tag_color'] = $this->popMinerTagColor;
            }
            if ($this->popMinerPurchaseTotal != ($minerData->data->data->purchase_total ?? '')) {
                $modifyParams['purchase_total'] = $this->popMinerPurchaseTotal;
            }
            if ($this->popMinerGroupTotal != ($minerData->data->data->group_total ?? '')) {
                $modifyParams['group_total'] = $this->popMinerGroupTotal;
            }
            if ($this->popMinerExperienceCommission != ($minerData->data->data->promo->rent->experience_commission ?? '')) {
                $modifyParams['experience_commission'] = $this->popMinerExperienceCommission;
            }
            if ($this->popMinerRebatePercentage1 != ($this->formatRebate($minerData->data->data->rebate_percentage1 ?? ''))) {
                $modifyParams['rebate_percentage_1'] = $this->popMinerRebatePercentage1;
            }
            if ($this->popMinerRebatePercentage2 != ($this->formatRebate($minerData->data->data->rebate_percentage2 ?? ''))) {
                $modifyParams['rebate_percentage_2'] = $this->popMinerRebatePercentage2;
            }
            if ($this->popMinerRebatePercentage3 != ($this->formatRebate($minerData->data->data->rebate_percentage3 ?? ''))) {
                $modifyParams['rebate_percentage_3'] = $this->popMinerRebatePercentage3;
            }
            if ($this->popMinerRewardQuantity3 != $minerData->data->data->reward_quantity_3 ?? 0) {
                $modifyParams['reward_quantity_3'] = $this->popMinerRewardQuantity3;
            }
            if ($this->popMinerRewardQuantity5 != $minerData->data->data->reward_quantity_5 ?? 0) {
                $modifyParams['reward_quantity_5'] = $this->popMinerRewardQuantity5;
            }
            if ($this->popMinerRewardQuantity10 != $minerData->data->data->reward_quantity_10 ?? 0) {
                $modifyParams['reward_quantity_10'] = $this->popMinerRewardQuantity10;
            }
            //無異動
            if (empty($modifyParams)) {
                //顯示錯誤訊息POP
                session()->flash('pagePopMessage', __('web_message.general.noChange'));
                //恢復原本POP
                session()->flash('popId', 'minerPop');
            } else {
                $modifyParams = array_merge(['miner_id' => $this->popMinerId], $modifyParams);
                $updateRequests = $common->getRequest($modifyParams);
                if (PublicEnum::ResponseCode['needLogin'] == $updateRequests['code']) {
                    $common->doLogout();
                }
                //修改接口
                $updateMiner = $purchaseController->update($updateRequests['request'])->getData();
                if (PublicEnum::ResponseCode['success'] == $updateMiner->code) {
                    $searchParams = $this->getSearchParams();
                    $this->getMinerList($searchParams);
                    session()->flash('pagePopMessage', __('web_message.menu.system.modifyMiner') . ' ' . __('web_message.general.success'));
                } else {
                    //顯示錯誤訊息POP
                    session()->flash('pagePopMessage', collect($updateMiner->message ?? [])->implode('; '));
                    //恢復原本POP
                    session()->flash('popId', 'minerPop');
                }
            }
        }
    }

    private function currencyDropdown(): void
    {
        //接口
        $common = new Common();
        $dropDownRequests = $common->getRequest(['type' => 3]);
        if (PublicEnum::ResponseCode['needLogin'] == $dropDownRequests['code']) {
            $common->doLogout();
        }
        $mainController = new MainController();
        $dropDownResult = $mainController->getDropdown($dropDownRequests['request'])->getData();
        $pageCacheData = session()->get('pageCacheData') ?? [];
        $pageCacheData['currencyList'] = $dropDownResult->data->list ?? [];
        session()->put('pageCacheData', $pageCacheData);
    }

    //搜尋參數串接
    private function getSearchParams(): array
    {
        $searchParams = [];
        if (!empty($this->minerName ?? '')) {
            $searchParams['name'] = $this->minerName;
        }
        if (-1 != ($this->minerDisplay ?? -1)) {
            $searchParams['is_show'] = $this->minerDisplay;
        }
        if (-1 != ($this->minerStatus ?? -1)) {
            $searchParams['status'] = $this->minerStatus;
        }
        $searchParams['page'] = PublicEnum::DefaultPage;
        if (!empty(session('nowPage') ?? 0)) {
            $searchParams['page'] = session('nowPage');
        }
        $searchParams['per_page'] = PublicEnum::DefaultPageSize;
        return $searchParams;
    }

    private function getMinerList($params): void
    {
        $common = new Common();
        $requests = $common->getRequest($params);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $purchaseController = new PurchaseController();
        $minerList = $purchaseController->getList($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $minerList->code) {
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $pageCacheData['pageData'] = $minerList->data->list ?? [];
            $pageCacheData['totalPage'] = $minerList->data->pagination->pages ?? PublicEnum::DefaultPage;
            $pageCacheData['totalRow'] = $minerList->data->pagination->total ?? 0;
            session()->put('nowPage', $minerList->data->pagination->current ?? PublicEnum::DefaultPage);
            session()->put('pageCacheData', $pageCacheData);
        } else {
            session()->flash('pagePopMessage', collect($minerList->message ?? [])->implode('; '));
        }
    }

    function formatRebate($value)
    {
        return Helper::IsNotNullOrEmptyString($value) ? floatval(number_format($value * 100, PublicEnum::DefaultScale)) : '';
    }
}
