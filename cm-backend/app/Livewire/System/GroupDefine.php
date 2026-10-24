<?php

namespace App\Livewire\System;

use App\Enums\PublicEnum;
use App\Livewire\Common;
use Livewire\Component;
use App\Http\Controllers\AgentGroup\AgentGroupController;
use App\Http\Controllers\AgentGroup\GroupDefineController;
use App\Http\Controllers\Language\LanguageController;

class GroupDefine extends Component
{
    //語言
    public $popDefineEnId = PublicEnum::Language['en']['id'], $popDefineCnId = PublicEnum::Language['zh-cn']['id'];
    //搜尋
    public $groupType;
    //新增 主定義
    public $addMainDefineGroupType, $addMainDefineId, $addMainDefineEnName, $addMainDefineCnName, $addMainDefineRemark;
    //主定義 POP/修改
    public $popMainGroupType, $popMainDefineId;
    public $modifyMainDefineEnName, $modifyMainDefineCnName, $modifyMainDefineRemark;
    //子定義
    public $popSubGroupType, $popMainDefineName;
    //新增 子定義
    public $addSubDefineParentId, $addSubDefineId, $addSubDefineEnName, $addSubDefineCnName, $addSubDefineDataType, $addSubDefineDefaultValue, $addSubDefineMemo;
    //子定義 POP/修改
    public $popSubDefineParentId, $popSubDefineDataType;
    public $modifySubDefineId, $modifySubDefineEnName, $modifySubDefineCnName, $modifySubDefineDefaultValue, $modifySubDefineMemo;

    public function mount()
    {
        $this->getDropdown();
        $this->getLanguageDropdown();
        $this->getDataTypeDropDown();
    }

    public function render()
    {
        return view('livewire.system.group-define');
    }

    public function changeGroupType()
    {
        $this->addMainDefineGroupType = $this->groupType;
    }

    //分頁切換
    public function changePage($oldPage, $newPage)
    {
        if ($oldPage != $newPage) {
            if (1 > $newPage) $newPage = 1;
            if ($newPage > session()->get('pageCacheData')['totalPage']) $newPage = session()->get('pageCacheData')['totalPage'];
            session()->put('nowPage', ($newPage ?? PublicEnum::DefaultPage));
        }
    }

    //搜尋
    public function doSearch(): void
    {
        session()->put('nowPage', PublicEnum::DefaultPage);
        $searchParams = $this->getSearchParams();
        $this->getDataList($searchParams);
    }

    //新增主定義
    public function addMainDefine()
    {
        $language_text = [
            [
                'id' => $this->popDefineEnId,
                'txt' => $this->addMainDefineEnName,
            ], [
                'id' => $this->popDefineCnId,
                'txt' => $this->addMainDefineCnName,
            ]
        ];
        //頁面傳參
        $addParams = [
            'define_id' => $this->addMainDefineId,
            'parent_define_id' => 0,
            'group_type' => $this->groupType,
        ];
        $addParams['language_text'] = json_encode($language_text, JSON_UNESCAPED_UNICODE);
        if (!empty($this->addMainDefineRemark ?? '')) {
            $addParams['memo'] = $this->addMainDefineRemark;
        }
        //串接request
        $common = new Common();
        $requests = $common->getRequest($addParams);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        //新增 接口
        $groupDefineController = new GroupDefineController();
        $addMainDefine = $groupDefineController->create($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $addMainDefine->code) {
            redirect()->to('/groupDefine')->with('pagePopMessage', __('web_message.general.add') . ' ' . __('web_message.groupMgmt.mainDefine') . ' ' . __('web_message.general.success'));
        } else {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', collect($addMainDefine->message ?? [])->implode('; '));
            //恢復原本POP
            session()->flash('popId', 'addMainDefine');
        }
    }

