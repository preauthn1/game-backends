<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\MinerLog;

/**
 * Class MinerLogRepository.
 */
class MinerLogRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return MinerLog::class;
    }
}
