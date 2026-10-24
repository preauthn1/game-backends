<?php

namespace App\Services;

use App\Repositories\OrganizationRepository;

class OrganizationService extends BaseService
{
    public function __construct()
    {
        $this->repository = new OrganizationRepository();
    }

    public function getDropdown()
    {
        return $this->repository->getDropdown();
    }
}
