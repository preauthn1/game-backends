<?php

namespace App\Livewire\Manager;

use App\Enums\PublicEnum;
use App\Enums\TimeEnum as TimeEnum;
use App\Livewire\Common;
use Illuminate\Support\Carbon;
use Livewire\Component;
use App\Http\Controllers\Agent\AgentOperateLogController;

class OperationLog extends Component
{
    //搜尋
    public $managerName, $operationApiPath, $operationApiName, $operationStatus, $timeRange;
    //POP
    public $popId, $popStatusTxt, $popManagerName, $popApiPath, $popApiName, $popRequest, $popResponse, $popCostSec, $popOperationTime;

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
        $now = Carbon::now()->setTimezone(TimeEnum::TimeZone['backend']);
        $this->timeRange = $now->startOfDay()->format('Y-m-d H:i') . ' ~ ' . $now->endOfDay()->format('Y-m-d H:i');
        $searchParams = $this->getSearchParams();
        $this->getDataList($searchParams);
    }

    public function render()
    {
        return view('livewire.manager.operation-log');
    }

    public function setTimeRange($option)
    {
        $common = new Common();
        $this->timeRange = $common->fastTimeRange($option);
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

    //修改/詳細 POP
    public function showDataPop($id = '', $action = '')
    {
        if (!empty($id) && !empty($action)) {
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $pageCacheData['popAction'] = $action;
            session()->put('pageCacheData', $pageCacheData);
            //串接request
            $common = new Common();
            $requests = $common->getRequest(['id' => $id]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            //資訊
            $agentOperateLogController = new AgentOperateLogController();
            $dataList = $agentOperateLogController->view($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $dataList->code) {
                $this->popId = $dataList->data->data->id ?? '';
                $this->popStatusTxt = $dataList->data->data->statusTxt ?? '';
                $this->popManagerName = $dataList->data->data->username ?? '';
                $this->popApiPath = $dataList->data->data->api_path ?? '';
                $this->popApiName = $dataList->data->data->api_name ?? '';
                $this->popRequest = $dataList->data->data->request ?? '';
                $this->popResponse = $dataList->data->data->response_data ?? '';
                $this->popCostSec = $dataList->data->data->time_cost ?? 0;
                $this->popOperationTime = $dataList->data->data->created_at ?? '';
                //展示POP
                session()->flash('showPopId', 'dataPop');
            }
        }
    }

    //搜尋參數串接
    private function getSearchParams(): array
    {
        $searchParams = [];
        if (!empty($this->managerName ?? '')) {
            $searchParams['manager_name'] = $this->managerName;
        }
        if (!empty($this->operationApiPath ?? '')) {
            $searchParams['api_path'] = $this->operationApiPath;
        }
        if (!empty($this->operationApiName ?? '')) {
            $searchParams['api_name'] = $this->operationApiName;
        }
        if (-1 != ($this->operationStatus ?? -1)) {
            $searchParams['status'] = $this->operationStatus;
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

    private function getDataList($params): void
    {
        $common = new Common();
        $requests = $common->getRequest($params);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $agentOperateLogController = new AgentOperateLogController();
        $dataList = $agentOperateLogController->getList($requests['request'])->getData();
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
