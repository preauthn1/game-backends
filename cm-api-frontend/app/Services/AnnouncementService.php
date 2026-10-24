<?php

namespace App\Services;

use App\Repositories\AnnouncementRepository;

/**
 * Class AnnouncementService.
 */
class AnnouncementService extends BaseService
{
    public function getList($member_id, $type)
    {
        $this->repository = new AnnouncementRepository();
        $result = $this->repository->getList($member_id, $type);
        return $result;
    }
}
