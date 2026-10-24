<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\AgentLoginLog;

/**
 * Class AgentLoginLogRepository.
 */
class AgentLoginLogRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return AgentLoginLog::class;
    }
}
