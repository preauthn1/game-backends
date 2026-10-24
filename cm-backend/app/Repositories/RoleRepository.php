<?php

namespace App\Repositories;

use App\Helpers\Common;
use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\Role;

class RoleRepository extends BaseRepository
{
    public function model()
    {
        return Role::class;
    }

    public function getDropdown($params)
    {
        return $this->model
            ->select(
                'id as role_id',
                'name as role_name',
            )
            ->when(Common::IsNotNullOrEmptyString($params->ORGANIZATION_ID), function ($query) use ($params) {
                $query->where('organization_id', $params->ORGANIZATION_ID);
            })
            ->orderBy('id')
            ->get();
    }

    public function getList($params)
    {
        return $this->model
            ->when(Common::IsNotNullOrEmptyString($params->ORGANIZATION_ID), function ($query) use ($params) {
                $query->where('organization_id', $params->ORGANIZATION_ID);
            })
            ->select(
                'id',
                'name',
                'created_at',
            );
    }
}
