<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\CurrencyLog;

/**
 * Class CurrencyLogRepository.
 */
class CurrencyLogRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return CurrencyLog::class;
    }
}
