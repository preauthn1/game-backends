<?php

namespace App\Services;

use App\Enums\TimeEnum;
use App\Helpers\Common;
use App\Repositories\RoleRepository;
use Carbon\Carbon;

class RoleService extends BaseService
{
    public function __construct()
    {
        $this->repository = new RoleRepository();
    }

    public function getDropdown($params)
    {
        return $this->repository->getDropdown($params);
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
            //轉換為 +8 時區並格式化為 'Y-m-d H:i'
            $item->created_time = Carbon::parse($item->created_at, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d H:i');
            return $item;
        });
        return [$total, $result];
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
        $data['name'] = $params->role_name;
        $data['organization_id'] = empty($params->ORGANIZATION_ID ?? '') ? 1 : $params->ORGANIZATION_ID;
        $result = $this->repository->create($data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function update($params)
    {
        $role = $this->find($params->role_id);
        if (!is_object($role)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        $data = array();
        if (Common::IsNotNullOrEmptyString($params->role_name)) {
            $data['name'] = $params->role_name;
        }
        if (count($data) > 0) {
            $result = $this->repository->updateById($params->role_id, $data);
            if (!$result) {
                throw new Exception(__('error_message.2001'), 2001);
            }
        }
    }
}
