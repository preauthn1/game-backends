<?php

namespace App\Services;

use App\Repositories\CountryRepository;

/**
 * Class CountryService.
 */
class CountryService extends BaseService
{
    public function __construct()
    {
        $this->repository = new CountryRepository();
    }

    public function getById($id)
    {
        return $this->repository->getById($id);
    }

    public function getList($domain = '')
    {
        return $this->repository->getList($domain);
    }

    public function getListBySms($sms_gateway_group_id = '')
    {
        return $this->repository->getListBySms($sms_gateway_group_id);
    }
}
