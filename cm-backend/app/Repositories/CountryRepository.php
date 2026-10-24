<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\Country;
use Illuminate\Support\Facades\DB;
use App\Helpers\Common;

/**
 * Class CountryRepository.
 */
class CountryRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return Country::class;
    }

    public function findAll()
    {
        return $this->model->get();
    }

    /**
     * 取得國別下拉選單
     */
    public function getDropdown()
    {
        return $this->model
            ->where('status', 1)
            ->select(
                'id as country_id',
                'name as country_name',
                'country_code',
                'phone_rule'
            )
            ->orderByRaw('CAST(country_code AS CHAR)')
            ->get();
    }

    public function getList(object $params)
    {
        return $this->model
            ->when(Common::IsNotNullOrEmptyString($params->isoCode), function ($query) use ($params) {
                $query->where('code', $params->isoCode);
            })
            ->when(Common::IsNotNullOrEmptyString($params->countryCode), function ($query) use ($params) {
                $query->where('country_code', $params->countryCode);
            })
            ->when(Common::IsNotNullOrEmptyString($params->status), function ($query) use ($params) {
                $query->where('status', $params->status);
            })
            ->select(
                'id',
                'code as iso_code',
                'name as country_name',
                'country_code',
                'phone_rule',
                'sort',
                'status'
            )
            ->orderBy('sort', 'desc')
            ->orderBy('id', 'desc');
    }
}
