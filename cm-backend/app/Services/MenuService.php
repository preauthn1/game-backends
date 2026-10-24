<?php

namespace App\Services;

use App\Repositories\MenuRepository;
use App\Repositories\AgentRepository;
use Exception;
use App\Helpers\Common;

class MenuService extends BaseService
{
    public function __construct()
    {
        $this->repository = new MenuRepository();
    }

    public function getParentMenuDropdown()
    {
        $result = $this->repository->getParentMenuDropdown();
        return $result->map(function ($item) {
            $item->name = __('web_message.menu.' . $item->name . '.title');
            return $item;
        });
    }

    public function getList($params)
    {
        $page = isset($params->page) ? $params->page : 1;
        $per_page = isset($params->per_page) ? $params->per_page : 10;
        $model = $this->repository->getList($params);
        $total = $model->count();
        $result = $model->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();

        $result = $result->map(function ($item) {
            $item->pNameTxt = (0 == $item->parent_id) ? __('web_message.menuMgmt.mainMenu') : __('web_message.menu.' . $item->parent_name . '.title');
            $item->nameTxt = (0 == $item->parent_id) ? __('web_message.menu.' . $item->name . '.title') : __('web_message.menu.' . $item->parent_name . '.' . $item->name);
            return $item;
        });
        return [$total, $result];
    }

    public function getGroupList($params)
    {
        $result = [];
        $list = $this->repository->getList($params)->get();
        foreach ($list as $item) {
            $parentId = $item->parent_id;
            $item->pNameTxt = (0 == $parentId) ? __('web_message.menuMgmt.mainMenu') : __('web_message.menu.' . $item->parent_name . '.title');
            $item->nameTxt = (0 == $parentId) ? __('web_message.menu.' . $item->name . '.title') : __('web_message.menu.' . $item->parent_name . '.' . $item->name);
            if (0 == $parentId) {
                if (empty($result[$item->id] ?? [])) {
                    $result[$item->id] = [
                        'id' => $item->id,
                        'name' => $item->name,
                        'nameTxt' => $item->nameTxt,
                    ];
                }
            } else {
                $result[$parentId]['sub'][] = [
                    'id' => $item->id,
                    'name' => $item->name,
                    'nameTxt' => $item->nameTxt,
                ];
            }
        }
        return $result;
    }

    public function find($id)
    {
        $data = $this->repository->getById($id);
        if (!is_object($data)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        return $data;
    }

    public function create($params)
    {
        $data = array();
        $data['parent_id'] = $params->parent_id;
        $data['name'] = $params->menu_name;
        $data['order'] = $params->order;
        $data['is_show'] = $params->is_show;
        if (Common::IsNotNullOrEmptyString($params->url)) {
            $data['uri'] = $params->url;
        }
        $result = $this->repository->create($data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function update($params)
    {
        $menu = $this->find($params->menu_id);
        if (!is_object($menu)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        $data = array();
        if (Common::IsNotNullOrEmptyString($params->parent_id)) {
            $data['parent_id'] = $params->parent_id;
        }
        if (Common::IsNotNullOrEmptyString($params->menu_name)) {
            $data['name'] = $params->menu_name;
        }
        if (Common::IsNotNullOrEmptyString($params->order)) {
            $data['order'] = $params->order;
        }
        if (Common::IsNotNullOrEmptyString($params->url)) {
            $data['uri'] = $params->url;
        }
        if (isset($params->is_show)) {
            $data['is_show'] = $params->is_show;
        }
        if (count($data) > 0) {
            $result = $this->repository->updateById($params->menu_id, $data);
            if (!$result) {
                throw new Exception(__('error_message.2001'), 2001);
            }
        }
    }

    public function getAgentMenu($agent_id)
    {
        $agentRepository = new AgentRepository();
        $agent = $agentRepository->find($agent_id);

        $role_id = $agent->role_id;
        $menus = $this->repository->getAgentMenu($role_id);
        $totalSubMenus = [];

        foreach ($menus as $key => $menu) {
            $sub_menus = $this->repository->getAgentMenu($role_id, $menu->id);
            $menus[$key]->count = count($sub_menus);
            $menus[$key]->sub = $sub_menus;
            foreach ($sub_menus ?? [] as $sub) {
                $totalSubMenus[] = $sub->uri;
            }
        }
        return [$menus, $totalSubMenus];
    }
}
