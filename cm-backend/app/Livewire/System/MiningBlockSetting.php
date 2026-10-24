<?php

namespace App\Livewire\System;

use App\Enums\PublicEnum;
use App\Services\AgentService;
use App\Http\Controllers\Miner\MiningBlockController;
use App\Http\Controllers\Currency\MainController;
use App\Livewire\Common;
use Livewire\Component;
use App\Enums\AgentCustomizedSettingsEnum;

class MiningBlockSetting extends Component
{
    //監聽時間相關
    protected $listeners = [
        'addMiningBlockTime' => 'addMiningBlockTime',
        'modifyMiningBlockTime' => 'modifyMiningBlockTime',
    ];
    //新增
    public $addMiningBlockCurrencyId, $addMiningBlockHeight, $addMiningBlockTime, $addMiningBlockDifficulty, $addMiningBlockChange, $addMiningBlockReward;
    //POP
    public $popMiningBlockId;
    //修改
    public $modifyMiningBlockCurrencyId, $modifyMiningBlockHeight, $modifyMiningBlockTime, $modifyMiningBlockDifficulty, $modifyMiningBlockChange, $modifyMiningBlockReward, $modifyMiningBlockStatus;

    //監聽新增挖礦難度時間
    public function addMiningBlockTime($date)
    {
        $common = new Common();
        $dateCheck = $common->checkDateTime($date);
        if (PublicEnum::ResponseCode['success'] != $dateCheck['code']) {
            session()->flash('pagePopMessage', $dateCheck['message']);
            $this->addMiningBlockTime = '';
        } else {
            $this->addMiningBlockTime = $date;
        }
        //不更新畫面
        $this->skipRender();
    }

    //監聽修改挖礦難度時間
    public function modifyMiningBlockTime($date)
    {
        $common = new Common();
        $dateCheck = $common->checkDateTime($date);
        if (PublicEnum::ResponseCode['success'] != $dateCheck['code']) {
            session()->flash('pagePopMessage', $dateCheck['message']);
            $this->modifyMiningBlockTime = '';
        } else {
            $this->modifyMiningBlockTime = $date;
        }
        //不更新畫面
        $this->skipRender();
    }

    public function mount()
    {
        $this->currencyDropdown();
        $this->statusDropdown();
        $this->getSomeInfo();
        $searchParams = $this->getSearchParams();
        $this->getPageData($searchParams);
    }

    public function render()
    {
        return view('livewire.system.mining-block-setting');
    }

    //分頁切換
    public function changePage($oldPage, $newPage)
    {
        if ($oldPage != $newPage) {
            if (1 > $newPage) $newPage = 1;
            if ($newPage > session()->get('pageCacheData')['totalPage']) $newPage = session()->get('pageCacheData')['totalPage'];
            session()->put('nowPage', ($newPage ?? PublicEnum::DefaultPage));
            $searchParams = $this->getSearchParams();
            $this->getPageData($searchParams);
        }
    }

    //新增
    public function addMiningBlock()
    {
        //頁面傳參
        $addParams = [
            'currency_id' => $this->addMiningBlockCurrencyId,
            'block_height' => $this->addMiningBlockHeight,
            'block_time' => $this->addMiningBlockTime,
            'mining_difficulty' => $this->addMiningBlockDifficulty,
            'mining_change' => $this->addMiningBlockChange,
            'block_reward' => $this->addMiningBlockReward,
        ];
        //串接request
        $common = new Common();
        $requests = $common->getRequest($addParams);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        //新增接口
        $miningBlockController = new MiningBlockController();
        $addRankList = $miningBlockController->create($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $addRankList->code) {
            redirect()->to('/miningBlockSetting')->with('pagePopMessage', __('web_message.menu.system.addMiningBlock') . ' ' . __('web_message.general.success'));
        } else {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', collect($addRankList->message ?? [])->implode('; '));
            //恢復原本POP
            session()->flash('popId', 'addMiningBlock');
        }
    }

