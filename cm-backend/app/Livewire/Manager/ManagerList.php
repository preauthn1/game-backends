<?php

namespace App\Livewire\Manager;

use App\Enums\AgentGroupEnum;
use App\Enums\PublicEnum;
use App\Livewire\Common;
use Livewire\Component;
use App\Http\Controllers\Agent\AgentController;
use App\Http\Controllers\Agent\MainController;

class ManagerList extends Component
{
    //搜尋
    public $managerId, $managerOrganizationId, $parentName, $managerName, $managerRoleId, $managerStatus;
    //新增
    public $addManagerUsername, $addManagerPassword, $addManagerAgentName, $addManagerReferralCode, $addManagerRoleId, $addManagerSubAccount = true;
    //POP
    public $popManagerName, $popOrganizationId, $popCreatedAt;
    //修改
    public $modifyManagerId, $modifyRoleId, $modifyManagerPwd, $modifyCustomizedId, $modifyCashFlowId, $modifySmsGatewayId, $modifyMinerId, $modifyManagerStatus;

    public function mount()
    {
        $this->getDropdown();
        $searchParams = $this->getSearchParams();
        $this->getDataList($searchParams);
    }

    public function render()
    {
        return view('livewire.manager.manager-list');
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

    //新增
    public function addManager()
    {
        //頁面傳參
        $addParams = [
            'username' => $this->addManagerUsername,
            'password' => $this->addManagerPassword,
            'role_id' => $this->addManagerRoleId,
            'sub_account' => $this->addManagerSubAccount ? 1 : 0
        ];
        if (!empty($this->addManagerAgentName ?? '')) {
            $addParams['parent_username'] = $this->addManagerAgentName;
        }
        if (!empty($this->addManagerReferralCode ?? '')) {
            $addParams['referral_code'] = $this->addManagerReferralCode;
        }
        //串接request
        $common = new Common();
        $requests = $common->getRequest($addParams);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        //新增接口
        $agentController = new AgentController();
        $addMember = $agentController->create($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $addMember->code) {
            redirect()->to('/managerList')->with('pagePopMessage', __('web_message.menu.manager.addManager') . ' ' . __('web_message.general.success'));
        } else {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', collect($addMember->message ?? [])->implode('; '));
            //恢復原本POP
            session()->flash('popId', 'addManager');
        }
    }

    //修改/詳細 POP
    public function showDataPop($managerId = '', $action = '')
    {
        if (!empty($managerId) && !empty($action)) {
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $pageCacheData['popAction'] = $action;
            session()->put('pageCacheData', $pageCacheData);
            //串接request
            $common = new Common();
            $requests = $common->getRequest(['manager_id' => $managerId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            //資訊
            $agentController = new AgentController();
            $dataList = $agentController->view($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $dataList->code) {
                $this->modifyManagerId = $dataList->data->data->id ?? '';
                $this->popManagerName = $dataList->data->data->username ?? '';
                $this->popOrganizationId = $dataList->data->data->organization_id ?? '';
                $this->modifyRoleId = $dataList->data->data->role_id ?? '';
                $this->modifyCustomizedId = $dataList->data->data->customized_setting_group_id ?? '';
                $this->modifyCashFlowId = $dataList->data->data->cash_flow_group_id ?? '';
                $this->modifySmsGatewayId = $dataList->data->data->sms_gateway_group_id ?? '';
                $this->modifyMinerId = $dataList->data->data->miner_group_id ?? '';
                $this->popCreatedAt = $dataList->data->data->created_at ?? '';
                $this->modifyManagerStatus = (1 == ($dataList->data->data->status ?? 1));
                //展示POP
                session()->flash('showPopId', 'dataPop');
            }
        }
    }

    //修改
    public function doModify()
    {
        if (empty($this->modifyManagerId)) {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', __('error_message.9001'));
            //恢復原本POP
            session()->flash('popId', 'dataPop');
            return;
        }
        //抓取資訊
        $common = new Common();
        $requests = $common->getRequest(['manager_id' => $this->modifyManagerId]);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $agentController = new AgentController();
        $dataList = $agentController->view($requests['request'])->getData();
        //串接修改request
        $modifyParams = [];
        //修改
        if (-1 != ($this->modifyRoleId ?? -1)) {
            if ($this->modifyRoleId != ($dataList->data->data->role_id ?? '')) {
                $modifyParams['role_id'] = $this->modifyRoleId;
            }
        }
        if (-1 != ($this->modifyCustomizedId ?? -1)) {
            if ($this->modifyCustomizedId != ($dataList->data->data->customized_setting_group_id ?? '')) {
                $modifyParams['customized_setting_group_id'] = $this->modifyCustomizedId;
            }
        }
        if (-1 != ($this->modifyCashFlowId ?? -1)) {
            if ($this->modifyCashFlowId != ($dataList->data->data->cash_flow_group_id ?? '')) {
                $modifyParams['cash_flow_group_id'] = $this->modifyCashFlowId;
            }
        }
        if (-1 != ($this->modifySmsGatewayId ?? -1)) {
            if ($this->modifySmsGatewayId != ($dataList->data->data->sms_gateway_group_id ?? '')) {
                $modifyParams['sms_gateway_group_id'] = $this->modifySmsGatewayId;
            }
        }
        if (-1 != ($this->modifyMinerId ?? -1)) {
            if ($this->modifyMinerId != ($dataList->data->data->miner_group_id ?? '')) {
                $modifyParams['miner_group_id'] = $this->modifyMinerId;
            }
        }
        if (!empty($this->modifyManagerPwd ?? '')) {
            $modifyParams['password'] = $this->modifyManagerPwd;
        }
        if ($this->modifyManagerStatus != (1 == ($dataList->data->data->status ?? 1))) {
            $modifyParams['status'] = $this->modifyManagerStatus ? 1 : 0;
        }
        //無異動
        if (empty($modifyParams)) {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', __('web_message.general.noChange'));
            //恢復原本POP
            session()->flash('popId', 'dataPop');
        } else {
            $modifyParams = array_merge(['manager_id' => $this->modifyManagerId], $modifyParams);
            $updateRequests = $common->getRequest($modifyParams);
            if (PublicEnum::ResponseCode['needLogin'] == $updateRequests['code']) {
                $common->doLogout();
            }
            //修改接口
            $updateMember = $agentController->update($updateRequests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $updateMember->code) {
                redirect()->to('/managerList')->with('pagePopMessage', __('web_message.menu.manager.modifyManager') . ' ' . __('web_message.general.success'));
            } else {
                //顯示錯誤訊息POP
                session()->flash('pagePopMessage', collect($updateMember->message ?? [])->implode('; '));
                //恢復原本POP
                session()->flash('popId', 'dataPop');
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
        //角色
        $roleResult = $mainController->getRoleDropdown($dropDownRequests['request'])->getData();
        //組織
        $organizationResult = $mainController->getOrganizationDropdown($dropDownRequests['request'])->getData();
        //個性化群組
        $dropDownRequests = $common->getRequest([
            'type' => AgentGroupEnum::TYPE_CUSTOMIZED_SETTING
        ]);
        if (PublicEnum::ResponseCode['needLogin'] == $dropDownRequests['code']) {
            $common->doLogout();
        }
        $customizedResult = $mainController->getAgentGroupDropdown($dropDownRequests['request'])->getData();
        //支付群組
        $dropDownRequests = $common->getRequest([
            'type' => AgentGroupEnum::TYPE_PAYMENT
        ]);
        if (PublicEnum::ResponseCode['needLogin'] == $dropDownRequests['code']) {
            $common->doLogout();
        }
        $cashFlowResult = $mainController->getAgentGroupDropdown($dropDownRequests['request'])->getData();
        //簡訊群組
        $dropDownRequests = $common->getRequest([
            'type' => AgentGroupEnum::TYPE_SMS_GATEWAY
        ]);
        if (PublicEnum::ResponseCode['needLogin'] == $dropDownRequests['code']) {
            $common->doLogout();
        }
        $smsGatewayResult = $mainController->getAgentGroupDropdown($dropDownRequests['request'])->getData();
        //礦機群組
        $dropDownRequests = $common->getRequest([
            'type' => AgentGroupEnum::TYPE_MINER
        ]);
        if (PublicEnum::ResponseCode['needLogin'] == $dropDownRequests['code']) {
            $common->doLogout();
        }
        $minerResult = $mainController->getAgentGroupDropdown($dropDownRequests['request'])->getData();
        //塞進session
        $pageCacheData = session()->get('pageCacheData') ?? [];
        $pageCacheData['roleList'] = $roleResult->data->list ?? [];
        $pageCacheData['organizationList'] = $organizationResult->data->list ?? [];
        $pageCacheData['customizedList'] = $customizedResult->data->list ?? [];
        $pageCacheData['cashFlowList'] = $cashFlowResult->data->list ?? [];
        $pageCacheData['smsGatewayList'] = $smsGatewayResult->data->list ?? [];
        $pageCacheData['minerList'] = $minerResult->data->list ?? [];
        session()->put('pageCacheData', $pageCacheData);
    }

    //搜尋參數串接
    private function getSearchParams(): array
    {
        $searchParams = [];
        if (!empty($this->managerId ?? '')) {
            $searchParams['manager_id'] = $this->managerId;
        }
        if (-1 != ($this->managerOrganizationId ?? -1)) {
            $searchParams['organization_id'] = $this->managerOrganizationId;
        }
        if (!empty($this->parentName ?? '')) {
            $searchParams['parent_name'] = $this->parentName;
        }
        if (!empty($this->managerName ?? '')) {
            $searchParams['manager_name'] = $this->managerName;
        }
        if (-1 != ($this->managerRoleId ?? -1)) {
            $searchParams['role_id'] = $this->managerRoleId;
        }
        if (-1 != ($this->managerStatus ?? -1)) {
            $searchParams['status'] = $this->managerStatus;
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
        $agentController = new AgentController();
        $dataList = $agentController->getList($requests['request'])->getData();
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
