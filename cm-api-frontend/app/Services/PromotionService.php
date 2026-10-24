<?php

namespace App\Services;

use App\Enums\PromotionEnum;
use App\Repositories\PromotionRepository;
use Exception;

class PromotionService extends BaseService
{
    public function __construct()
    {
        $this->repository = new PromotionRepository();
    }

    public function getList($miner_group_id, $language_id, $type = null)
    {
        $list = $this->repository->getList($miner_group_id, $language_id, $type);
        return $list;
    }
}
