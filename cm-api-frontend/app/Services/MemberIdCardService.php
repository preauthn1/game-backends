<?php

namespace App\Services;

use App\Repositories\MemberIdcardRepository;
use Exception;

class MemberIdCardService extends BaseService
{
    public function __construct()
    {
        $this->repository = new MemberIdcardRepository();
    }

    public function find($id)
    {
        $result = $this->repository->find($id);
        if (!is_object($result)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        return $result;
    }
}