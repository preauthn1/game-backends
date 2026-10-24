<?php

namespace App\Services;

use App\Helpers\Common;
use App\Repositories\RoleMenuRepository;
use App\Repositories\RoleRepository;

class RoleMenuService extends BaseService
{
    public function __construct()
    {
        $this->repository = new RoleMenuRepository();
    }

    public function getAccessList($params)
    {
        return $this->repository->getAccessList($params)->get();
    }

    public function update($params)
    {
        $roleRoleRepository = new RoleRepository();
        $role = $roleRoleRepository->getById($params->role_id);
        if (!is_object($role)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        $insertDataList = [];
        if (Common::IsNotNullOrEmptyString($params->add_menu_ids)) {
            $addIds = explode(';', $params->add_menu_ids);
            foreach ($addIds ?? [] as $addId) {
                if (!empty($addId ?? '')) {
                    $insertDataList[] = [
                        'role_id' => $params->role_id,
                        'menu_id' => (int)$addId,
                    ];
                }
            }
        }
        $deleteDataList = [];
        if (Common::IsNotNullOrEmptyString($params->sub_menu_ids)) {
            $deleteIdList = explode(';', $params->sub_menu_ids);
            if (!empty($deleteIdList ?? [])) {
                $deleteDataList = [
                    'role_id' => $params->role_id,
                    'menu_ids' => $deleteIdList,
                ];
            }
        }
        if (count($insertDataList) > 0) {
            $resultInsert = $this->repository->multiInsertMenu($insertDataList);
            if (!$resultInsert) {
                throw new Exception(__('error_message.2001'), 2001);
            }
        }
        if (count($deleteDataList) > 0) {
            $resultDelete = $this->repository->multiDeleteMenu($deleteDataList);
            if (!$resultDelete) {
                throw new Exception(__('error_message.2001'), 2001);
            }
        }
    }
}
