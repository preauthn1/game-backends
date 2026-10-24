<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\SmsGatewayCountry;
use DB;

/**
 * Class SmsGatewayCountryRepository.
 */
class SmsGatewayCountryRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return SmsGatewayCountry::class;
    }
}
