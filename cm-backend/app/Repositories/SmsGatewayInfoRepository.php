<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\SmsGatewayInfo;
use DB;

/**
 * Class SmsGatewayInfoRepository.
 */
class SmsGatewayInfoRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return SmsGatewayInfo::class;
    }
}
