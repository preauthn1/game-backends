<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\AgentAccessToken;

/**
 * Class AgentAccessTokenRepository.
 */
class AgentAccessTokenRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return AgentAccessToken:[REDACTED];
    }

    public function findByToken($token)
    {
        return $this->model->where('token', $token)
            ->where('expire_date', '>', date('Y-m-d H:i:s'))
            ->first();
    }

    public function setDisable($agent_id)
    {
        // 將舊TOKEN改為無效
        $now = date('Y-m-d H:i:s');
        return $this->model->where('agent_id', $agent_id)
            ->where('expire_date', '>', $now)
            ->update([
                'expire_date' => $now
            ]);
    }
}
