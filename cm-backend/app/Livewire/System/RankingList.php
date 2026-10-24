<?php

namespace App\Livewire\System;

use App\Enums\PublicEnum;
use App\Http\Controllers\Miner\RankingController;
use App\Livewire\Common;
use Livewire\Component;

class RankingList extends Component
{
    //新增
    public $addRankListUsername, $addRankListQty, $addRankListRebate;
    //修改
    public $modifyRankListId, $modifyRankListUsername, $modifyRankListQty, $modifyRankListRebate;

    public function mount()
    {
        $searchParams = $this->getSearchParams();
        $this->getRankListData($searchParams);
    }

    public function render()
    {
        return view('livewire.system.ranking-list');
    }

    //分頁切換
    public function changePage($oldPage, $newPage)
    {
        if ($oldPage != $newPage) {
            if (1 > $newPage) $newPage = 1;
            if ($newPage > session()->get('pageCacheData')['totalPage']) $newPage = session()->get('pageCacheData')['totalPage'];
            session()->put('nowPage', ($newPage ?? PublicEnum::DefaultPage));
            $searchParams = $this->getSearchParams();
            $this->getRankListData($searchParams);
        }
    }

    //新增
    public function addRankList()
    {
        //頁面傳參
        $addParams = [
            'username' => $this->addRankListUsername,
            'qty' => $this->addRankListQty,
            'rebate_amount' => $this->addRankListRebate,
        ];
        //串接request
        $common = new Common();
        $requests = $common->getRequest($addParams);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        //新增接口
        $rankingController = new RankingController();
        $addRankList = $rankingController->create($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $addRankList->code) {
            redirect()->to('/rankingList')->with('pagePopMessage', __('web_message.menu.system.addRankList') . ' ' . __('web_message.general.success'));
        } else {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', collect($addRankList->message ?? [])->implode('; '));
            //恢復原本POP
            session()->flash('popId', 'addRankList');
        }
    }

    //修改POP
    public function showModify($rankListId)
    {
        if (!empty($rankListId)) {
            //串接request
            $common = new Common();
            $requests = $common->getRequest(['ranking_id' => $rankListId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            //資訊
            $rankingController = new RankingController();
            $rankListData = $rankingController->view($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $rankListData->code) {
                $this->modifyRankListId = $rankListData->data->data->id;
                $this->modifyRankListUsername = $rankListData->data->data->username ?? '';
                $this->modifyRankListQty = $rankListData->data->data->quantity_sold ?? 0;
                $this->modifyRankListRebate = floatval($rankListData->data->data->rebate_amount ?? 0);
                //展示POP
                session()->flash('showPopId', 'modifyRankList');
            }
        }
    }

    //修改
    public function doModify()
    {
        if (!empty($this->modifyRankListId)) {
            //抓取資訊
            $common = new Common();
            $requests = $common->getRequest(['ranking_id' => $this->modifyRankListId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            $rankingController = new RankingController();
            $rankListData = $rankingController->view($requests['request'])->getData();
            //串接修改request
            $modifyParams = [];
            if ($this->modifyRankListQty != ($rankListData->data->data->quantity_sold ?? 0)) {
                $modifyParams['qty'] = $this->modifyRankListQty;
            }
            if ($this->modifyRankListRebate != ($rankListData->data->data->rebate_amount ?? 0)) {
                $modifyParams['rebate_amount'] = $this->modifyRankListRebate;
            }
            //無異動
            if (empty($modifyParams)) {
                //顯示錯誤訊息POP
                session()->flash('pagePopMessage', __('web_message.general.noChange'));
                //恢復原本POP
                session()->flash('popId', 'modifyRankList');
            } else {
                $origin = [
                    'ranking_id' => $this->modifyRankListId,
                    'qty' => ($rankListData->data->data->quantity_sold ?? 0),
                    'rebate_amount' => (float)($rankListData->data->data->rebate_amount ?? 0),
                ];
                $modifyParams = array_merge($origin, $modifyParams);
                $updateRequests = $common->getRequest($modifyParams);
                if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                    $common->doLogout();
                }
                //修改接口
                $updateRankList = $rankingController->update($updateRequests['request'])->getData();
                if (PublicEnum::ResponseCode['success'] == $updateRankList->code) {
                    $searchParams = $this->getSearchParams();
                    $this->getRankListData($searchParams);
                    session()->flash('pagePopMessage', __('web_message.menu.system.modifyRankList') . ' ' . __('web_message.general.success'));
                } else {
                    //顯示錯誤訊息POP
                    session()->flash('pagePopMessage', collect($updateRankList->message ?? [])->implode('; '));
                    //恢復原本POP
                    session()->flash('popId', 'modifyRankList');
                }
            }
        }
    }

    //刪除
    public function doDelete($rankListId)
    {
        if (!empty($rankListId)) {
            //串接request
            $common = new Common();
            $requests = $common->getRequest(['ranking_id' => $rankListId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            //刪除接口
            $rankingController = new RankingController();
            $deleteRankList = $rankingController->delete($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $deleteRankList->code) {
                $searchParams = $this->getSearchParams();
                $this->getRankListData($searchParams);
                session()->flash('pagePopMessage', __('web_message.menu.system.deleteRankList') . ' ' . __('web_message.general.success'));
            } else {
                //顯示錯誤訊息POP
                session()->flash('pagePopMessage', collect($deleteRankList->message ?? [])->implode('; '));
            }
        }
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

    private function getRankListData($params)
    {
        $common = new Common();
        $requests = $common->getRequest($params);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $rankingController = new RankingController();
        $rankListData = $rankingController->getList($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $rankListData->code) {
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $pageCacheData['pageData'] = $rankListData->data->list ?? [];
            $pageCacheData['totalPage'] = $rankListData->data->pagination->pages ?? PublicEnum::DefaultPage;
            $pageCacheData['totalRow'] = $rankListData->data->pagination->total ?? 0;
            session()->put('nowPage', $rankListData->data->pagination->current ?? PublicEnum::DefaultPage);
            session()->put('pageCacheData', $pageCacheData);
        } else {
            session()->flash('pagePopMessage', collect($rankListData->message ?? [])->implode('; '));
        }
    }
}