    //主定義POP/修改
    public function showMainPop($defineId = '', $action = '')
    {
        if (!empty($defineId) && !empty($action)) {
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $pageCacheData['popAction'] = $action;
            session()->put('pageCacheData', $pageCacheData);
            //抓取資訊
            $common = new Common();
            $requests = $common->getRequest(['define_id' => $defineId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            $groupDefineController = new GroupDefineController();
            $defineData = $groupDefineController->view($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $defineData->code) {
                $this->popMainGroupType = $defineData->data->data->group_type;
                $this->popMainDefineId = $defineData->data->data->define_id;
                $languageList = $this->getLanguageNameList($defineData->data->data->language_text ?? '');
                $this->modifyMainDefineEnName = $languageList[$this->popDefineEnId] ?? '';
                $this->modifyMainDefineCnName = $languageList[$this->popDefineCnId] ?? '';
                $this->modifyMainDefineRemark = $defineData->data->data->memo ?? '';
                //展示POP
                session()->flash('showPopId', 'popMainDefine');
            }
        }
    }

    //修改主定義
    public function doModifyMainDefine()
    {
        if (!empty($this->popMainDefineId)) {
            //抓取資訊
            $common = new Common();
            $requests = $common->getRequest(['define_id' => $this->popMainDefineId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            $groupDefineController = new GroupDefineController();
            $defineData = $groupDefineController->view($requests['request'])->getData();
            //修改request
            $modifyParams = [];
            $languageList = $this->getLanguageNameList($defineData->data->data->language_text ?? '');
            $mainDefineEnName = $languageList[$this->popDefineEnId] ?? '';
            $mainDefineCnName = $languageList[$this->popDefineCnId] ?? '';
            if (!empty($this->modifyMainDefineEnName ?? '')) {
                if ($this->modifyMainDefineEnName != $mainDefineEnName) {
                    $modifyEnName = 1;
                    $mainDefineEnName = $this->modifyMainDefineEnName;
                }
            }
            if (!empty($this->modifyMainDefineCnName ?? '')) {
                if ($this->modifyMainDefineCnName != $mainDefineCnName) {
                    $modifyCnName = 1;
                    $mainDefineCnName = $this->modifyMainDefineCnName;
                }
            }
            $language_text = [
                [
                    'id' => $this->popDefineEnId,
                    'txt' => $mainDefineEnName,
                ], [
                    'id' => $this->popDefineCnId,
                    'txt' => $mainDefineCnName,
                ]
            ];
            if ($this->modifyMainDefineRemark != $defineData->data->data->memo ?? '') {
                $modifyParams['memo'] = $this->modifyMainDefineRemark;
            }
            //無異動
            if ((0 == ($modifyEnName ?? 0)) && (0 == ($modifyCnName ?? 0)) && empty($modifyParams)) {
                //顯示錯誤訊息POP
                session()->flash('pagePopMessage', __('web_message.general.noChange'));
                //恢復原本POP
                session()->flash('popId', 'popMainDefine');
            } else {
                $modifyParams = array_merge(
                    [
                        'define_id' => $this->popMainDefineId,
                        'language_text' => json_encode($language_text, JSON_UNESCAPED_UNICODE)
                    ], $modifyParams);
                $updateRequests = $common->getRequest($modifyParams);
                if (PublicEnum::ResponseCode['needLogin'] == $updateRequests['code']) {
                    $common->doLogout();
                }
                //修改接口
                $updateResult = $groupDefineController->update($updateRequests['request'])->getData();
                if (PublicEnum::ResponseCode['success'] == $updateResult->code) {
                    $nowPage = session()->get('nowPage') ?? PublicEnum::DefaultPage;
                    $searchParams = $this->getSearchParams();
                    $this->getDataList($searchParams);
                    session()->put('nowPage', $nowPage);
                    session()->flash('pagePopMessage', __('web_message.general.modify') . ' ' . __('web_message.groupMgmt.mainDefine') . ' ' . __('web_message.general.success'));
                } else {
                    //顯示錯誤訊息POP
                    session()->flash('pagePopMessage', collect($updateResult->message ?? [])->implode('; '));
                    //恢復原本POP
                    session()->flash('popId', 'popMainDefine');
                }
            }
        }
    }

    //新增子定義POP
    public function showAddSub($mainGroupType, $mainId, $mainName)
    {
        if (!empty($mainId ?? 0)) {
            $this->popSubGroupType = $mainGroupType;
            $this->addSubDefineParentId = $mainId;
            $this->popMainDefineName = $mainName ?? '';
            $this->addSubDefineId = '';
            $this->addSubDefineEnName = '';
            $this->addSubDefineCnName = '';
            $this->addSubDefineDataType = -1;
            $this->addSubDefineDefaultValue = '';
            $this->addSubDefineMemo = '';

            //展示POP
            session()->flash('showPopId', 'addSubDefine');
        }
    }

    //新增子定義
    public function addSubDefine()
    {
        $language_text = [
            [
                'id' => $this->popDefineEnId,
                'txt' => $this->addSubDefineEnName,
            ], [
                'id' => $this->popDefineCnId,
                'txt' => $this->addSubDefineCnName,
            ]
        ];
        //頁面傳參
        $addParams = [
            'define_id' => $this->addSubDefineId,
            'parent_define_id' => $this->addSubDefineParentId,
            'group_type' => $this->popSubGroupType,
            'data_type' => $this->addSubDefineDataType,
            'default_value' => $this->addSubDefineDefaultValue,
        ];
        $addParams['language_text'] = json_encode($language_text, JSON_UNESCAPED_UNICODE);
        if (!empty($this->addSubDefineMemo ?? '')) {
            $addParams['memo'] = $this->addSubDefineMemo;
        }
        //串接request
        $common = new Common();
        $requests = $common->getRequest($addParams);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        //新增 接口
        $groupDefineController = new GroupDefineController();
        $addSubDefine = $groupDefineController->create($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $addSubDefine->code) {
            $nowPage = session()->get('nowPage') ?? PublicEnum::DefaultPage;
            $searchParams = $this->getSearchParams();
            $this->getDataList($searchParams);
            session()->put('nowPage', $nowPage);
            session()->flash('pagePopMessage', __('web_message.general.add') . ' ' . __('web_message.groupMgmt.subDefine') . ' ' . __('web_message.general.success'));
        } else {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', collect($addSubDefine->message ?? [])->implode('; '));
            //恢復原本POP
            session()->flash('popId', 'addSubDefine');
        }
    }

    //子定義POP/修改
    public function showSubPop($defineId = '', $action = '')
    {
        if (!empty($defineId) && !empty($action)) {
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $pageCacheData['popAction'] = $action;
            session()->put('pageCacheData', $pageCacheData);
            //抓取資訊
            $common = new Common();
            $requests = $common->getRequest(['define_id' => $defineId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            $groupDefineController = new GroupDefineController();
            $defineData = $groupDefineController->view($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $defineData->code) {
                $this->popSubGroupType = $defineData->data->data->group_type;
                $this->popSubDefineParentId = $defineData->data->data->parent_id;
                $nowLanguage = session()->get('language') ?? config('app.locale');
                $languageListParent = $this->getLanguageNameList($defineData->data->data->parent_language_text ?? '');
                $this->popMainDefineName = $languageListParent[PublicEnum::Language[$nowLanguage]['id']];
                $this->modifySubDefineId = $defineData->data->data->define_id;
                $languageList = $this->getLanguageNameList($defineData->data->data->language_text ?? '');
                $this->modifySubDefineEnName = $languageList[$this->popDefineEnId] ?? '';
                $this->modifySubDefineCnName = $languageList[$this->popDefineCnId] ?? '';
                $this->popSubDefineDataType = $defineData->data->data->data_type;
                $this->modifySubDefineDefaultValue = $defineData->data->data->default_value ?? '';
                $this->modifySubDefineMemo = $defineData->data->data->memo ?? '';
                //展示POP
                session()->flash('showPopId', 'popSubDefine');
            }
        }
    }

    //修改子定義
    public function doModifySubDefine()
    {
        if (!empty($this->modifySubDefineId)) {
            //抓取資訊
            $common = new Common();
            $requests = $common->getRequest(['define_id' => $this->modifySubDefineId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            $groupDefineController = new GroupDefineController();
            $defineData = $groupDefineController->view($requests['request'])->getData();
            //修改request
            $modifyParams = [];
            $languageList = $this->getLanguageNameList($defineData->data->data->language_text ?? '');
            $subDefineEnName = $languageList[$this->popDefineEnId] ?? '';
            $subDefineCnName = $languageList[$this->popDefineCnId] ?? '';
            if (!empty($this->modifySubDefineEnName ?? '')) {
                if ($this->modifySubDefineEnName != $subDefineEnName) {
                    $modifyEnName = 1;
                    $subDefineEnName = $this->modifySubDefineEnName;
                }
            }
            if (!empty($this->modifySubDefineCnName ?? '')) {
                if ($this->modifySubDefineCnName != $subDefineCnName) {
                    $modifyCnName = 1;
                    $subDefineCnName = $this->modifySubDefineCnName;
                }
            }
            $language_text = [
                [
                    'id' => $this->popDefineEnId,
                    'txt' => $subDefineEnName,
                ], [
                    'id' => $this->popDefineCnId,
                    'txt' => $subDefineCnName,
                ]
            ];
            if ($this->modifySubDefineDefaultValue != $defineData->data->data->default_value ?? '') {
                $modifyParams['default_value'] = $this->modifySubDefineDefaultValue;
            }
            if ($this->modifySubDefineMemo != $defineData->data->data->memo ?? '') {
                $modifyParams['memo'] = $this->modifySubDefineMemo;
            }
            //無異動
            if ((0 == ($modifyEnName ?? 0)) && (0 == ($modifyCnName ?? 0)) && empty($modifyParams)) {
                //顯示錯誤訊息POP
                session()->flash('pagePopMessage', __('web_message.general.noChange'));
                //恢復原本POP
                session()->flash('popId', 'popSubDefine');
            } else {
                $modifyParams = array_merge(
                    [
                        'define_id' => $this->modifySubDefineId,
                        'language_text' => json_encode($language_text, JSON_UNESCAPED_UNICODE)
                    ], $modifyParams);
                $updateRequests = $common->getRequest($modifyParams);
                if (PublicEnum::ResponseCode['needLogin'] == $updateRequests['code']) {
                    $common->doLogout();
                }
                //修改接口
                $updateResult = $groupDefineController->update($updateRequests['request'])->getData();
                if (PublicEnum::ResponseCode['success'] == $updateResult->code) {
                    $nowPage = session()->get('nowPage') ?? PublicEnum::DefaultPage;
                    $searchParams = $this->getSearchParams();
                    $this->getDataList($searchParams);
                    session()->put('nowPage', $nowPage);
                    session()->flash('pagePopMessage', __('web_message.general.modify') . ' ' . __('web_message.groupMgmt.subDefine') . ' ' . __('web_message.general.success'));
                } else {
                    //顯示錯誤訊息POP
                    session()->flash('pagePopMessage', collect($updateResult->message ?? [])->implode('; '));
                    //恢復原本POP
                    session()->flash('popId', 'popSubDefine');
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
        $groupResult = $agentGroupController->getTypeDropdown($dropDownRequests['request'])->getData();
        //塞進session
        $pageCacheData = session()->get('pageCacheData') ?? [];
        $pageCacheData['groupDropdown'] = $groupResult->data->list ?? [];
        session()->put('pageCacheData', $pageCacheData);
    }

    private function getLanguageDropdown(): void
    {
        $pageCacheData = session()->get('pageCacheData') ?? [];
        if (empty($pageCacheData['languageDropdown'] ?? [])) {
            //接口
            $common = new Common();
            $dropDownRequests = $common->getRequest([]);
            if (PublicEnum::ResponseCode['needLogin'] == $dropDownRequests['code']) {
                $common->doLogout();
            }
            $languageController = new LanguageController();
            $languageResult = $languageController->getDropdown($dropDownRequests['request'])->getData();
            //塞進session
            $pageCacheData['languageDropdown'] = $languageResult->data->list ?? [];
            session()->put('pageCacheData', $pageCacheData);
        }
    }

    private function getDataTypeDropdown(): void
    {
        $pageCacheData = session()->get('pageCacheData') ?? [];
        if (empty($pageCacheData['dataTypeDropdown'] ?? [])) {
            //接口
            $common = new Common();
            $dropDownRequests = $common->getRequest([]);
            if (PublicEnum::ResponseCode['needLogin'] == $dropDownRequests['code']) {
                $common->doLogout();
            }
            $groupDefineController = new GroupDefineController();
            $dataTypeResult = $groupDefineController->getDataTypeDropdown($dropDownRequests['request'])->getData();
            //塞進session
            $pageCacheData['dataTypeDropdown'] = $dataTypeResult->data->list ?? [];
            session()->put('pageCacheData', $pageCacheData);
        }
    }

    //搜尋參數串接
    private function getSearchParams(): array
    {
        $searchParams = [];
        if (-1 != ($this->groupType ?? -1)) {
            $searchParams['group_type'] = $this->groupType;
        }
        return $searchParams;
    }

    private function getDataList($params)
    {
        if (!empty($params ?? [])) {
            $common = new Common();
            $requests = $common->getRequest($params);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            $groupDefineController = new GroupDefineController();
            $defineList = $groupDefineController->getList($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $defineList->code) {
                $pageCacheData = session()->get('pageCacheData') ?? [];
                $pageCacheData['pageData'] = $defineList->data->list ?? [];
                $pageCacheData['totalPage'] = $defineList->data->pagination->pages ?? PublicEnum::DefaultPage;
                $pageCacheData['totalRow'] = $defineList->data->pagination->total ?? 0;
                session()->put('nowPage', $defineList->data->pagination->current ?? PublicEnum::DefaultPage);
                session()->put('pageCacheData', $pageCacheData);
            } else {
                session()->flash('pagePopMessage', collect($defineList->message ?? [])->implode('; '));
            }
        }
    }

    private function getLanguageNameList($languageJson)
    {
        $result = [];
        $languageList = json_decode($languageJson ?? '', true);
        foreach ($languageList ?? [] as $language) {
            if ($language['id'] == $this->popDefineEnId) {
                $result[$this->popDefineEnId] = $language['txt'] ?? '';
            }
            if ($language['id'] == $this->popDefineCnId) {
                $result[$this->popDefineCnId] = $language['txt'] ?? '';
            }
        }
        return $result;
    }
}
