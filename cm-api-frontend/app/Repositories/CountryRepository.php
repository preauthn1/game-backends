<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\Country;
use Illuminate\Support\Facades\DB;

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

    public function getList($domain)
    {
        return $this->model->where('status', 1)
            ->select(
                'id as country_id',
                'name as country_name',
                'country_code',
                'phone_rule'
            )
            ->orderBy('sort', 'desc')
            ->get();
    }

    public function getListBySms($sms_gateway_group_id)
    {
        return DB::table('countries as c')
            ->join('sms_gateway_info_countries as sgic', 'c.id', 'sgic.country_id')
            ->join('sms_gateway_infos as sgi', function ($join) use ($sms_gateway_group_id) {
                $join->on('sgi.id', 'sgic.sms_gateway_info_id')
                    ->where('sgi.status', 1)
                    ->where('sgi.agent_group_id', $sms_gateway_group_id);
            })
            ->select(
                'c.id as country_id',
                'c.name as country_name',
                'c.country_code',
                'c.phone_rule'
            )
            ->orderBy('c.sort', 'desc')
            ->get();
    }
}
