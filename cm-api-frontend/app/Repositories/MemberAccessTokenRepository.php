<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\MemberAccessToken;
use DB;

/**
 * Class MemberAccessTokenRepository.
 */
class MemberAccessTokenRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return MemberAccessToken:[REDACTED];
    }

    public function findByToken($token)
    {
        return $this->model->where('token', $token)
            ->where('expire_date', '>', date('Y-m-d H:i:s'))
            ->first();
    }
}
