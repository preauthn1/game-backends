<?php

namespace App\Services;

use App\Helpers\Common;
use App\Repositories\AvatarRepository;
use Exception;

/**
 * Class AvatarService.
 */
class AvatarService extends BaseService
{

    public function getList()
    {
        $this->repository = new AvatarRepository();
        $result = $this->repository->getAll();
        return $result;
    }

    public function check($avatar_id)
    {
        $this->repository = new AvatarRepository();
        $result = $this->repository->find(intval($avatar_id));
        if (Common::IsNullOrEmptyString($result)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        return true;
    }

}
