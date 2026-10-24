<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\SmsSendRecord;
use DB;

/**
 * Class SmsSendRecordRepository.
 */
class SmsSendRecordRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return SmsSendRecord::class;
    }
}
