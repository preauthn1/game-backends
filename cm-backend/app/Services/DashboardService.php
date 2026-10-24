<?php

namespace App\Services;

use Exception;

class DashboardService extends BaseService
{
    public function __construct()
    {
        $this->repository = new CurrencyRepository();
    }


}