<?php

namespace App\Services;

use App\Helpers\Common;
use App\Repositories\WalletTypeRepository;
use Exception;

/**
 * Class WalletTypeService.
 */
class WalletTypeService extends BaseService
{
    public function __construct()
    {
        $this->repository = new WalletTypeRepository();
    }

    public function getByLanguageId($language_id = 1)
    {
        $walletType = $this->repository->getByLanguageId($language_id);
        return $walletType;
    }
}
