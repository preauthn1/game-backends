<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\SmsGatewayInfoCountry;
use DB;

/**
 * Class SmsGatewayInfoCountryRepository.
 */
class SmsGatewayInfoCountryRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return SmsGatewayInfoCountry::class;
    }
}
