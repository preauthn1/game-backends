<?php

namespace App\Services;

use App\Repositories\LanguageRepository;
use Exception;

class LanguageService extends BaseService
{
    public function __construct()
    {
        $this->repository = new LanguageRepository();
    }

    public function getLanguageIdByCode($code)
    {
        $model = $this->repository->getByCode($code);
        return is_object($model) ? $model->id : 1;
    }

    public function getDropdown()
    {
        return $this->repository->getDropdown();
    }
}
