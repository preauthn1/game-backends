<?php

namespace App\Livewire\System;

use App\Enums\PublicEnum;
use App\Livewire\Common;
use Livewire\Component;
use App\Http\Controllers\Task\TaskController;

class TaskScheduling extends Component
{
    //搜尋
    public $taskCommand, $taskStatus;
    //新增
    public $addTaskCommand, $addTaskDescription, $addTaskExpression = '* * * * *', $addTaskStatus, $addTaskInfo;
    //POP
    public $popTaskId, $popTaskCommand;
    //修改
    public $modifyTaskDescription, $modifyTaskExpression, $modifyTaskStatus, $modifyTaskInfo;

    public function mount()
    {
        $this->getDropdown();
        $searchParams = $this->getSearchParams();
        $this->getDataList($searchParams);
    }

    public function render()
    {
        return view('livewire.system.task-scheduling');
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
    public function addTask()
    {
        //頁面傳參
        $addParams = [
            'command' => $this->addTaskCommand,
            'expression' => $this->addTaskExpression,
            'status' => $this->addTaskStatus ? 1 : 0,
        ];
        if (!empty($this->addTaskDescription ?? '')) {
            $addParams['description'] = $this->addTaskDescription;
        }
        if (!empty($this->addTaskInfo ?? '')) {
            $addParams['info'] = $this->addTaskInfo;
        }
        //串接request
        $common = new Common();
        $requests = $common->getRequest($addParams);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        //新增接口
        $taskController = new TaskController();
        $addTask = $taskController->create($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $addTask->code) {
            redirect()->to('/taskScheduling')->with('pagePopMessage', __('web_message.menu.system.addTask') . ' ' . __('web_message.general.success'));
        } else {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', collect($addTask->message ?? [])->implode('; '));
            //恢復原本POP
            session()->flash('popId', 'addTask');
        }
    }

    //修改POP
    public function showModify($taskId)
    {
        if (!empty($taskId)) {
            //串接request
            $common = new Common();
            $requests = $common->getRequest(['id' => $taskId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            //資訊
            $taskController = new TaskController();
            $taskData = $taskController->view($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $taskData->code) {
                $this->popTaskId = $taskData->data->data->id;
                $this->popTaskCommand = $taskData->data->data->command ?? '';
                $this->modifyTaskDescription = $taskData->data->data->description ?? '';
                $this->modifyTaskExpression = $taskData->data->data->expression ?? '';
                $this->modifyTaskStatus = (1 == ($taskData->data->data->status ?? 1));
                $this->modifyTaskInfo = $taskData->data->data->info ?? '';
                //展示POP
                session()->flash('showPopId', 'modifyTask');
            }
        }
    }

    //修改
    public function doModify()
    {
        if (!empty($this->popTaskId)) {
            //抓取資訊
            $common = new Common();
            $requests = $common->getRequest(['id' => $this->popTaskId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            $taskController = new TaskController();
            $taskData = $taskController->view($requests['request'])->getData();
            //串接修改request
            $modifyParams = [];
            if ($this->modifyTaskDescription != ($taskData->data->data->description ?? '')) {
                $modifyParams['description'] = $this->modifyTaskDescription;
            }
            if ($this->modifyTaskExpression != ($taskData->data->data->expression ?? '')) {
                $modifyParams['expression'] = $this->modifyTaskExpression;
            }
            if ($this->modifyTaskStatus != (1 == ($taskData->data->data->status ?? 1))) {
                $modifyParams['status'] = $this->modifyTaskStatus ? 1 : 0;
            }
            if ($this->modifyTaskInfo != ($taskData->data->data->info ?? '')) {
                $modifyParams['info'] = $this->modifyTaskInfo;
            }
            //無異動
            if (empty($modifyParams)) {
                //顯示錯誤訊息POP
                session()->flash('pagePopMessage', __('web_message.general.noChange'));
                //恢復原本POP
                session()->flash('popId', 'modifyTask');
            } else {
                $modifyParams = array_merge(['id' => $this->popTaskId], $modifyParams);
                $updateRequests = $common->getRequest($modifyParams);
                if (PublicEnum::ResponseCode['needLogin'] == $updateRequests['code']) {
                    $common->doLogout();
                }
                //修改接口
                $updateTask = $taskController->update($updateRequests['request'])->getData();
                if (PublicEnum::ResponseCode['success'] == $updateTask->code) {
                    $searchParams = $this->getSearchParams();
                    $this->getDataList($searchParams);
                    session()->flash('pagePopMessage', __('web_message.menu.system.modifyTask') . ' ' . __('web_message.general.success'));
                } else {
                    //顯示錯誤訊息POP
                    session()->flash('pagePopMessage', collect($updateTask->message ?? [])->implode('; '));
                    //恢復原本POP
                    session()->flash('popId', 'modifyTask');
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
        $taskController = new TaskController();
        //狀態
        $statusList = $taskController->getStatusDropdown($dropDownRequests['request'])->getData();
        //塞進session
        $pageCacheData = session()->get('pageCacheData') ?? [];
        $pageCacheData['statusList'] = $statusList->data->list ?? [];
        session()->put('pageCacheData', $pageCacheData);
    }

    //搜尋參數串接
    private function getSearchParams(): array
    {
        $searchParams = [];
        if (!empty($this->command ?? '')) {
            $searchParams['command'] = $this->taskCommand;
        }
        if (-1 != ($this->taskStatus ?? -1)) {
            $searchParams['status'] = $this->taskStatus;
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
        $taskController = new TaskController();
        $taskList = $taskController->getList($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $taskList->code) {
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $pageCacheData['pageData'] = $taskList->data->list ?? [];
            $pageCacheData['totalPage'] = $taskList->data->pagination->pages ?? PublicEnum::DefaultPage;
            $pageCacheData['totalRow'] = $taskList->data->pagination->total ?? 0;
            session()->put('nowPage', $taskList->data->pagination->current ?? PublicEnum::DefaultPage);
            session()->put('pageCacheData', $pageCacheData);
        } else {
            session()->flash('pagePopMessage', collect($taskList->message ?? [])->implode('; '));
        }
    }
}
