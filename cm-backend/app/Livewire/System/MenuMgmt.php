<?php

namespace App\Livewire\System;

use App\Enums\PublicEnum;
use App\Livewire\Common;
use Livewire\Component;
use App\Http\Controllers\Admin\MenuController;

class MenuMgmt extends Component
{
    //搜尋
    public $parentMenuId, $menuUrl;
    //新增
    public $addParentMenuId, $addMenu, $addMenuUrl, $addMenuOrder, $addMenuIsShow = true;
    //修改
    public $modifyMenuId, $modifyParentMenuId, $modifyMenu, $modifyMenuUrl, $modifyMenuOrder, $modifyMenuIsShow;

    public function mount()
    {
        $this->getDropdown();
        $searchParams = $this->getSearchParams();
        $this->getDataList($searchParams);
    }

    public function render()
    {
        return view('livewire.system.menu-mgmt');
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
    public function addNewMenu()
    {
        //頁面傳參
        $addParams = [
            'parent_id' => $this->addParentMenuId,
            'menu_name' => $this->addMenu,
            'order' => $this->addMenuOrder,
            'is_show' => $this->addMenuIsShow ? 1 : 0,
        ];
        if (!empty($this->addMenuUrl ?? '')) {
            $addParams['url'] = $this->addMenuUrl;
        }
        //串接request
        $common = new Common();
        $requests = $common->getRequest($addParams);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        //新增接口
        $menuController = new MenuController();
        $addMenu = $menuController->create($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $addMenu->code) {
            redirect()->to('/menuMgmt')->with('pagePopMessage', __('web_message.menu.system.addMenu') . ' ' . __('web_message.general.success'));
        } else {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', collect($addMenu->message ?? [])->implode('; '));
            //恢復原本POP
            session()->flash('popId', 'addMenu');
        }
    }

    //修改POP
    public function showModify($menuId)
    {
        if (!empty($menuId)) {
            //串接request
            $common = new Common();
            $requests = $common->getRequest(['id' => $menuId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            //資訊
            $menuController = new MenuController();
            $menuData = $menuController->view($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $menuData->code) {
                $this->modifyMenuId = $menuData->data->data->id;
                $this->modifyParentMenuId = $menuData->data->data->parent_id ?? '';
                $this->modifyMenu = $menuData->data->data->name ?? '';
                $this->modifyMenuUrl = $menuData->data->data->uri ?? '';
                $this->modifyMenuOrder = $menuData->data->data->order ?? '';
                $this->modifyMenuIsShow = (1 == ($menuData->data->data->is_show ?? 1));
                //展示POP
                session()->flash('showPopId', 'modifyMenu');
            }
        }
    }

    //修改
    public function doModify()
    {
        if (!empty($this->modifyMenuId)) {
            //抓取資訊
            $common = new Common();
            $requests = $common->getRequest(['id' => $this->modifyMenuId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            $menuController = new MenuController();
            $menuData = $menuController->view($requests['request'])->getData();
            //串接修改request
            $modifyParams = [];
            if ($this->modifyParentMenuId != ($menuData->data->data->parent_id ?? '')) {
                $modifyParams['parent_id'] = $this->modifyParentMenuId;
            }
            if ($this->modifyMenu != ($menuData->data->data->name ?? '')) {
                $modifyParams['menu_name'] = $this->modifyMenu;
            }
            if ($this->modifyMenuUrl != ($menuData->data->data->uri ?? '')) {
                $modifyParams['url'] = $this->modifyMenuUrl;
            }
            if ($this->modifyMenuOrder != ($menuData->data->data->order ?? '')) {
                $modifyParams['order'] = $this->modifyMenuOrder;
            }
            if ($this->modifyMenuIsShow != (1 == ($menuData->data->data->is_show ?? 1))) {
                $modifyParams['is_show'] = $this->modifyMenuIsShow ? 1 : 0;
            }
            //無異動
            if (empty($modifyParams)) {
                //顯示錯誤訊息POP
                session()->flash('pagePopMessage', __('web_message.general.noChange'));
                //恢復原本POP
                session()->flash('popId', 'modifyMenu');
            } else {
                $modifyParams = array_merge(['menu_id' => $this->modifyMenuId], $modifyParams);
                $updateRequests = $common->getRequest($modifyParams);
                if (PublicEnum::ResponseCode['needLogin'] == $updateRequests['code']) {
                    $common->doLogout();
                }
                //修改接口
                $updateMenu = $menuController->update($updateRequests['request'])->getData();
                if (PublicEnum::ResponseCode['success'] == $updateMenu->code) {
                    $searchParams = $this->getSearchParams();
                    $this->getDataList($searchParams);
                    session()->flash('pagePopMessage', __('web_message.menu.system.modifyMenu') . ' ' . __('web_message.general.success'));
                } else {
                    //顯示錯誤訊息POP
                    session()->flash('pagePopMessage', collect($updateMenu->message ?? [])->implode('; '));
                    //恢復原本POP
                    session()->flash('popId', 'modifyMenu');
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
        $menuController = new MenuController();
        //上級選單
        $parentMenuResult = $menuController->getParentMenuDropdown($dropDownRequests['request'])->getData();
        //塞進session
        $pageCacheData = session()->get('pageCacheData') ?? [];
        $pageCacheData['parentMenuList'] = $parentMenuResult->data->list ?? [];
        session()->put('pageCacheData', $pageCacheData);
    }

    //搜尋參數串接
    private function getSearchParams(): array
    {
        $searchParams = [];
        if (-1 != ($this->parentMenuId ?? -1)) {
            $searchParams['parent_id'] = $this->parentMenuId;
        }
        if (!empty($this->menuUrl ?? '')) {
            $searchParams['uri'] = $this->menuUrl;
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
        $menuController = new MenuController();
        $menuList = $menuController->getMenuList($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $menuList->code) {
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $pageCacheData['pageData'] = $menuList->data->list ?? [];
            $pageCacheData['totalPage'] = $menuList->data->pagination->pages ?? PublicEnum::DefaultPage;
            $pageCacheData['totalRow'] = $menuList->data->pagination->total ?? 0;
            session()->put('nowPage', $menuList->data->pagination->current ?? PublicEnum::DefaultPage);
            session()->put('pageCacheData', $pageCacheData);
        } else {
            session()->flash('pagePopMessage', collect($menuList->message ?? [])->implode('; '));
        }
    }
}
