<?php

namespace App\Livewire\Manager;

use App\Enums\PublicEnum;
use App\Http\Controllers\Admin\MenuController;
use App\Http\Controllers\Admin\RoleController;
use App\Http\Controllers\Admin\RoleMenuController;
use App\Livewire\Common;
use Livewire\Component;

class RoleMgmt extends Component
{
    //新增
    public $addRoleName;
    //修改
    public $modifyRoleId, $modifyRoleName;
    public $modifyRoleMenusId, $modifyRoleMenusName, $modifyRoleMenus = [];

    public function mount()
    {
        $searchParams = $this->getSearchParams();
        $this->getDataList($searchParams);
    }

    public function render()
    {
        return view('livewire.manager.role-mgmt');
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

    //新增Role
    public function addRole()
    {
        //頁面傳參
        $addParams = [
            'role_name' => $this->addRoleName,
        ];
        //串接request
        $common = new Common();
        $requests = $common->getRequest($addParams);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        //新增角色 接口
        $roleController = new RoleController();
        $addRole = $roleController->create($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $addRole->code) {
            redirect()->to('/roleMgmt')->with('pagePopMessage', __('web_message.menu.manager.addRole') . ' ' . __('web_message.general.success'));
        } else {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', collect($addRole->message ?? [])->implode('; '));
            //恢復原本POP
            session()->flash('popId', 'addRole');
        }
    }

    //修改Role Pop
    public function showModifyRole($roleId)
    {
        if (!empty($roleId)) {
            //串接request
            $common = new Common();
            $requests = $common->getRequest(['role_id' => $roleId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            //資訊
            $roleController = new RoleController();
            $roleData = $roleController->view($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $roleData->code) {
                $this->modifyRoleId = $roleData->data->data->id;
                $this->modifyRoleName = $roleData->data->data->name ?? '';
                //展示POP
                session()->flash('showPopId', 'modifyRole');
            }
        }
    }

    //修改Role
    public function doModifyRole()
    {
        if (!empty($this->modifyRoleId)) {
            //抓取資訊
            $common = new Common();
            $requests = $common->getRequest(['role_id' => $this->modifyRoleId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            //角色
            $roleController = new RoleController();
            $roleData = $roleController->view($requests['request'])->getData();
            //修改request
            $roleModifyParams = [];
            if ($this->modifyRoleName != ($roleData->data->data->name ?? '')) {
                $roleModifyParams['role_name'] = $this->modifyRoleName;
            }
            //無異動
            if (empty($roleModifyParams)) {
                //顯示錯誤訊息POP
                session()->flash('pagePopMessage', __('web_message.general.noChange'));
                //恢復原本POP
                session()->flash('popId', 'modifyRole');
            } else {
                $roleModifyParams = array_merge(['role_id' => $this->modifyRoleId], $roleModifyParams);
                $updateRoleRequests = $common->getRequest($roleModifyParams);
                if (PublicEnum::ResponseCode['needLogin'] == $updateRoleRequests['code']) {
                    $common->doLogout();
                }
                //修改接口
                $updateResult = $roleController->update($updateRoleRequests['request'])->getData();
                if (PublicEnum::ResponseCode['success'] == $updateResult->code) {
                    $searchParams = $this->getSearchParams();
                    $this->getDataList($searchParams);
                    session()->flash('pagePopMessage', __('web_message.menu.manager.modifyRole') . ' ' . __('web_message.general.success'));
                } else {
                    //顯示錯誤訊息POP
                    session()->flash('pagePopMessage', collect($updateResult->message ?? [])->implode('; '));
                    //恢復原本POP
                    session()->flash('popId', 'modifyRole');
                }
            }
        }
    }

    //修改權限 Pop
    public function showModifyRoleMenus($roleId)
    {
        if (!empty($roleId)) {
            //串接request
            $common = new Common();
            $requests = $common->getRequest(['role_id' => $roleId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            $roleController = new RoleController();
            $roleData = $roleController->view($requests['request'])->getData();
            $this->modifyRoleMenusId = $roleId;
            $this->modifyRoleMenusName = $roleData->data->data->name ?? '';
            //選單列表
            $this->getMenuGroupList($roleId);
            //權限
            $this->modifyRoleMenus = $this->getRoleMenuIds($roleId);
            //展示POP
            session()->flash('showPopId', 'modifyRoleMenus');
        }
    }

    //修改權限
    public function doModifyRoleMenus()
    {
        if (!empty($this->modifyRoleMenusId)) {
            //權限
            $accessList = $this->getRoleMenuIds($this->modifyRoleMenusId);
            //修改request
            $roleMenusAddIds = collect($this->modifyRoleMenus)->diff($accessList)->all() ?? [];
            $roleMenusSubIds = collect($accessList)->diff($this->modifyRoleMenus)->all() ?? [];
            //無異動
            if (empty($roleMenusAddIds) && empty($roleMenusSubIds)) {
                //顯示錯誤訊息POP
                session()->flash('pagePopMessage', __('web_message.general.noChange'));
                //恢復原本POP
                session()->flash('popId', 'modifyRoleMenus');
            } else {
                $modifyParams = [
                    'role_id' => $this->modifyRoleMenusId,
                    'add_menu_ids' => implode(';', $roleMenusAddIds),
                    'sub_menu_ids' => implode(';', $roleMenusSubIds),
                ];
                $common = new Common();
                $updateRequests = $common->getRequest($modifyParams);
                if (PublicEnum::ResponseCode['needLogin'] == $updateRequests['code']) {
                    $common->doLogout();
                }
                //修改接口
                $roleMenuController = new RoleMenuController();
                $updateResult = $roleMenuController->update($updateRequests['request'])->getData();
                if (PublicEnum::ResponseCode['success'] == $updateResult->code) {
                    $searchParams = $this->getSearchParams();
                    $this->getDataList($searchParams);
                    session()->flash('pagePopMessage', __('web_message.menu.manager.modifyRoleMenus') . ' ' . __('web_message.general.success'));
                } else {
                    //顯示錯誤訊息POP
                    session()->flash('pagePopMessage', collect($updateResult->message ?? [])->implode('; '));
                    //恢復原本POP
                    session()->flash('popId', 'modifyRoleMenus');
                }
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

    private function getDataList($params)
    {
        $common = new Common();
        $requests = $common->getRequest($params);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $roleController = new RoleController();
        $roleList = $roleController->getList($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $roleList->code) {
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $pageCacheData['pageData'] = $roleList->data->list ?? [];
            $pageCacheData['totalPage'] = $roleList->data->pagination->pages ?? PublicEnum::DefaultPage;
            $pageCacheData['totalRow'] = $roleList->data->pagination->total ?? 0;
            session()->put('nowPage', $roleList->data->pagination->current ?? PublicEnum::DefaultPage);
            session()->put('pageCacheData', $pageCacheData);
        } else {
            session()->flash('pagePopMessage', collect($roleList->message ?? [])->implode('; '));
        }
    }

    private function getMenuGroupList($role_id = 0)
    {
        $common = new Common();
        $requests = $common->getRequest([]);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $menuController = new MenuController();
        $groupList = $menuController->getMenuGroupList($requests['request'])->getData();
        //塞進session
        $pageCacheData = session()->get('pageCacheData') ?? [];
        $pageCacheData['accessGroupList'] = $groupList->data->list ?? [];
        session()->put('pageCacheData', $pageCacheData);
    }

    private function getRoleMenuIds($role_id = 0)
    {
        if (empty($role_id)) {
            return [];
        }
        $common = new Common();
        $requests = $common->getRequest(['role_id' => $role_id]);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $roleMenuController = new RoleMenuController();
        $accessList = $roleMenuController->getAccessList($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $accessList->code) {
            return collect($accessList->data->list ?? [])->pluck('menu_id')->toArray();
        } else {
            return [];
        }
    }
}
