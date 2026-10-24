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
    public function __construct()
    {
        $this->repository = new AvatarRepository();
    }

    public function getList()
    {
        $result = $this->repository->getAll();
        return $result;
    }

    public function find($id)
    {
        $result = $this->repository->find($id);
        if (Common::IsNullOrEmptyString($result)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        return $result;
    }

    public function check($avatar_id)
    {
        $result = $this->repository->find(intval($avatar_id));
        if (Common::IsNullOrEmptyString($result)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        return true;
    }

}