    //修改POP
    public function showDataPop($miningBlockId = '', $action = '')
    {
        if (!empty($miningBlockId) && !empty($action)) {
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $pageCacheData['popAction'] = $action;
            session()->put('pageCacheData', $pageCacheData);
            //串接request
            $common = new Common();
            $requests = $common->getRequest(['mining_block_id' => $miningBlockId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            //資訊
            $miningBlockController = new MiningBlockController();
            $miningBlockData = $miningBlockController->view($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $miningBlockData->code) {
                $this->popMiningBlockId = $miningBlockData->data->data->id ?? '';
                $this->modifyMiningBlockCurrencyId = $miningBlockData->data->data->currency_id ?? '';
                $this->modifyMiningBlockHeight = $miningBlockData->data->data->block_height ?? 0;
                $this->modifyMiningBlockTime = $miningBlockData->data->data->block_time ?? '';
                $this->modifyMiningBlockDifficulty = floatval($miningBlockData->data->data->mining_difficulty ?? 0);
                $this->modifyMiningBlockChange = floatval($miningBlockData->data->data->mining_change ?? 0);
                $this->modifyMiningBlockReward = floatval($miningBlockData->data->data->block_reward ?? 0);
                $this->modifyMiningBlockStatus = $miningBlockData->data->data->status ?? 1;
                //展示POP
                session()->flash('showPopId', 'modifyMiningBlock');
            }
        }
    }

    //修改
    public function doModify()
    {
        if (!empty($this->popMiningBlockId)) {
            //抓取資訊
            $common = new Common();
            $requests = $common->getRequest(['mining_block_id' => $this->popMiningBlockId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            $miningBlockController = new MiningBlockController();
            $miningBlockData = $miningBlockController->view($requests['request'])->getData();
            //串接修改request
            $modifyParams = [];
            if ($this->modifyMiningBlockCurrencyId != ($miningBlockData->data->data->currency_id ?? '')) {
                $modifyParams['currency_id'] = $this->modifyMiningBlockCurrencyId;
            }
            if ($this->modifyMiningBlockHeight != ($miningBlockData->data->data->block_height ?? '')) {
                $modifyParams['block_height'] = $this->modifyMiningBlockHeight;
            }
            if ($this->modifyMiningBlockTime != ($miningBlockData->data->data->block_time ?? '')) {
                $modifyParams['block_time'] = $this->modifyMiningBlockTime;
            }
            if ($this->modifyMiningBlockDifficulty != ($miningBlockData->data->data->mining_difficulty ?? '')) {
                $modifyParams['mining_difficulty'] = $this->modifyMiningBlockDifficulty;
            }
            if ($this->modifyMiningBlockChange != ($miningBlockData->data->data->mining_change ?? '')) {
                $modifyParams['mining_change'] = $this->modifyMiningBlockChange;
            }
            if ($this->modifyMiningBlockReward != ($miningBlockData->data->data->block_reward ?? '')) {
                $modifyParams['block_reward'] = $this->modifyMiningBlockReward;
            }
            //無異動
            if (empty($modifyParams)) {
                //顯示錯誤訊息POP
                session()->flash('pagePopMessage', __('web_message.general.noChange'));
                //恢復原本POP
                session()->flash('popId', 'modifyMiningBlock');
            } else {
                $origin = [
                    'mining_block_id' => $this->popMiningBlockId
                ];
                $modifyParams = array_merge($origin, $modifyParams);
                $updateRequests = $common->getRequest($modifyParams);
                if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                    $common->doLogout();
                }
                //修改接口
                $updateRankList = $miningBlockController->update($updateRequests['request'])->getData();
                if (PublicEnum::ResponseCode['success'] == $updateRankList->code) {
                    $searchParams = $this->getSearchParams();
                    $this->getPageData($searchParams);
                    session()->flash('pagePopMessage', __('web_message.menu.system.modifyMiningBlock') . ' ' . __('web_message.general.success'));
                } else {
                    //顯示錯誤訊息POP
                    session()->flash('pagePopMessage', collect($updateRankList->message ?? [])->implode('; '));
                    //恢復原本POP
                    session()->flash('popId', 'modifyMiningBlock');
                }
            }
        }
    }

    //審核通過
    public function doApprove()
    {
        if (empty($this->popMiningBlockId)) {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', __('error_message.9001'));
            //恢復原本POP
            session()->flash('popId', 'modifyMiningBlock');
            return;
        }
        //抓取資訊
        $common = new Common();
        $requests = $common->getRequest(['mining_block_id' => $this->popMiningBlockId]);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $miningBlockController = new MiningBlockController();
        $miningBlockData = $miningBlockController->view($requests['request'])->getData();
        if (0 != ($miningBlockData->data->data->status ?? -1)) {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', __('error_message.5035'));
            //恢復原本POP
            session()->flash('popId', 'modifyMiningBlock');
            return;
        }
        //串接修改request
        $modifyParams = [
            'mining_block_id' => $this->popMiningBlockId,
            'status' => 1,
        ];
        $updateRequests = $common->getRequest($modifyParams);
        if (PublicEnum::ResponseCode['needLogin'] == $updateRequests['code']) {
            $common->doLogout();
        }
        //修改接口
        $updateRankList = $miningBlockController->updateStatus($updateRequests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $updateRankList->code) {
            $searchParams = $this->getSearchParams();
            $this->getPageData($searchParams);
            session()->flash('pagePopMessage', __('web_message.menu.system.modifyMiningBlock') . ' ' . __('web_message.general.success'));
        } else {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', collect($updateRankList->message ?? [])->implode('; '));
            //恢復原本POP
            session()->flash('popId', 'modifyMiningBlock');
        }
    }

    //刪除
    public function doDelete($miningBlockId)
    {
        if (!empty($miningBlockId)) {
            //串接request
            $common = new Common();
            $requests = $common->getRequest(['mining_block_id' => $miningBlockId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            //刪除接口
            $miningBlockController = new MiningBlockController();
            $deleteRankList = $miningBlockController->delete($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $deleteRankList->code) {
                $searchParams = $this->getSearchParams();
                $this->getPageData($searchParams);
                session()->flash('pagePopMessage', __('web_message.menu.system.deleteMiningBlock') . ' ' . __('web_message.general.success'));
            } else {
                //顯示錯誤訊息POP
                session()->flash('pagePopMessage', collect($deleteRankList->message ?? [])->implode('; '));
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

    private function statusDropdown(): void
    {
        //接口
        $common = new Common();
        $dropDownRequests = $common->getRequest([]);
        if (PublicEnum::ResponseCode['needLogin'] == $dropDownRequests['code']) {
            $common->doLogout();
        }
        $miningBlockController = new MiningBlockController();
        $dropDownResult = $miningBlockController->getStatusDropdown($dropDownRequests['request'])->getData();
        $pageCacheData = session()->get('pageCacheData') ?? [];
        $pageCacheData['statusList'] = $dropDownResult->data->list ?? [];
        session()->put('pageCacheData', $pageCacheData);
    }

    private function getSomeInfo()
    {
        //接口
        $common = new Common();
        $infoRequests = $common->getRequest(['setting_key' => AgentCustomizedSettingsEnum::MINING_BLOCK_URL]);
        if (PublicEnum::ResponseCode['needLogin'] == $infoRequests['code']) {
            $common->doLogout();
        }
        $agentService = new AgentService();
        $infoResult = $agentService->getCustomizedSetting($infoRequests['request'])?->value;
        $pageCacheData = session()->get('pageCacheData') ?? [];
        $pageCacheData['miningBlockUrl'] = $infoResult ?? '';
        session()->put('pageCacheData', $pageCacheData);
    }

    //搜尋參數串接
    private function getSearchParams(): array
    {
        $searchParams = [];
        $searchParams['page'] = PublicEnum::DefaultPage;
        if (!empty(session('nowPage') ?? 0)) {
            $searchParams['page'] = session('nowPage');
        }
        $searchParams['per_page'] = PublicEnum::DefaultPageSize;
        return $searchParams;
    }

    private function getPageData($params)
    {
        $common = new Common();
        $requests = $common->getRequest($params);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $miningBlockController = new MiningBlockController();
        $blockData = $miningBlockController->getList($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $blockData->code) {
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $pageCacheData['pageData'] = $blockData->data->list ?? [];
            $pageCacheData['totalPage'] = $blockData->data->pagination->pages ?? PublicEnum::DefaultPage;
            $pageCacheData['totalRow'] = $blockData->data->pagination->total ?? 0;
            session()->put('nowPage', $blockData->data->pagination->current ?? PublicEnum::DefaultPage);
            session()->put('pageCacheData', $pageCacheData);
        } else {
            session()->flash('pagePopMessage', collect($blockData->message ?? [])->implode('; '));
        }
    }
}
