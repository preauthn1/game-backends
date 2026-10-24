<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\SmsGateway;
use DB;

/**
 * Class SmsGatewayRepository.
 */
class SmsGatewayRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return SmsGateway::class;
    }

    public function getSendList($agent_group_id, $mobile)
    {
        $model = DB::table('sms_gateway_infos as sgi')
            ->join('sms_gateways as sg', function ($join) {
                $join->on('sg.id', 'sgi.sms_gateway_id')
                    ->where('sg.status', 1);
            })
            ->join('sms_gateway_info_countries as sgic', function ($join) {
                $join->on('sgic.sms_gateway_info_id', 'sgi.id')
                    ->where('sgic.status', 1);
            })
            ->join('countries as c', function ($join) use ($mobile) {
                $join->on('c.id', 'sgic.country_id')
                    ->where('c.status', 1)
                    ->whereRaw("'" . $mobile . "' like concat(c.country_code, '%')");
            })
            ->where('sgi.status', 1)
            ->where('sgi.agent_group_id', $agent_group_id)
            ->groupBy('sgi.id')
            ->orderBy('sgi.sort', 'ASC')
            ->select(
                'sg.id as sms_gateway_id',
                'sg.code',
                'sg.url',
                'example.invalid',
                'c.id as country_id',
                'c.country_code'
            )
            ->get();
        return $model;
    }
}
