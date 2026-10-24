<?php

namespace App\Livewire\Agent;

use App\Enums\PublicEnum;
use App\Http\Controllers\Member\AgentCenterController;
use App\Livewire\Common;
use Livewire\Component;

class AgentDetail extends Component
{
    public $maxLevel = 0;
    //搜尋
    public $username, $childLevel, $minerName;

    public function render()
    {
        return view('livewire.agent.agent-detail');
    }

    //搜尋
    public function doSearch(): void
    {
        session()->put('nowPage', PublicEnum::DefaultPage);
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

    private function getDropdown(): void
    {
        //接口
        $common = new Common();
        $dropDownRequests = $common->getRequest([]);
        if (PublicEnum::ResponseCode['needLogin'] == $dropDownRequests['code']) {
            $common->doLogout();
        }
        //下級級數
        $agentCenterController = new AgentCenterController();
        $levelResult = $agentCenterController->getLevelDropdown($dropDownRequests['request'])->getData();
        //塞進session
        $pageCacheData = session()->get('pageCacheData') ?? [];
        $pageCacheData['levelList'] = $levelResult->data->list ?? [];
        session()->put('pageCacheData', $pageCacheData);
    }

    //搜尋參數串接
    private function getSearchParams(): array
    {
        $searchParams = [];
        if (!empty($this->username ?? '')) {
            $searchParams['username'] = $this->username;
        }
        if (0 < ($this->childLevel ?? 0)) {
            $searchParams['level'] = $this->childLevel;
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
        $agentCenterController = new AgentCenterController();
        $dataList = $agentCenterController->view($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $dataList->code) {
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $this->maxLevel = $dataList->data->max_level ?? 0;
            $pageCacheData['summaryData'] = $dataList->data->summary ?? [];
            $pageCacheData['pageData'] = $dataList->data->data ?? [];
            $pageCacheData['totalPage'] = $dataList->data->pagination->pages ?? PublicEnum::DefaultPage;
            $pageCacheData['totalRow'] = $dataList->data->pagination->total ?? 0;
            session()->put('nowPage', $dataList->data->pagination->current ?? PublicEnum::DefaultPage);
            session()->put('pageCacheData', $pageCacheData);
        } else {
            session()->flash('pagePopMessage', collect($dataList->message ?? [])->implode('; '));
        }
    }
}
