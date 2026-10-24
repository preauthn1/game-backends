<?php

namespace App\Livewire\Manager;

use App\Enums\PublicEnum;
use App\Http\Controllers\AgentGroup\AgentGroupController;
use App\Livewire\Common;
use Livewire\Component;

class GroupMgmt extends Component
{
    //搜尋
    public $groupType, $groupId;
    //新增
    public $addAgentGroupType, $addAgentGroupName, $addAgentGroupMemo;
    //修改
    public $modifySettingId, $popMainId, $popMainName, $popSubId, $popSubName, $popDataType, $popDataTypeTxt, $modifySettingValue, $popMemo;

    public function mount()
    {
        $this->getDropdown();
    }

    public function render()
    {
        return view('livewire.manager.group-mgmt');
    }

    public function changeGroupList()
    {
        $groupResult = session()->get('pageCacheData')['groupDropdown'] ?? [];
        $groupList = $groupResult[$this->groupType]['group'] ?? [];
        $this->groupId = -1;
        $pageCacheData = session()->get('pageCacheData') ?? [];
        $pageCacheData['groupList'] = $groupList;
        session()->put('pageCacheData', $pageCacheData);
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

    //新增組別
    public function addAgentGroup()
    {
        //頁面傳參
        $addParams = [
            'group_type' => $this->addAgentGroupType,
            'name' => $this->addAgentGroupName,
        ];
        if (!empty($this->addAgentGroupMemo ?? '')) {
            $addParams['memo'] = $this->addAgentGroupMemo;
        }
        //串接request
        $common = new Common();
        $requests = $common->getRequest($addParams);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        //新增接口
        $agentGroupController = new AgentGroupController();
        $addAgentGroup = $agentGroupController->create($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $addAgentGroup->code) {
            redirect()->to('/groupMgmt')->with('pagePopMessage', __('web_message.general.add') . ' ' . __('web_message.groupMgmt.group') . ' ' . __('web_message.general.success'));
        } else {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', collect($addAgentGroup->message ?? [])->implode('; '));
            //恢復原本POP
            session()->flash('popId', 'addAgentGroup');
        }
    }

    //修改設定值POP
    public function showModifySetting($settingId)
    {
        if (!empty($settingId ?? '')) {
            //串接request
            $common = new Common();
            $requests = $common->getRequest(['setting_id' => $settingId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            //資訊
            $agentGroupController = new AgentGroupController();
            $settingData = $agentGroupController->view($requests['request'])->getData();
            //dd($settingData);
            if (PublicEnum::ResponseCode['success'] == $settingData->code) {
                $this->modifySettingId = $settingData->data->data->id;
                $this->popMainId = $settingData->data->data->parent_id ?? '';
                $this->popMainName = $settingData->data->data->mainName ?? '';
                $this->popSubId = $settingData->data->data->define_id ?? '';
                $this->popSubName = $settingData->data->data->subName ?? '';
                $this->popDataType = $settingData->data->data->data_type ?? '';
                $this->popDataTypeTxt = $settingData->data->data->dataTypeTxt ?? '';
                $this->modifySettingValue = $settingData->data->data->value;
                $this->popMemo = $settingData->data->data->memo ?? '';
                if ('boolean' == $this->popDataType) {
                    $this->modifySettingValue = (bool)$settingData->data->data->value;
                }
                //展示POP
                session()->flash('showPopId', 'modifySetting');
            }
        }
    }

    //修改設定值
    public function doModifySetting()
    {
        if (!empty($this->modifySettingId)) {
            //抓取資訊
            $common = new Common();
            $requests = $common->getRequest(['setting_id' => $this->modifySettingId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            //資訊
            $agentGroupController = new AgentGroupController();
            $settingData = $agentGroupController->view($requests['request'])->getData();
            //修改request
            $modifyParams = [];
            if ('boolean' == $this->popDataType) {
                $this->modifySettingValue = (string)($this->modifySettingValue ? 1 : 0);
            }
            if ($this->modifySettingValue != ($settingData->data->data->value ?? '')) {
                $modifyParams['value'] = $this->modifySettingValue;
            }
            //無異動
            if (empty($modifyParams)) {
                //顯示錯誤訊息POP
                session()->flash('pagePopMessage', __('web_message.general.noChange'));
                //恢復原本POP
                session()->flash('popId', 'modifySetting');
            } else {
                $modifyParams = array_merge(['setting_id' => $this->modifySettingId], $modifyParams);
                $updateRoleRequests = $common->getRequest($modifyParams);
                if (PublicEnum::ResponseCode['needLogin'] == $updateRoleRequests['code']) {
                    $common->doLogout();
                }
                //修改接口
                $updateResult = $agentGroupController->update($updateRoleRequests['request'])->getData();
                if (PublicEnum::ResponseCode['success'] == $updateResult->code) {
                    $searchParams = $this->getSearchParams();
                    $this->getDataList($searchParams);
                    session()->flash('pagePopMessage', __('web_message.general.content') . ' ' . __('web_message.general.modify') . ' ' . __('web_message.general.success'));
                } else {
                    //顯示錯誤訊息POP
                    session()->flash('pagePopMessage', collect($updateResult->message ?? [])->implode('; '));
                    //恢復原本POP
                    session()->flash('popId', 'modifySetting');
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
        $agentGroupController = new AgentGroupController();
        $groupResult = $agentGroupController->getDropdown($dropDownRequests['request'])->getData();
        foreach ($groupResult->data->list ?? [] as $group) {
            $type = $group->type ?? '';
            if (!empty($type)) {
                if (empty($groupDropdown[$type] ?? '')) {
                    $groupDropdown[$type] = [
                        'id' => $type,
                        'name' => __('param.agent_group.type.' . $type),
                        'group' => [],
                    ];
                }
                $groupId = $group->id ?? '';
                if (!empty($groupId)) {
                    if (empty($groupDropdown[$type]['group'][$groupId] ?? [])) {
                        $groupDropdown[$type]['group'][$groupId] = [
                            'id' => $groupId,
                            'name' => $group->name,
                        ];
                    }
                }
            }
        }
        //塞進session
        $pageCacheData = session()->get('pageCacheData') ?? [];
        $pageCacheData['groupDropdown'] = $groupDropdown ?? [];
        session()->put('pageCacheData', $pageCacheData);
    }

    //搜尋參數串接
    private function getSearchParams(): array
    {
        $searchParams = [];
        if (-1 != ($this->groupType ?? -1)) {
            $searchParams['group_type'] = $this->groupType;
        }
        if (-1 != ($this->groupId ?? -1)) {
            $searchParams['group_id'] = $this->groupId;
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
        $agentGroupController = new AgentGroupController();
        $settingList = $agentGroupController->getList($requests['request'])->getData();
        //dd($settingList);
        if (PublicEnum::ResponseCode['success'] == $settingList->code) {
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $pageCacheData['pageData'] = $settingList->data->list ?? [];
            $pageCacheData['totalPage'] = $settingList->data->pagination->pages ?? PublicEnum::DefaultPage;
            $pageCacheData['totalRow'] = $settingList->data->pagination->total ?? 0;
            session()->put('nowPage', $settingList->data->pagination->current ?? PublicEnum::DefaultPage);
            session()->put('pageCacheData', $pageCacheData);
        } else {
            session()->flash('pagePopMessage', collect($settingList->message ?? [])->implode('; '));
        }
    }
}
