<?php

namespace App\Services;

use App\Repositories\MemberAccessTokenRepository;
use Exception;

/**
 * Class MemberAccessTokenService.
 */
class MemberAccessTokenService extends BaseService
{
    public function __construct()
    {
        $this->repository = new MemberAccessTokenRepository();
    }

    public function checkToken($token)
    {
        $memberAccessToken = [REDACTED];
        if (!is_object($memberAccessToken)) {
            throw new Exception(__('error_message.9998'), 9998);
        }
        return $memberAccessToken;

    }
}
