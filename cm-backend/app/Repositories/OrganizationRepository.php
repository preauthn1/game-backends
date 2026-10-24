<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\Organization;

class OrganizationRepository extends BaseRepository
{
    public function model()
    {
        return Organization::class;
    }

    public function getDropdown()
    {
        return $this->model
            ->select(
                'id as organization_id',
                'name as organization_name',
            )
            ->orderBy('id')
            ->get();
    }
}
